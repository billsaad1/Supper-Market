using System;
using System.Collections.Generic;
using System.Data;
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
                        // Account 5 (Inventory), Account 7 (Inventory Adjustment Loss/Gain)
                        int journalId = await db.QuerySingleAsync<int>(
                            "INSERT INTO JournalEntries (Description, CreatedBy) VALUES (@Desc, @User); SELECT CAST(SCOPE_IDENTITY() as int)",
                            new { Desc = "Stock " + type, User = userId }, transaction);

                        if (quantityChange < 0) {
                            await db.ExecuteAsync("INSERT INTO JournalEntryDetails (JournalID, AccountID, Debit, Credit) VALUES (@jid, 7, @amt, 0)", new { jid = journalId, amt = Math.Abs(quantityChange * 10) }, transaction); // Dummy cost 10
                            await db.ExecuteAsync("INSERT INTO JournalEntryDetails (JournalID, AccountID, Debit, Credit) VALUES (@jid, 5, 0, @amt)", new { jid = journalId, amt = Math.Abs(quantityChange * 10) }, transaction);
                        } else {
                            await db.ExecuteAsync("INSERT INTO JournalEntryDetails (JournalID, AccountID, Debit, Credit) VALUES (@jid, 5, @amt, 0)", new { jid = journalId, amt = quantityChange * 10 }, transaction);
                            await db.ExecuteAsync("INSERT INTO JournalEntryDetails (JournalID, AccountID, Debit, Credit) VALUES (@jid, 7, 0, @amt)", new { jid = journalId, amt = quantityChange * 10 }, transaction);
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
