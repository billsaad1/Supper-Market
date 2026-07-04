using System;
using System.Collections.Generic;
using System.Data;
using System.Threading.Tasks;
using Dapper;
using Microsoft.Data.SqlClient;

namespace Supermarket.DAL
{
    public class ReportRepository
    {
        private readonly string _connectionString;

        public ReportRepository(string connectionString)
        {
            _connectionString = connectionString;
        }

        public async Task<IEnumerable<dynamic>> GetDailySalesAsync(DateTime from, DateTime to)
        {
            using (IDbConnection db = new SqlConnection(_connectionString))
            {
                string sql = @"SELECT InvoiceDate, NetAmount, PaymentType FROM SalesInvoices
                               WHERE InvoiceDate BETWEEN @from AND @to";
                return await db.QueryAsync(sql, new { from, to });
            }
        }

        public async Task<IEnumerable<dynamic>> GetStockShortagesAsync()
        {
            using (IDbConnection db = new SqlConnection(_connectionString))
            {
                string sql = @"SELECT i.ItemName, i.MinimumStockLevel, SUM(s.Quantity) as CurrentQuantity
                               FROM Items i
                               JOIN Stock s ON i.ItemID = s.ItemID
                               GROUP BY i.ItemName, i.MinimumStockLevel
                               HAVING SUM(s.Quantity) <= i.MinimumStockLevel";
                return await db.QueryAsync(sql);
            }
        }

        public async Task<IEnumerable<dynamic>> GetItemMovementAsync(int itemId)
        {
            using (IDbConnection db = new SqlConnection(_connectionString))
            {
                string sql = @"SELECT MovementDate, MovementType, Quantity, Notes
                               FROM StockMovement WHERE ItemID = @itemId ORDER BY MovementDate DESC";
                return await db.QueryAsync(sql, new { itemId });
            }
        }

        public async Task<IEnumerable<dynamic>> GetExpiryReportAsync(int days)
        {
            using (IDbConnection db = new SqlConnection(_connectionString))
            {
                string sql = @"SELECT i.ItemName, s.ExpiryDate, s.Quantity
                               FROM Stock s JOIN Items i ON s.ItemID = i.ItemID
                               WHERE s.ExpiryDate <= DATEADD(day, @days, GETDATE())";
                return await db.QueryAsync(sql, new { days });
            }
        }
    }
}
