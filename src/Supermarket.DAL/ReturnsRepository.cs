using System;
using System.Collections.Generic;
using System.Data;
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
                        // 1. Logic to reverse inventory and sales records
                        // 2. Create Reversing Journal Entry
                        // Debit Sales Returns, Debit VAT / Credit Cash/Customer

                        await db.ExecuteAsync("INSERT INTO JournalEntries (ReferenceNumber, Description, CreatedBy) VALUES (@Ref, @Desc, @User)",
                            new { Ref = "RET-" + salesId, Desc = "Return for Invoice " + salesId, User = userId }, transaction);

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
