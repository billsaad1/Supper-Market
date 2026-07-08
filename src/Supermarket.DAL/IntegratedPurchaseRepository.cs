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
                string sql = @"SELECT pi.*, i.ItemName, i.Barcode
                               FROM PurchaseInvoiceItems pi
                               JOIN Items i ON pi.ItemID = i.ItemID
                               WHERE pi.PurchaseID = @id";
                return await db.QueryAsync(sql, new { id });
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
                            // 1. Revert Stock
                            await db.ExecuteAsync("UPDATE Stock SET Quantity = Quantity - @Quantity WHERE ItemID = @ItemID AND StoreID = @StoreID",
                                new { item.ItemID, invoice.StoreID, item.Quantity }, transaction);

                            // 2. Remove Movement
                            await db.ExecuteAsync("DELETE FROM StockMovement WHERE ReferenceID = @id AND MovementType = 'Purchase'", new { id }, transaction);
                        }

                        // 3. Delete accounting entries (simplified: find by description)
                        string desc = "Purchase Invoice: " + invoice.InvoiceNumber;
                        var journalId = await db.QueryFirstOrDefaultAsync<int?>("SELECT JournalID FROM JournalEntries WHERE Description = @desc", new { desc }, transaction);
                        if (journalId.HasValue)
                        {
                            await db.ExecuteAsync("DELETE FROM JournalEntryDetails WHERE JournalID = @jid", new { jid = journalId.Value }, transaction);
                            await db.ExecuteAsync("DELETE FROM JournalEntries WHERE JournalID = @jid", new { jid = journalId.Value }, transaction);
                        }

                        // 4. Delete Invoice
                        await db.ExecuteAsync("DELETE FROM PurchaseInvoiceItems WHERE PurchaseID = @id", new { id }, transaction);
                        await db.ExecuteAsync("DELETE FROM PurchaseInvoices WHERE PurchaseID = @id", new { id }, transaction);

                        transaction.Commit();
                    }
                    catch { transaction.Rollback(); throw; }
                }
            }
        }

        public async Task UpdatePurchaseInvoiceAsync(PurchaseInvoice invoice, List<PurchaseInvoiceItem> items)
        {
            using (IDbConnection db = new SqlConnection(_connectionString))
            {
                db.Open();
                using (var transaction = db.BeginTransaction())
                {
                    try
                    {
                        // 1. Revert previous state (simplified: similar to delete but keeping header)
                        var oldItems = await db.QueryAsync<PurchaseInvoiceItem>("SELECT * FROM PurchaseInvoiceItems WHERE PurchaseID = @id", new { id = invoice.PurchaseID }, transaction);
                        var oldInv = await db.QueryFirstOrDefaultAsync<PurchaseInvoice>("SELECT * FROM PurchaseInvoices WHERE PurchaseID = @id", new { id = invoice.PurchaseID }, transaction);

                        foreach (var item in oldItems)
                        {
                            await db.ExecuteAsync("UPDATE Stock SET Quantity = Quantity - @Quantity WHERE ItemID = @ItemID AND StoreID = @StoreID",
                                new { item.ItemID, oldInv.StoreID, item.Quantity }, transaction);
                        }

                        await db.ExecuteAsync("DELETE FROM PurchaseInvoiceItems WHERE PurchaseID = @id", new { id = invoice.PurchaseID }, transaction);
                        await db.ExecuteAsync("DELETE FROM StockMovement WHERE ReferenceID = @id AND MovementType = 'Purchase'", new { id = invoice.PurchaseID }, transaction);

                        // 2. Update Header
                        string updateSql = @"UPDATE PurchaseInvoices SET SupplierID=@SupplierID, StoreID=@StoreID, TotalAmount=@TotalAmount,
                                             TaxAmount=@TaxAmount, NetAmount=@NetAmount, PaymentType=@PaymentType WHERE PurchaseID=@PurchaseID";
                        await db.ExecuteAsync(updateSql, invoice, transaction);

                        // 3. Re-insert items and update stock (same logic as save)
                        foreach (var item in items)
                        {
                            item.PurchaseID = invoice.PurchaseID;
                            await db.ExecuteAsync("INSERT INTO PurchaseInvoiceItems (PurchaseID, ItemID, Quantity, UnitPrice, TaxAmount, TotalAmount) VALUES (@PurchaseID, @ItemID, @Quantity, @UnitPrice, @TaxAmount, @TotalAmount)", item, transaction);
                            await db.ExecuteAsync("INSERT INTO StockMovement (ItemID, StoreID, MovementType, Quantity, ReferenceID) VALUES (@ItemID, @StoreID, 'Purchase', @Quantity, @PurchaseID)", new { item.ItemID, invoice.StoreID, item.Quantity, invoice.PurchaseID }, transaction);

                            string updateStockSql = @"
                                IF EXISTS (SELECT 1 FROM Stock WHERE ItemID = @ItemID AND StoreID = @StoreID)
                                    UPDATE Stock SET Quantity = Quantity + @Quantity WHERE ItemID = @ItemID AND StoreID = @StoreID
                                ELSE
                                    INSERT INTO Stock (ItemID, StoreID, Quantity) VALUES (@ItemID, @StoreID, @Quantity)";
                            await db.ExecuteAsync(updateStockSql, new { item.ItemID, item.Quantity, invoice.StoreID }, transaction);
                        }

                        // 4. Update accounting (simplified: delete and recreate)
                        string desc = "Purchase Invoice: " + oldInv.InvoiceNumber;
                        var journalId = await db.QueryFirstOrDefaultAsync<int?>("SELECT JournalID FROM JournalEntries WHERE Description = @desc", new { desc }, transaction);
                        if (journalId.HasValue)
                        {
                            await db.ExecuteAsync("DELETE FROM JournalEntryDetails WHERE JournalID = @jid", new { jid = journalId.Value }, transaction);

                            var accounts = await db.QueryAsync<dynamic>("SELECT AccountID, AccountNumber FROM ChartOfAccounts WHERE AccountNumber IN ('1201', '1101', '1202', '2102')", null, transaction);
                            int inventoryAcc = accounts.First(a => a.AccountNumber == "1201").AccountID;
                            int cashAcc = accounts.First(a => a.AccountNumber == "1101").AccountID;
                            int inputVatAcc = accounts.First(a => a.AccountNumber == "1202").AccountID;
                            int supplierAcc = accounts.First(a => a.AccountNumber == "2102").AccountID;

                            await db.ExecuteAsync("INSERT INTO JournalEntryDetails (JournalID, AccountID, Debit, Credit) VALUES (@jid, @acc, @amt, 0)", new { jid = journalId, acc = inventoryAcc, amt = invoice.TotalAmount }, transaction);
                            if (invoice.TaxAmount > 0) await db.ExecuteAsync("INSERT INTO JournalEntryDetails (JournalID, AccountID, Debit, Credit) VALUES (@jid, @acc, @amt, 0)", new { jid = journalId, acc = inputVatAcc, amt = invoice.TaxAmount }, transaction);
                            int creditAcc = invoice.PaymentType == "Credit" ? supplierAcc : cashAcc;
                            await db.ExecuteAsync("INSERT INTO JournalEntryDetails (JournalID, AccountID, Debit, Credit) VALUES (@jid, @acc, 0, @amt)", new { jid = journalId, acc = creditAcc, amt = invoice.NetAmount }, transaction);
                        }

                        transaction.Commit();
                    }
                    catch { transaction.Rollback(); throw; }
                }
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
                        // 1. Save Invoice Header
                        string invoiceSql = @"INSERT INTO PurchaseInvoices (InvoiceNumber, SupplierID, StoreID, TotalAmount, TaxAmount, NetAmount, CreatedBy)
                                              VALUES (@InvoiceNumber, @SupplierID, @StoreID, @TotalAmount, @TaxAmount, @NetAmount, @CreatedBy);
                                              SELECT CAST(SCOPE_IDENTITY() as int)";
                        int purchaseId = await db.QuerySingleAsync<int>(invoiceSql, invoice, transaction);

                        foreach (var item in items)
                        {
                            item.PurchaseID = purchaseId;
                            // 2. Save Invoice Items
                            string itemSql = @"INSERT INTO PurchaseInvoiceItems (PurchaseID, ItemID, Quantity, UnitPrice, TaxAmount, TotalAmount)
                                                VALUES (@PurchaseID, @ItemID, @Quantity, @UnitPrice, @TaxAmount, @TotalAmount)";
                            await db.ExecuteAsync(itemSql, item, transaction);

                            // 2.5 Record Movement
                            await db.ExecuteAsync("INSERT INTO StockMovement (ItemID, StoreID, MovementType, Quantity, ReferenceID) VALUES (@ItemID, @StoreID, 'Purchase', @Quantity, @purchaseId)",
                                new { item.ItemID, invoice.StoreID, item.Quantity, purchaseId }, transaction);

                            // 3. Update Weighted Average Cost & Stock
                            // Formula: NewAvgCost = (OldQty * OldCost + NewQty * NewPrice) / (OldQty + NewQty)
                            string updateStockSql = @"
                                DECLARE @OldQty DECIMAL(18,2) = 0, @OldCost DECIMAL(18,2) = 0
                                SELECT @OldQty = ISNULL(SUM(Quantity), 0) FROM Stock WHERE ItemID = @ItemID
                                SELECT @OldCost = ISNULL(CostPrice, 0) FROM Items WHERE ItemID = @ItemID

                                UPDATE Items SET CostPrice = ((@OldQty * @OldCost) + (@Quantity * @UnitPrice)) / NULLIF(@OldQty + @Quantity, 0)
                                WHERE ItemID = @ItemID;

                                IF EXISTS (SELECT 1 FROM Stock WHERE ItemID = @ItemID AND StoreID = @StoreID)
                                    UPDATE Stock SET Quantity = Quantity + @Quantity WHERE ItemID = @ItemID AND StoreID = @StoreID
                                ELSE
                                    INSERT INTO Stock (ItemID, StoreID, Quantity) VALUES (@ItemID, @StoreID, @Quantity)";

                            await db.ExecuteAsync(updateStockSql, new { item.ItemID, item.Quantity, item.UnitPrice, invoice.StoreID }, transaction);
                        }

                        // 4. Create Accounting Journal Entry (Inventory vs. Cash/Supplier)
                        int journalId = await db.QuerySingleAsync<int>(
                            "INSERT INTO JournalEntries (Description, CreatedBy) VALUES (@Desc, @User); SELECT CAST(SCOPE_IDENTITY() as int)",
                            new { Desc = "Purchase Invoice: " + invoice.InvoiceNumber, User = invoice.CreatedBy }, transaction);

                        // Get Account IDs by Codes
                        var accounts = await db.QueryAsync<dynamic>("SELECT AccountID, AccountNumber FROM ChartOfAccounts WHERE AccountNumber IN ('1201', '1101', '1202', '2102')", null, transaction);
                        int inventoryAcc = accounts.First(a => a.AccountNumber == "1201").AccountID;
                        int cashAcc = accounts.First(a => a.AccountNumber == "1101").AccountID;
                        int inputVatAcc = accounts.First(a => a.AccountNumber == "1202").AccountID;
                        int supplierAcc = accounts.First(a => a.AccountNumber == "2102").AccountID;

                        // Debit: Inventory (Total amount before tax)
                        await db.ExecuteAsync("INSERT INTO JournalEntryDetails (JournalID, AccountID, Debit, Credit) VALUES (@jid, @acc, @amt, 0)",
                            new { jid = journalId, acc = inventoryAcc, amt = invoice.TotalAmount }, transaction);

                        // Debit: Input VAT (if any)
                        if (invoice.TaxAmount > 0)
                        {
                            await db.ExecuteAsync("INSERT INTO JournalEntryDetails (JournalID, AccountID, Debit, Credit) VALUES (@jid, @acc, @amt, 0)",
                                new { jid = journalId, acc = inputVatAcc, amt = invoice.TaxAmount }, transaction);
                        }

                        // Credit: Cash or Supplier (Net amount with tax)
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
    }
}
