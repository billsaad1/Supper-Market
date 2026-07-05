using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using Dapper;
using Microsoft.Data.SqlClient;

namespace Supermarket.DAL
{
    public class IntegratedAccountingRepository
    {
        private readonly string _connectionString;

        public IntegratedAccountingRepository(string connectionString)
        {
            _connectionString = connectionString;
        }

        public async Task<int> PostVoucherAsync(int accountId, decimal amount, string type, int userId, string notes)
        {
            using (IDbConnection db = new SqlConnection(_connectionString))
            {
                db.Open();
                using (var transaction = db.BeginTransaction())
                {
                    try
                    {
                        // 1. Save Voucher
                        string vSql = "INSERT INTO Vouchers (VoucherType, Amount, AccountID, Notes, CreatedBy) VALUES (@type, @amount, @accountId, @notes, @userId); SELECT CAST(SCOPE_IDENTITY() as int)";
                        int vId = await db.QuerySingleAsync<int>(vSql, new { type, amount, accountId, notes, userId }, transaction);

                        // 2. Journal Entry
                        int jId = await db.QuerySingleAsync<int>("INSERT INTO JournalEntries (Description, CreatedBy) VALUES (@notes, @userId); SELECT CAST(SCOPE_IDENTITY() as int)", new { notes, userId }, transaction);

                        var accounts = await db.QueryAsync<dynamic>("SELECT AccountID, AccountNumber FROM ChartOfAccounts WHERE AccountNumber = '1101'", null, transaction);
                        int cashAcc = accounts.First().AccountID;

                        if (type == "Receipt") // Receipt from Customer (Debit Cash, Credit Customer)
                        {
                            await db.ExecuteAsync("INSERT INTO JournalEntryDetails (JournalID, AccountID, Debit, Credit) VALUES (@jId, @cashAcc, @amount, 0)", new { jId, cashAcc, amount }, transaction);
                            await db.ExecuteAsync("INSERT INTO JournalEntryDetails (JournalID, AccountID, Debit, Credit) VALUES (@jId, @accountId, 0, @amount)", new { jId, accountId, amount }, transaction);
                        }
                        else // Payment to Supplier (Debit Supplier, Credit Cash)
                        {
                            await db.ExecuteAsync("INSERT INTO JournalEntryDetails (JournalID, AccountID, Debit, Credit) VALUES (@jId, @accountId, @amount, 0)", new { jId, accountId, amount }, transaction);
                            await db.ExecuteAsync("INSERT INTO JournalEntryDetails (JournalID, AccountID, Debit, Credit) VALUES (@jId, @cashAcc, 0, @amount)", new { jId, cashAcc, amount }, transaction);
                        }

                        transaction.Commit();
                        return vId;
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
