using System.Collections.Generic;
using System.Data;
using System.Threading.Tasks;
using Dapper;
using Microsoft.Data.SqlClient;
using System.Linq;

namespace Supermarket.DAL
{
    public class AccountingRepository
    {
        private readonly string _connectionString;

        public AccountingRepository(string connectionString)
        {
            _connectionString = connectionString;
        }

        public async Task<int> SaveJournalEntryAsync(JournalEntry entry, List<JournalEntryDetail> details)
        {
            using (IDbConnection db = new SqlConnection(_connectionString))
            {
                db.Open();
                using (var transaction = db.BeginTransaction())
                {
                    try
                    {
                        string entrySql = @"INSERT INTO JournalEntries (ReferenceNumber, Description, CreatedBy)
                                            VALUES (@ReferenceNumber, @Description, @CreatedBy);
                                            SELECT CAST(SCOPE_IDENTITY() as int)";

                        int journalId = await db.QuerySingleAsync<int>(entrySql, entry, transaction);

                        foreach (var detail in details)
                        {
                            detail.JournalID = journalId;
                            string detailSql = @"INSERT INTO JournalEntryDetails (JournalID, AccountID, Debit, Credit, Description)
                                                 VALUES (@JournalID, @AccountID, @Debit, @Credit, @Description)";
                            await db.ExecuteAsync(detailSql, detail, transaction);
                        }

                        transaction.Commit();
                        return journalId;
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

    public class JournalEntry
    {
        public int JournalID { get; set; }
        public string ReferenceNumber { get; set; }
        public string Description { get; set; }
        public int CreatedBy { get; set; }
    }

    public class JournalEntryDetail
    {
        public int DetailID { get; set; }
        public int JournalID { get; set; }
        public int AccountID { get; set; }
        public decimal Debit { get; set; }
        public decimal Credit { get; set; }
        public string Description { get; set; }
    }
}
