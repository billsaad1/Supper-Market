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
                        var accounts = await db.QueryAsync<dynamic>("SELECT AccountID, AccountNumber FROM ChartOfAccounts WHERE AccountNumber IN ('1201', '1101')", null, transaction);
                        int inventoryAcc = accounts.First(a => a.AccountNumber == "1201").AccountID;
                        int cashAcc = accounts.First(a => a.AccountNumber == "1101").AccountID;

                        // Debit: Inventory, Credit: Cash
                        await db.ExecuteAsync("INSERT INTO JournalEntryDetails (JournalID, AccountID, Debit, Credit) VALUES (@jid, @acc, @amt, 0)", new { jid = journalId, acc = inventoryAcc, amt = invoice.TotalAmount }, transaction);
                        await db.ExecuteAsync("INSERT INTO JournalEntryDetails (JournalID, AccountID, Debit, Credit) VALUES (@jid, @acc, 0, @amt)", new { jid = journalId, acc = cashAcc, amt = invoice.NetAmount }, transaction);

                        transaction.Commit();
                        return purchaseId;
                    }
                    catch { transaction.Rollback(); throw; }
                }
            }
        }
    }
}
