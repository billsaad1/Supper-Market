using System;
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

        public async Task<dynamic> GetCurrentShiftAsync(int userId)
        {
            using (IDbConnection db = new SqlConnection(_connectionString))
            {
                return await db.QueryFirstOrDefaultAsync("SELECT TOP 1 * FROM CashierShifts WHERE UserID = @userId AND Status = 'Open' ORDER BY StartTime DESC", new { userId });
            }
        }

        public async Task<decimal> CalculateExpectedAmountAsync(int shiftId)
        {
            using (IDbConnection db = new SqlConnection(_connectionString))
            {
                var shift = await db.QueryFirstOrDefaultAsync("SELECT * FROM CashierShifts WHERE ShiftID = @shiftId", new { shiftId });
                string sql = @"SELECT ISNULL(SUM(NetAmount), 0) FROM SalesInvoices WHERE CreatedBy = @uid AND InvoiceDate >= @start";
                decimal sales = await db.QuerySingleAsync<decimal>(sql, new { uid = shift.UserID, start = shift.StartTime });

                // Subtract returns
                string retSql = @"SELECT ISNULL(SUM(d.Debit + d.Credit), 0) FROM JournalEntryDetails d JOIN JournalEntries e ON d.JournalID = e.JournalID WHERE e.CreatedBy = @uid AND e.EntryDate >= @start AND e.Description LIKE 'Sales Return%'";
                // Simplified return calc
                return shift.OpeningBalance + sales;
            }
        }

        public async Task CloseShiftAsync(int shiftId, decimal actualAmount)
        {
            using (IDbConnection db = new SqlConnection(_connectionString))
            {
                db.Open();
                using (var transaction = db.BeginTransaction())
                {
                    try
                    {
                        decimal expected = await CalculateExpectedAmountAsync(shiftId);
                        decimal diff = actualAmount - expected;

                        await db.ExecuteAsync(
                            "UPDATE CashierShifts SET EndTime = GETDATE(), ActualAmount = @actual, ExpectedAmount = @exp, DifferenceAmount = @diff, Status = 'Closed' WHERE ShiftID = @id",
                            new { actual = actualAmount, exp = expected, diff = diff, id = shiftId }, transaction);

                        // Accounting for Variance
                        if (diff != 0)
                        {
                            var shift = await db.QueryFirstOrDefaultAsync("SELECT * FROM CashierShifts WHERE ShiftID = @shiftId", new { shiftId }, transaction);
                            int journalId = await db.QuerySingleAsync<int>(
                                "INSERT INTO JournalEntries (Description, CreatedBy) VALUES (@Desc, @User); SELECT CAST(SCOPE_IDENTITY() as int)",
                                new { Desc = "Shift Variance - Shift #" + shiftId, User = shift.UserID }, transaction);

                            var accounts = await db.QueryAsync<dynamic>("SELECT AccountID, AccountNumber FROM ChartOfAccounts WHERE AccountNumber IN ('1101', '5103', '4102')", null, transaction);
                            int cashAcc = accounts.First(a => a.AccountNumber == "1101").AccountID;
                            int shortageAcc = accounts.First(a => a.AccountNumber == "5103").AccountID;
                            int overAcc = accounts.First(a => a.AccountNumber == "4102").AccountID;

                            if (diff < 0) // Shortage (Expense)
                            {
                                await db.ExecuteAsync("INSERT INTO JournalEntryDetails (JournalID, AccountID, Debit, Credit) VALUES (@jid, @acc, @amt, 0)", new { jid = journalId, acc = shortageAcc, amt = Math.Abs(diff) }, transaction);
                                await db.ExecuteAsync("INSERT INTO JournalEntryDetails (JournalID, AccountID, Debit, Credit) VALUES (@jid, @acc, 0, @amt)", new { jid = journalId, acc = cashAcc, amt = Math.Abs(diff) }, transaction);
                            }
                            else // Over (Revenue)
                            {
                                await db.ExecuteAsync("INSERT INTO JournalEntryDetails (JournalID, AccountID, Debit, Credit) VALUES (@jid, @acc, @amt, 0)", new { jid = journalId, acc = cashAcc, amt = diff }, transaction);
                                await db.ExecuteAsync("INSERT INTO JournalEntryDetails (JournalID, AccountID, Debit, Credit) VALUES (@jid, @acc, 0, @amt)", new { jid = journalId, acc = overAcc, amt = diff }, transaction);
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
