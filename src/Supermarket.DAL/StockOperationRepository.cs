using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using Dapper;
using Microsoft.Data.SqlClient;

namespace Supermarket.DAL
{
    public class StockOperationRepository
    {
        private readonly string _connectionString;

        public StockOperationRepository(string connectionString)
        {
            _connectionString = connectionString;
        }

        public async Task<IEnumerable<dynamic>> GetStockByStoreAsync(int storeId)
        {
            using (IDbConnection db = new SqlConnection(_connectionString))
            {
                string sql = @"SELECT s.ItemID, i.ItemName, s.Quantity
                               FROM Stock s
                               JOIN Items i ON s.ItemID = i.ItemID
                               WHERE s.StoreID = @storeId";
                return await db.QueryAsync(sql, new { storeId });
            }
        }

        public async Task ProcessAdjustmentsAsync(int storeId, List<dynamic> adjustments, int userId)
        {
            using (IDbConnection db = new SqlConnection(_connectionString))
            {
                db.Open();
                using (var transaction = db.BeginTransaction())
                {
                    try
                    {
                        foreach (var adj in adjustments)
                        {
                            // 1. Update Stock
                            await db.ExecuteAsync(
                                "UPDATE Stock SET Quantity = Quantity + @v WHERE ItemID = @iid AND StoreID = @sid",
                                new { v = adj.Variance, iid = adj.ItemID, sid = storeId }, transaction);

                            // 2. Log Movement
                            await db.ExecuteAsync(
                                "INSERT INTO StockMovement (ItemID, StoreID, MovementType, Quantity, ReferenceID) VALUES (@iid, @sid, 'Adjustment', @qty, 0)",
                                new { iid = adj.ItemID, sid = storeId, qty = adj.Variance }, transaction);

                            // 3. Accounting (Gain or Loss)
                            var item = await db.QueryFirstOrDefaultAsync<dynamic>("SELECT CostPrice FROM Items WHERE ItemID = @id", new { id = adj.ItemID }, transaction);
                            decimal value = Math.Abs(adj.Variance * (decimal)item.CostPrice);

                            int journalId = await db.QuerySingleAsync<int>(
                                "INSERT INTO JournalEntries (Description, CreatedBy) VALUES (@Desc, @User); SELECT CAST(SCOPE_IDENTITY() as int)",
                                new { Desc = "Inventory Adjustment - Item ID " + adj.ItemID, User = userId }, transaction);

                            var accounts = await db.QueryAsync<dynamic>("SELECT AccountID, AccountNumber FROM ChartOfAccounts WHERE AccountNumber IN ('1201', '5102', '4102')", null, transaction);
                            int inventoryAcc = accounts.First(a => a.AccountNumber == "1201").AccountID;
                            int adjExpenseAcc = accounts.First(a => a.AccountNumber == "5102").AccountID;
                            int adjRevenueAcc = accounts.First(a => a.AccountNumber == "4102").AccountID;

                            if (adj.Variance < 0) // Loss
                            {
                                await db.ExecuteAsync("INSERT INTO JournalEntryDetails (JournalID, AccountID, Debit, Credit) VALUES (@jid, @acc, @amt, 0)", new { jid = journalId, acc = adjExpenseAcc, amt = value }, transaction);
                                await db.ExecuteAsync("INSERT INTO JournalEntryDetails (JournalID, AccountID, Debit, Credit) VALUES (@jid, @acc, 0, @amt)", new { jid = journalId, acc = inventoryAcc, amt = value }, transaction);
                            }
                            else // Gain
                            {
                                await db.ExecuteAsync("INSERT INTO JournalEntryDetails (JournalID, AccountID, Debit, Credit) VALUES (@jid, @acc, @amt, 0)", new { jid = journalId, acc = inventoryAcc, amt = value }, transaction);
                                await db.ExecuteAsync("INSERT INTO JournalEntryDetails (JournalID, AccountID, Debit, Credit) VALUES (@jid, @acc, 0, @amt)", new { jid = journalId, acc = adjRevenueAcc, amt = value }, transaction);
                            }
                        }
                        transaction.Commit();
                    }
                    catch { transaction.Rollback(); throw; }
                }
            }
        }
    }
}
