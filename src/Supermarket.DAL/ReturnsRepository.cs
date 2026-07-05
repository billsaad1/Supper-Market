using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using Dapper;
using Microsoft.Data.SqlClient;

namespace Supermarket.DAL
{
    public class ReturnsRepository
    {
        private readonly string _connectionString;

        public ReturnsRepository(string connectionString)
        {
            _connectionString = connectionString;
        }

        public async Task ProcessSalesReturnAsync(int salesId, List<dynamic> returnedItems, int userId)
        {
            using (IDbConnection db = new SqlConnection(_connectionString))
            {
                db.Open();
                using (var transaction = db.BeginTransaction())
                {
                    try
                    {
                        decimal totalRefund = 0;
                        decimal totalTaxRefund = 0;

                        foreach (var item in returnedItems)
                        {
                            // Update Stock
                            await db.ExecuteAsync(
                                "UPDATE Stock SET Quantity = Quantity + @Qty WHERE ItemID = @ItemID",
                                new { Qty = item.Qty, ItemID = item.ItemID }, transaction);

                            // Log Movement
                            await db.ExecuteAsync(
                                "INSERT INTO StockMovement (ItemID, MovementType, Quantity, ReferenceNumber) VALUES (@ItemID, 'RETURN', @Qty, @Ref)",
                                new { ItemID = item.ItemID, Qty = item.Qty, Ref = "RET-" + salesId }, transaction);

                            // Calculate refund amounts (Simplified: assume tax is included or calculated here)
                            var itemDetails = await db.QueryFirstOrDefaultAsync<dynamic>(
                                "SELECT UnitPrice, TaxRate FROM SalesInvoiceItems WHERE SalesID = @SID AND ItemID = @IID",
                                new { SID = salesId, IID = item.ItemID }, transaction);

                            if (itemDetails != null)
                            {
                                decimal lineTotal = (decimal)itemDetails.UnitPrice * (decimal)item.Qty;
                                decimal tax = lineTotal * ((decimal)itemDetails.TaxRate / 100);
                                totalRefund += lineTotal;
                                totalTaxRefund += tax;
                            }
                        }

                        // Create Journal Entry
                        int journalId = await db.QuerySingleAsync<int>(
                            @"INSERT INTO JournalEntries (ReferenceNumber, Description, CreatedBy)
                              OUTPUT INSERTED.JournalID
                              VALUES (@Ref, @Desc, @User)",
                            new { Ref = "RET-" + salesId, Desc = "Sales Return for Invoice #" + salesId, User = userId }, transaction);

                        // Account Mapping (Using AccountCodes)
                        var accounts = await db.QueryAsync<dynamic>("SELECT AccountID, AccountNumber FROM ChartOfAccounts WHERE AccountNumber IN ('4103', '2101', '1101')", null, transaction);
                        int salesReturnAccount = accounts.First(a => a.AccountNumber == "4103").AccountID;
                        int vatOutputAccount = accounts.First(a => a.AccountNumber == "2101").AccountID;
                        int cashAccount = accounts.First(a => a.AccountNumber == "1101").AccountID;

                        // Debit Sales Returns
                        await db.ExecuteAsync(
                            "INSERT INTO JournalEntryDetails (JournalID, AccountID, Debit, Credit) VALUES (@JID, @AID, @Dr, 0)",
                            new { JID = journalId, AID = salesReturnAccount, Dr = totalRefund }, transaction);

                        // Debit VAT Output
                        await db.ExecuteAsync(
                            "INSERT INTO JournalEntryDetails (JournalID, AccountID, Debit, Credit) VALUES (@JID, @AID, @Dr, 0)",
                            new { JID = journalId, AID = vatOutputAccount, Dr = totalTaxRefund }, transaction);

                        // Credit Cash
                        await db.ExecuteAsync(
                            "INSERT INTO JournalEntryDetails (JournalID, AccountID, Debit, Credit) VALUES (@JID, @AID, 0, @Cr)",
                            new { JID = journalId, AID = cashAccount, Cr = totalRefund + totalTaxRefund }, transaction);

                        transaction.Commit();
                    }
                    catch
                    {
                        transaction.Rollback();
                        throw;
                    }
                }
            }
        }
    }
}
