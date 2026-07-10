using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using Dapper;
using Microsoft.Data.SqlClient;
using Supermarket.Models.Entities;

namespace Supermarket.DAL
{
    public class IntegratedPurchaseRepository
    {
        private readonly string _connectionString;

        public IntegratedPurchaseRepository(string connectionString)
        {
            _connectionString = connectionString;
        }

        public async Task<IEnumerable<dynamic>> GetPurchaseInvoicesAsync()
        {
            using (IDbConnection db = new SqlConnection(_connectionString))
            {
                string sql = @"SELECT p.*, s.SupplierName, st.StoreName
                               FROM PurchaseInvoices p
                               JOIN Suppliers s ON p.SupplierID = s.SupplierID
                               JOIN Stores st ON p.StoreID = st.StoreID
                               ORDER BY p.InvoiceDate DESC";
                return await db.QueryAsync(sql);
            }
        }

        public async Task<PurchaseInvoice> GetPurchaseInvoiceHeaderAsync(int id)
        {
            using (IDbConnection db = new SqlConnection(_connectionString))
            {
                return await db.QueryFirstOrDefaultAsync<PurchaseInvoice>("SELECT * FROM PurchaseInvoices WHERE PurchaseID = @id", new { id });
            }
        }

        public async Task<IEnumerable<dynamic>> GetPurchaseInvoiceItemsAsync(int id)
        {
            using (IDbConnection db = new SqlConnection(_connectionString))
            {
                string sql = @"SELECT pi.*, i.ItemName, i.Barcode, i.Unit, i.PurchaseUnit, i.ConversionFactor
                               FROM PurchaseInvoiceItems pi
                               JOIN Items i ON pi.ItemID = i.ItemID
                               WHERE pi.PurchaseID = @id";
                return await db.QueryAsync(sql, new { id });
            }
        }

        public async Task<decimal> GetSupplierBalanceAsync(int supplierId)
        {
            using (IDbConnection db = new SqlConnection(_connectionString))
            {
                string sql = @"SELECT ISNULL(SUM(d.Credit - d.Debit), 0)
                               FROM JournalEntryDetails d
                               JOIN JournalEntries e ON d.JournalID = e.JournalID
                               JOIN ChartOfAccounts a ON d.AccountID = a.AccountID
                               WHERE a.AccountNumber = '2102' AND e.Description LIKE '%' + (SELECT SupplierName FROM Suppliers WHERE SupplierID = @supplierId) + '%'";
                return await db.QuerySingleAsync<decimal>(sql, new { supplierId });
            }
        }

        public async Task<int> SavePurchaseInvoiceAsync(PurchaseInvoice invoice, List<PurchaseInvoiceItem> items)
        {
            using (IDbConnection db = new SqlConnection(_connectionString))
            {
                db.Open();
                using (var transaction = db.BeginTransaction())
                {
                    try
                    {
                        // Calculate Landed Cost Distribution Factor (Other Charges / Total Net before Charges)
                        decimal baseNet = items.Sum(i => i.TotalAmount);
                        decimal landingFactor = baseNet > 0 ? invoice.OtherCharges / baseNet : 0;

                        // 1. Save Invoice Header
                        string invoiceSql = @"INSERT INTO PurchaseInvoices (InvoiceNumber, SupplierID, StoreID, TotalAmount, TaxAmount, DiscountAmount, OtherCharges, NetAmount, PaymentType, CreatedBy)
                                              VALUES (@InvoiceNumber, @SupplierID, @StoreID, @TotalAmount, @TaxAmount, @DiscountAmount, @OtherCharges, @NetAmount, @PaymentType, @CreatedBy);
                                              SELECT CAST(SCOPE_IDENTITY() as int)";
                        int purchaseId = await db.QuerySingleAsync<int>(invoiceSql, invoice, transaction);

                        foreach (var item in items)
                        {
                            item.PurchaseID = purchaseId;

                            // Fetch unit conversion factor
                            var itemMaster = await db.QueryFirstOrDefaultAsync<Item>("SELECT * FROM Items WHERE ItemID = @ItemID", new { item.ItemID }, transaction);
                            decimal conversion = (item.SelectedUnit == itemMaster.PurchaseUnit) ? itemMaster.ConversionFactor : 1.0m;
                            decimal baseQuantity = item.Quantity * conversion;
                            decimal unitCostWithLanded = (item.TotalAmount * (1 + landingFactor)) / baseQuantity;

                            // 2. Save Invoice Items
                            string itemSql = @"INSERT INTO PurchaseInvoiceItems (PurchaseID, ItemID, Quantity, UnitPrice, DiscountRate, DiscountAmount, TaxAmount, TotalAmount, ExpiryDate)
                                                VALUES (@PurchaseID, @ItemID, @Quantity, @UnitPrice, @DiscountRate, @DiscountAmount, @TaxAmount, @TotalAmount, @ExpiryDate)";
                            await db.ExecuteAsync(itemSql, item, transaction);

                            // 2.5 Record Movement in Base Units
                            await db.ExecuteAsync("INSERT INTO StockMovement (ItemID, StoreID, MovementType, Quantity, ReferenceID, Notes) VALUES (@ItemID, @StoreID, 'Purchase', @Quantity, @purchaseId, @Notes)",
                                new { item.ItemID, invoice.StoreID, Quantity = baseQuantity, purchaseId, Notes = $"Unit: {item.SelectedUnit}, Expiry: {item.ExpiryDate?.ToShortDateString()}" }, transaction);

                            // 3. Update Weighted Average Cost (including Landed Cost)
                            string updateStockSql = @"
                                DECLARE @OldQty DECIMAL(18,2) = 0, @OldCost DECIMAL(18,2) = 0
                                SELECT @OldQty = ISNULL(SUM(Quantity), 0) FROM Stock WHERE ItemID = @ItemID
                                SELECT @OldCost = ISNULL(CostPrice, 0) FROM Items WHERE ItemID = @ItemID

                                UPDATE Items SET CostPrice = ((@OldQty * @OldCost) + (@BaseQty * @NewCost)) / NULLIF(@OldQty + @BaseQty, 0)
                                WHERE ItemID = @ItemID;

                                IF EXISTS (SELECT 1 FROM Stock WHERE ItemID = @ItemID AND StoreID = @StoreID AND (ExpiryDate = @ExpiryDate OR (ExpiryDate IS NULL AND @ExpiryDate IS NULL)))
                                    UPDATE Stock SET Quantity = Quantity + @BaseQty WHERE ItemID = @ItemID AND StoreID = @StoreID AND (ExpiryDate = @ExpiryDate OR (ExpiryDate IS NULL AND @ExpiryDate IS NULL))
                                ELSE
                                    INSERT INTO Stock (ItemID, StoreID, Quantity, ExpiryDate) VALUES (@ItemID, @StoreID, @BaseQty, @ExpiryDate)";

                            await db.ExecuteAsync(updateStockSql, new { item.ItemID, BaseQty = baseQuantity, NewCost = unitCostWithLanded, invoice.StoreID, item.ExpiryDate }, transaction);
                        }

                        // 4. Accounting
                        string supplierName = await db.QuerySingleAsync<string>("SELECT SupplierName FROM Suppliers WHERE SupplierID = @id", new { id = invoice.SupplierID }, transaction);
                        int journalId = await db.QuerySingleAsync<int>(
                            "INSERT INTO JournalEntries (Description, CreatedBy) VALUES (@Desc, @User); SELECT CAST(SCOPE_IDENTITY() as int)",
                            new { Desc = $"Purchase Invoice: {invoice.InvoiceNumber} - {supplierName}", User = invoice.CreatedBy }, transaction);

                        var accounts = await db.QueryAsync<dynamic>("SELECT AccountID, AccountNumber FROM ChartOfAccounts WHERE AccountNumber IN ('1201', '1101', '1202', '2102')", null, transaction);
                        int inventoryAcc = accounts.First(a => a.AccountNumber == "1201").AccountID;
                        int cashAcc = accounts.First(a => a.AccountNumber == "1101").AccountID;
                        int inputVatAcc = accounts.First(a => a.AccountNumber == "1202").AccountID;
                        int supplierAcc = accounts.First(a => a.AccountNumber == "2102").AccountID;

                        await db.ExecuteAsync("INSERT INTO JournalEntryDetails (JournalID, AccountID, Debit, Credit) VALUES (@jid, @acc, @amt, 0)",
                            new { jid = journalId, acc = inventoryAcc, amt = invoice.NetAmount - invoice.TaxAmount }, transaction);

                        if (invoice.TaxAmount > 0)
                        {
                            await db.ExecuteAsync("INSERT INTO JournalEntryDetails (JournalID, AccountID, Debit, Credit) VALUES (@jid, @acc, @amt, 0)",
                                new { jid = journalId, acc = inputVatAcc, amt = invoice.TaxAmount }, transaction);
                        }

                        int creditAcc = invoice.PaymentType == "Credit" ? supplierAcc : cashAcc;
                        await db.ExecuteAsync("INSERT INTO JournalEntryDetails (JournalID, AccountID, Debit, Credit) VALUES (@jid, @acc, 0, @amt)",
                            new { jid = journalId, acc = creditAcc, amt = invoice.NetAmount }, transaction);

                        transaction.Commit();
                        return purchaseId;
                    }
                    catch { transaction.Rollback(); throw; }
                }
            }
        }

        public async Task<int> SavePurchaseReturnAsync(PurchaseReturn ret, List<PurchaseReturnItem> items)
        {
            using (IDbConnection db = new SqlConnection(_connectionString))
            {
                db.Open();
                using (var transaction = db.BeginTransaction())
                {
                    try
                    {
                        string sql = @"INSERT INTO PurchaseReturns (ReturnNumber, OriginalPurchaseID, SupplierID, StoreID, TotalAmount, TaxAmount, NetAmount, CreatedBy)
                                       VALUES (@ReturnNumber, @OriginalPurchaseID, @SupplierID, @StoreID, @TotalAmount, @TaxAmount, @NetAmount, @CreatedBy);
                                       SELECT CAST(SCOPE_IDENTITY() as int)";
                        int returnId = await db.QuerySingleAsync<int>(sql, ret, transaction);

                        foreach (var item in items)
                        {
                            item.ReturnID = returnId;
                            await db.ExecuteAsync("INSERT INTO PurchaseReturnItems (ReturnID, ItemID, Quantity, UnitPrice, TaxAmount, TotalAmount) VALUES (@ReturnID, @ItemID, @Quantity, @UnitPrice, @TaxAmount, @TotalAmount)", item, transaction);
                            await db.ExecuteAsync("UPDATE Stock SET Quantity = Quantity - @Quantity WHERE ItemID = @ItemID AND StoreID = @StoreID", new { item.ItemID, ret.StoreID, item.Quantity }, transaction);
                            await db.ExecuteAsync("INSERT INTO StockMovement (ItemID, StoreID, MovementType, Quantity, ReferenceID) VALUES (@ItemID, @StoreID, 'Return', @Quantity, @returnId)", new { item.ItemID, ret.StoreID, Quantity = -item.Quantity, returnId }, transaction);
                        }

                        string supplierName = await db.QuerySingleAsync<string>("SELECT SupplierName FROM Suppliers WHERE SupplierID = @id", new { id = ret.SupplierID }, transaction);
                        int journalId = await db.QuerySingleAsync<int>("INSERT INTO JournalEntries (Description, CreatedBy) VALUES (@Desc, @User); SELECT CAST(SCOPE_IDENTITY() as int)", new { Desc = $"Purchase Return: {ret.ReturnNumber} - {supplierName}", User = ret.CreatedBy }, transaction);

                        var accounts = await db.QueryAsync<dynamic>("SELECT AccountID, AccountNumber FROM ChartOfAccounts WHERE AccountNumber IN ('1201', '1202', '2102')", null, transaction);
                        int inventoryAcc = accounts.First(a => a.AccountNumber == "1201").AccountID;
                        int inputVatAcc = accounts.First(a => a.AccountNumber == "1202").AccountID;
                        int supplierAcc = accounts.First(a => a.AccountNumber == "2102").AccountID;

                        await db.ExecuteAsync("INSERT INTO JournalEntryDetails (JournalID, AccountID, Debit, Credit) VALUES (@jid, @acc, @amt, 0)", new { jid = journalId, acc = supplierAcc, amt = ret.NetAmount }, transaction);
                        await db.ExecuteAsync("INSERT INTO JournalEntryDetails (JournalID, AccountID, Debit, Credit) VALUES (@jid, @acc, 0, @amt)", new { jid = journalId, acc = inventoryAcc, amt = ret.TotalAmount }, transaction);
                        if (ret.TaxAmount > 0) await db.ExecuteAsync("INSERT INTO JournalEntryDetails (JournalID, AccountID, Debit, Credit) VALUES (@jid, @acc, 0, @amt)", new { jid = journalId, acc = inputVatAcc, amt = ret.TaxAmount }, transaction);

                        transaction.Commit();
                        return returnId;
                    }
                    catch { transaction.Rollback(); throw; }
                }
            }
        }

        public async Task DeletePurchaseInvoiceAsync(int id)
        {
            using (IDbConnection db = new SqlConnection(_connectionString))
            {
                db.Open();
                using (var transaction = db.BeginTransaction())
                {
                    try
                    {
                        var items = await db.QueryAsync<PurchaseInvoiceItem>("SELECT * FROM PurchaseInvoiceItems WHERE PurchaseID = @id", new { id }, transaction);
                        var invoice = await db.QueryFirstOrDefaultAsync<PurchaseInvoice>("SELECT * FROM PurchaseInvoices WHERE PurchaseID = @id", new { id }, transaction);
                        foreach (var item in items)
                        {
                            var itemMaster = await db.QueryFirstOrDefaultAsync<Item>("SELECT * FROM Items WHERE ItemID = @ItemID", new { item.ItemID }, transaction);
                            decimal conversion = (item.SelectedUnit == itemMaster.PurchaseUnit) ? itemMaster.ConversionFactor : 1.0m;
                            decimal baseQuantity = item.Quantity * conversion;
                            await db.ExecuteAsync("UPDATE Stock SET Quantity = Quantity - @Quantity WHERE ItemID = @ItemID AND StoreID = @StoreID AND (ExpiryDate = @ExpiryDate OR (ExpiryDate IS NULL AND @ExpiryDate IS NULL))", new { item.ItemID, invoice.StoreID, Quantity = baseQuantity, item.ExpiryDate }, transaction);
                        }
                        string desc = "Purchase Invoice: " + invoice.InvoiceNumber + "%";
                        var journalId = await db.QueryFirstOrDefaultAsync<int?>("SELECT JournalID FROM JournalEntries WHERE Description LIKE @desc", new { desc }, transaction);
                        if (journalId.HasValue)
                        {
                            await db.ExecuteAsync("DELETE FROM JournalEntryDetails WHERE JournalID = @jid", new { jid = journalId.Value }, transaction);
                            await db.ExecuteAsync("DELETE FROM JournalEntries WHERE JournalID = @jid", new { jid = journalId.Value }, transaction);
                        }
                        await db.ExecuteAsync("DELETE FROM StockMovement WHERE ReferenceID = @id AND MovementType = 'Purchase'", new { id }, transaction);
                        await db.ExecuteAsync("DELETE FROM PurchaseInvoiceItems WHERE PurchaseID = @id", new { id }, transaction);
                        await db.ExecuteAsync("DELETE FROM PurchaseInvoices WHERE PurchaseID = @id", new { id }, transaction);
                        transaction.Commit();
                    }
                    catch { transaction.Rollback(); throw; }
                }
            }
        }
    }
}
