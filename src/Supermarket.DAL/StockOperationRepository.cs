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

        public async Task SaveAdjustmentAsync(int itemId, int storeId, decimal quantityChange, string type, int userId)
        {
            using (IDbConnection db = new SqlConnection(_connectionString))
            {
                db.Open();
                using (var transaction = db.BeginTransaction())
                {
                    try
                    {
                        // 1. Update Stock
                        await db.ExecuteAsync("UPDATE Stock SET Quantity = Quantity + @quantityChange WHERE ItemID = @itemId AND StoreID = @storeId",
                            new { itemId, storeId, quantityChange }, transaction);

                        // 2. Record Movement
                        await db.ExecuteAsync("INSERT INTO StockMovement (ItemID, StoreID, MovementType, Quantity, Notes) VALUES (@itemId, @storeId, @type, @quantityChange, 'Adjustment')",
                            new { itemId, storeId, type, quantityChange }, transaction);

                        // 3. Accounting Entry (Simplified: Loss or Gain)
                        int journalId = await db.QuerySingleAsync<int>(
                            "INSERT INTO JournalEntries (Description, CreatedBy) VALUES (@Desc, @User); SELECT CAST(SCOPE_IDENTITY() as int)",
                            new { Desc = "Stock " + type, User = userId }, transaction);

                        var accounts = await db.QueryAsync<dynamic>(
                            "SELECT AccountID, AccountNumber FROM ChartOfAccounts WHERE AccountNumber IN ('1201', '5102')",
                            null, transaction);

                        int inventoryAcc = accounts.First(a => a.AccountNumber == "1201").AccountID;
                        int adjustmentAcc = accounts.First(a => a.AccountNumber == "5102").AccountID;

                        decimal cost = await db.QueryFirstOrDefaultAsync<decimal>("SELECT CostPrice FROM Items WHERE ItemID = @itemId", new { itemId }, transaction);
                        decimal totalImpact = Math.Abs(quantityChange * cost);

                        if (quantityChange < 0) {
                            await db.ExecuteAsync("INSERT INTO JournalEntryDetails (JournalID, AccountID, Debit, Credit) VALUES (@jid, @adjAcc, @amt, 0)", new { jid = journalId, adjAcc = adjustmentAcc, amt = totalImpact }, transaction);
                            await db.ExecuteAsync("INSERT INTO JournalEntryDetails (JournalID, AccountID, Debit, Credit) VALUES (@jid, @invAcc, 0, @amt)", new { jid = journalId, invAcc = inventoryAcc, amt = totalImpact }, transaction);
                        } else {
                            await db.ExecuteAsync("INSERT INTO JournalEntryDetails (JournalID, AccountID, Debit, Credit) VALUES (@jid, @invAcc, @amt, 0)", new { jid = journalId, invAcc = inventoryAcc, amt = totalImpact }, transaction);
                            await db.ExecuteAsync("INSERT INTO JournalEntryDetails (JournalID, AccountID, Debit, Credit) VALUES (@jid, @adjAcc, 0, @amt)", new { jid = journalId, adjAcc = adjustmentAcc, amt = totalImpact }, transaction);
                        }

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
