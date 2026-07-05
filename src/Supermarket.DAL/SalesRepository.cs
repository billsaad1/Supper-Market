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

                        decimal costOfGoodsSold = 0;

                        foreach (var item in items)
                        {
                            item.SalesID = salesId;

                            // Get Average Cost for Accounting
                            decimal avgCost = await db.QueryFirstOrDefaultAsync<decimal>(
                                "SELECT CostPrice FROM Items WHERE ItemID = @ItemID", new { item.ItemID }, transaction);
                            costOfGoodsSold += (avgCost * item.Quantity);

                            string itemSql = @"INSERT INTO SalesInvoiceItems (SalesID, ItemID, Quantity, UnitPrice, TaxAmount, TotalAmount)
                                                VALUES (@SalesID, @ItemID, @Quantity, @UnitPrice, @TaxAmount, @TotalAmount)";
                            await db.ExecuteAsync(itemSql, item, transaction);

                            string stockSql = @"UPDATE Stock SET Quantity = Quantity - @Quantity WHERE ItemID = @ItemID AND StoreID = @StoreID";
                            await db.ExecuteAsync(stockSql, new { item.ItemID, invoice.StoreID, item.Quantity }, transaction);
                        }

                        // Generate Accounting Entries
                        int journalId = await db.QuerySingleAsync<int>(
                            "INSERT INTO JournalEntries (ReferenceNumber, Description, CreatedBy) VALUES (@Ref, @Desc, @User); SELECT CAST(SCOPE_IDENTITY() as int)",
                            new { Ref = invoice.InvoiceNumber, Desc = "Sales Invoice " + invoice.InvoiceNumber, User = invoice.CreatedBy }, transaction);

                        // Account Mapping: Cash=1, Sales=2, Tax=3, COGS=4, Inventory=5, Customers(A/R)=6
                        int debitAccountId = invoice.PaymentType == "Credit" ? 6 : 1;

                        // Debit Cash/Customer
                        await db.ExecuteAsync("INSERT INTO JournalEntryDetails (JournalID, AccountID, Debit, Credit) VALUES (@jid, @accId, @amt, 0)", new { jid = journalId, accId = debitAccountId, amt = invoice.NetAmount }, transaction);
                        // Credit Sales
                        await db.ExecuteAsync("INSERT INTO JournalEntryDetails (JournalID, AccountID, Debit, Credit) VALUES (@jid, 2, 0, @amt)", new { jid = journalId, amt = invoice.TotalAmount }, transaction);
                        // Credit Tax
                        await db.ExecuteAsync("INSERT INTO JournalEntryDetails (JournalID, AccountID, Debit, Credit) VALUES (@jid, 3, 0, @amt)", new { jid = journalId, amt = invoice.TaxAmount }, transaction);

                        // Entry 2: COGS to Inventory
                        await db.ExecuteAsync("INSERT INTO JournalEntryDetails (JournalID, AccountID, Debit, Credit) VALUES (@jid, 4, @amt, 0)", new { jid = journalId, amt = costOfGoodsSold }, transaction);
                        await db.ExecuteAsync("INSERT INTO JournalEntryDetails (JournalID, AccountID, Debit, Credit) VALUES (@jid, 5, 0, @amt)", new { jid = journalId, amt = costOfGoodsSold }, transaction);

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
