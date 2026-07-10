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
                    DECLARE @OpeningStock DECIMAL(18,2) = 0, @ClosingStock DECIMAL(18,2) = 0

                    -- Simplified: Fetching based on stock snapshots would be better, but here we estimate from movements
                    SELECT @OpeningStock = ISNULL(SUM(Quantity * CostPrice), 0) FROM Items i JOIN Stock s ON i.ItemID = s.ItemID

                    SELECT
                        ISNULL((SELECT SUM(NetAmount) FROM SalesInvoices WHERE InvoiceDate BETWEEN @from AND @to), 0) as GrossSales,
                        ISNULL((SELECT SUM(DiscountAmount) FROM SalesInvoices WHERE InvoiceDate BETWEEN @from AND @to), 0) as SalesDiscounts,
                        ISNULL((SELECT SUM(NetAmount) FROM SalesInvoices WHERE InvoiceDate BETWEEN @from AND @to), 0) as NetSales,

                        @OpeningStock as OpeningStock,
                        ISNULL((SELECT SUM(NetAmount) FROM PurchaseInvoices WHERE InvoiceDate BETWEEN @from AND @to), 0) as Purchases,
                        @ClosingStock as ClosingStock, -- Usually calculated via physical count

                        -- Cost of Goods Sold (Sum of COGS entries)
                        ISNULL((SELECT SUM(Debit) FROM JournalEntryDetails d JOIN JournalEntries e ON d.JournalID = e.JournalID JOIN ChartOfAccounts a ON d.AccountID = a.AccountID WHERE a.AccountNumber = '5101' AND e.EntryDate BETWEEN @from AND @to), 0) as COGS,

                        -- Expenses
                        ISNULL((SELECT SUM(Debit) FROM JournalEntryDetails d JOIN JournalEntries e ON d.JournalID = e.JournalID JOIN ChartOfAccounts a ON d.AccountID = a.AccountID WHERE a.AccountNumber LIKE '510[4-6]' AND e.EntryDate BETWEEN @from AND @to), 0) as TotalOperatingExpenses";

                return await db.QueryFirstOrDefaultAsync(sql, new { from, to });
            }
        }

        public async Task<IEnumerable<dynamic>> GetSupplierStatementAsync(int supplierId, DateTime from, DateTime to)
        {
            using (IDbConnection db = new SqlConnection(_connectionString))
            {
                string sql = @"
                    SELECT
                        InvoiceDate as TransDate, 'Purchase' as TransType, InvoiceNumber as Ref, NetAmount as Debit, 0 as Credit
                    FROM PurchaseInvoices
                    WHERE SupplierID = @supplierId AND InvoiceDate BETWEEN @from AND @to
                    UNION ALL
                    SELECT
                        ReturnDate as TransDate, 'Return' as TransType, ReturnNumber as Ref, 0 as Debit, NetAmount as Credit
                    FROM PurchaseReturns
                    WHERE SupplierID = @supplierId AND ReturnDate BETWEEN @from AND @to
                    UNION ALL
                    SELECT
                        VoucherDate as TransDate, 'Payment' as TransType, CAST(VoucherID as NVARCHAR) as Ref, 0 as Debit, Amount as Credit
                    FROM Vouchers v
                    JOIN ChartOfAccounts a ON v.AccountID = a.AccountID
                    WHERE a.AccountNumber = '2102' AND VoucherDate BETWEEN @from AND @to
                    ORDER BY TransDate";
                return await db.QueryAsync(sql, new { supplierId, from, to });
            }
        }

        public async Task<dynamic> GetVatReturnAsync(DateTime from, DateTime to)
        {
            using (IDbConnection db = new SqlConnection(_connectionString))
            {
                string sql = @"
                    SELECT
                        ISNULL((SELECT SUM(TaxAmount) FROM SalesInvoices WHERE InvoiceDate BETWEEN @from AND @to), 0) as OutputTax,
                        ISNULL((SELECT SUM(TaxAmount) FROM PurchaseInvoices WHERE InvoiceDate BETWEEN @from AND @to), 0) as InputTax";
                return await db.QueryFirstOrDefaultAsync(sql, new { from, to });
            }
        }
    }
}
