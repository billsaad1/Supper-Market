using System.Collections.Generic;
using System.Data;
using System.Threading.Tasks;
using Dapper;
using Microsoft.Data.SqlClient;
using Supermarket.Models.Entities;
using System.Linq;

namespace Supermarket.DAL
{
    public class SalesRepository
    {
        private readonly string _connectionString;

        public SalesRepository(string connectionString)
        {
            _connectionString = connectionString;
        }

        public async Task<int> SaveSalesInvoiceAsync(SalesInvoice invoice, List<SalesInvoiceItem> items)
        {
            using (IDbConnection db = new SqlConnection(_connectionString))
            {
                db.Open();
                using (var transaction = db.BeginTransaction())
                {
                    try
                    {
                        string invoiceSql = @"INSERT INTO SalesInvoices (InvoiceNumber, CustomerID, StoreID, TotalAmount, TaxAmount, DiscountAmount, NetAmount, PaymentType, CreatedBy, QRCode)
                                              VALUES (@InvoiceNumber, @CustomerID, @StoreID, @TotalAmount, @TaxAmount, @DiscountAmount, @NetAmount, @PaymentType, @CreatedBy, @QRCode);
                                              SELECT CAST(SCOPE_IDENTITY() as int)";

                        int salesId = await db.QuerySingleAsync<int>(invoiceSql, invoice, transaction);

                        foreach (var item in items)
                        {
                            item.SalesID = salesId;
                            string itemSql = @"INSERT INTO SalesInvoiceItems (SalesID, ItemID, Quantity, UnitPrice, TaxAmount, TotalAmount)
                                                VALUES (@SalesID, @ItemID, @Quantity, @UnitPrice, @TaxAmount, @TotalAmount)";
                            await db.ExecuteAsync(itemSql, item, transaction);

                            string stockSql = @"UPDATE Stock SET Quantity = Quantity - @Quantity WHERE ItemID = @ItemID AND StoreID = @StoreID";
                            await db.ExecuteAsync(stockSql, new { item.ItemID, invoice.StoreID, item.Quantity }, transaction);
                        }

                        transaction.Commit();
                        return salesId;
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

    public class SalesInvoice
    {
        public int SalesID { get; set; }
        public string InvoiceNumber { get; set; }
        public int? CustomerID { get; set; }
        public int StoreID { get; set; }
        public decimal TotalAmount { get; set; }
        public decimal TaxAmount { get; set; }
        public decimal DiscountAmount { get; set; }
        public decimal NetAmount { get; set; }
        public string PaymentType { get; set; }
        public int CreatedBy { get; set; }
        public string QRCode { get; set; }
    }

    public class SalesInvoiceItem
    {
        public int SalesItemID { get; set; }
        public int SalesID { get; set; }
        public int ItemID { get; set; }
        public decimal Quantity { get; set; }
        public decimal UnitPrice { get; set; }
        public decimal TaxAmount { get; set; }
        public decimal TotalAmount { get; set; }
    }
}
