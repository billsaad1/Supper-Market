using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using Dapper;
using Microsoft.Data.SqlClient;

namespace Supermarket.DAL
{
    public class ShiftRepository
    {
        private readonly string _connectionString;

        public ShiftRepository(string connectionString)
        {
            _connectionString = connectionString;
        }

        public async Task<int> OpenShiftAsync(int userId, decimal openingBalance)
        {
            using (IDbConnection db = new SqlConnection(_connectionString))
            {
                string sql = @"INSERT INTO CashierShifts (UserID, StartTime, OpeningBalance, Status)
                               VALUES (@userId, GETDATE(), @openingBalance, 'Open');
                               SELECT CAST(SCOPE_IDENTITY() as int)";
                return await db.QuerySingleAsync<int>(sql, new { userId, openingBalance });
            }
        }

        public async Task CloseShiftAsync(int shiftId, decimal actualAmount, int userId)
        {
            using (IDbConnection db = new SqlConnection(_connectionString))
            {
                db.Open();
                using (var transaction = db.BeginTransaction())
                {
                    try
                    {
                        // 1. Calculate Difference
                        var shift = await db.QueryFirstOrDefaultAsync<dynamic>(
                            "SELECT ExpectedAmount FROM CashierShifts WHERE ShiftID = @shiftId", new { shiftId }, transaction);
                        decimal diff = actualAmount - (decimal)shift.ExpectedAmount;

                        // 2. Update Shift
                        string sql = @"UPDATE CashierShifts
                                       SET EndTime = GETDATE(), ActualAmount = @actualAmount, DifferenceAmount = @diff, Status = 'Closed'
                                       WHERE ShiftID = @shiftId";
                        await db.ExecuteAsync(sql, new { shiftId, actualAmount, diff }, transaction);

                        // 3. Accounting Entry for Difference
                        if (diff != 0)
                        {
                            int journalId = await db.QuerySingleAsync<int>(
                                "INSERT INTO JournalEntries (Description, CreatedBy) VALUES (@Desc, @User); SELECT CAST(SCOPE_IDENTITY() as int)",
                                new { Desc = "Shift Difference: " + shiftId, User = userId }, transaction);

                            var accounts = await db.QueryAsync<dynamic>(
                                "SELECT AccountID, AccountNumber FROM ChartOfAccounts WHERE AccountNumber IN ('1101', '5103', '4102')",
                                null, transaction);

                            int cashAcc = accounts.First(a => a.AccountNumber == "1101").AccountID;
                            int shortageAcc = accounts.First(a => a.AccountNumber == "5103").AccountID;
                            int excessAcc = accounts.First(a => a.AccountNumber == "4102").AccountID;

                            if (diff < 0) // Shortage (Debit Loss, Credit Cash)
                            {
                                await db.ExecuteAsync("INSERT INTO JournalEntryDetails (JournalID, AccountID, Debit, Credit) VALUES (@jid, @acc, @amt, 0)", new { jid = journalId, acc = shortageAcc, amt = Math.Abs(diff) }, transaction);
                                await db.ExecuteAsync("INSERT INTO JournalEntryDetails (JournalID, AccountID, Debit, Credit) VALUES (@jid, @acc, 0, @amt)", new { jid = journalId, acc = cashAcc, amt = Math.Abs(diff) }, transaction);
                            }
                            else // Excess (Debit Cash, Credit Gain)
                            {
                                await db.ExecuteAsync("INSERT INTO JournalEntryDetails (JournalID, AccountID, Debit, Credit) VALUES (@jid, @acc, @amt, 0)", new { jid = journalId, acc = cashAcc, amt = diff }, transaction);
                                await db.ExecuteAsync("INSERT INTO JournalEntryDetails (JournalID, AccountID, Debit, Credit) VALUES (@jid, @acc, 0, @amt)", new { jid = journalId, acc = excessAcc, amt = diff }, transaction);
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
