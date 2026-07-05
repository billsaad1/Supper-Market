using System;
using System.Collections.Generic;
using System.Data;
using System.Threading.Tasks;
using Dapper;
using Microsoft.Data.SqlClient;

namespace Supermarket.DAL
{
    public class FinancialReportRepository
    {
        private readonly string _connectionString;

        public FinancialReportRepository(string connectionString)
        {
            _connectionString = connectionString;
        }

        public async Task<dynamic> GetIncomeStatementAsync(DateTime from, DateTime to)
        {
            using (IDbConnection db = new SqlConnection(_connectionString))
            {
                string sql = @"
                    SELECT
                        (SELECT SUM(NetAmount) FROM SalesInvoices WHERE InvoiceDate BETWEEN @from AND @to) as Sales,
                        (SELECT SUM(NetAmount) FROM PurchaseInvoices WHERE InvoiceDate BETWEEN @from AND @to) as Purchases,
                        (SELECT SUM(Debit) FROM JournalEntryDetails jed JOIN JournalEntries je ON jed.JournalID = je.JournalID
                         WHERE AccountID = 4 AND je.EntryDate BETWEEN @from AND @to) as COGS";
                return await db.QueryFirstOrDefaultAsync(sql, new { from, to });
            }
        }

        public async Task<dynamic> GetVatReturnAsync(DateTime from, DateTime to)
        {
            using (IDbConnection db = new SqlConnection(_connectionString))
            {
                string sql = @"
                    SELECT
                        (SELECT SUM(TaxAmount) FROM SalesInvoices WHERE InvoiceDate BETWEEN @from AND @to) as OutputTax,
                        (SELECT SUM(TaxAmount) FROM PurchaseInvoices WHERE InvoiceDate BETWEEN @from AND @to) as InputTax";
                return await db.QueryFirstOrDefaultAsync(sql, new { from, to });
            }
        }
    }
}
