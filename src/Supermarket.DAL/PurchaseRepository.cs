using System.Collections.Generic;
using System.Data;
using System.Threading.Tasks;
using Dapper;
using Microsoft.Data.SqlClient;
using Supermarket.Models.Entities;
using System.Linq;

namespace Supermarket.DAL
{
    public class PurchaseRepository
    {
        private readonly string _connectionString;

        public PurchaseRepository(string connectionString)
        {
            _connectionString = connectionString;
        }

        public async Task<int> SavePurchaseInvoiceAsync(PurchaseInvoice invoice, List<PurchaseInvoiceItem> items)
        {
            using (IDbConnection db = new SqlConnection(_connectionString))
            {
                db.Open();
                using (var transaction = db.BeginTransaction())
                {
                    try
                    {
                        string invoiceSql = @"INSERT INTO PurchaseInvoices (InvoiceNumber, SupplierID, StoreID, TotalAmount, TaxAmount, DiscountAmount, NetAmount, PaymentStatus, PaymentType, CreatedBy)
                                              VALUES (@InvoiceNumber, @SupplierID, @StoreID, @TotalAmount, @TaxAmount, @DiscountAmount, @NetAmount, @PaymentStatus, @PaymentType, @CreatedBy);
                                              SELECT CAST(SCOPE_IDENTITY() as int)";

                        int invoiceId = await db.QuerySingleAsync<int>(invoiceSql, invoice, transaction);

                        foreach (var item in items)
                        {
                            item.PurchaseID = invoiceId;
                            string itemSql = @"INSERT INTO PurchaseInvoiceItems (PurchaseID, ItemID, Quantity, UnitPrice, TaxAmount, TotalAmount, ExpiryDate)
                                                VALUES (@PurchaseID, @ItemID, @Quantity, @UnitPrice, @TaxAmount, @TotalAmount, @ExpiryDate)";
                            await db.ExecuteAsync(itemSql, item, transaction);

                            // Update Stock
                            string stockSql = @"IF EXISTS (SELECT 1 FROM Stock WHERE ItemID = @ItemID AND StoreID = @StoreID)
                                                UPDATE Stock SET Quantity = Quantity + @Quantity WHERE ItemID = @ItemID AND StoreID = @StoreID
                                                ELSE
                                                INSERT INTO Stock (ItemID, StoreID, Quantity) VALUES (@ItemID, @StoreID, @Quantity)";
                            await db.ExecuteAsync(stockSql, new { item.ItemID, invoice.StoreID, item.Quantity }, transaction);
                        }

                        transaction.Commit();
                        return invoiceId;
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

    public class PurchaseInvoice
    {
        public int PurchaseID { get; set; }
        public string InvoiceNumber { get; set; }
        public int SupplierID { get; set; }
        public int StoreID { get; set; }
        public decimal TotalAmount { get; set; }
        public decimal TaxAmount { get; set; }
        public decimal DiscountAmount { get; set; }
        public decimal NetAmount { get; set; }
        public string PaymentStatus { get; set; }
        public string PaymentType { get; set; }
        public int CreatedBy { get; set; }
    }

    public class PurchaseInvoiceItem
    {
        public int PurchaseItemID { get; set; }
        public int PurchaseID { get; set; }
        public int ItemID { get; set; }
        public decimal Quantity { get; set; }
        public decimal UnitPrice { get; set; }
        public decimal TaxAmount { get; set; }
        public decimal TotalAmount { get; set; }
        public System.DateTime? ExpiryDate { get; set; }
    }
}
