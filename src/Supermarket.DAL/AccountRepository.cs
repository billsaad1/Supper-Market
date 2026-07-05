using System;
using System.Collections.Generic;
using System.Data;
using System.Threading.Tasks;
using Dapper;
using Microsoft.Data.SqlClient;

namespace Supermarket.DAL
{
    public class AccountRepository
    {
        private readonly string _connectionString;

        public AccountRepository(string connectionString)
        {
            _connectionString = connectionString;
        }

        public async Task<IEnumerable<dynamic>> GetFullChartOfAccountsAsync()
        {
            using (IDbConnection db = new SqlConnection(_connectionString))
            {
                string sql = @"
                    SELECT a.AccountID, a.AccountNumber, a.AccountName, a.AccountType, a.ParentAccountID,
                           (SELECT ISNULL(SUM(Debit - Credit), 0) FROM JournalEntryDetails WHERE AccountID = a.AccountID) as Balance
                    FROM ChartOfAccounts a";
                return await db.QueryAsync(sql);
            }
        }

        public async Task<int> AddAccountAsync(string code, string name, string type, int? parentId)
        {
            using (IDbConnection db = new SqlConnection(_connectionString))
            {
                string sql = "INSERT INTO ChartOfAccounts (AccountNumber, AccountName, AccountType, ParentAccountID) VALUES (@code, @name, @type, @parentId)";
                return await db.ExecuteAsync(sql, new { code, name, type, parentId });
            }
        }
    }
}
