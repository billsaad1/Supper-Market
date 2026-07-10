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
                        // 1. Save Invoice Header
                        string invoiceSql = @"INSERT INTO SalesInvoices (InvoiceNumber, CustomerID, StoreID, TotalAmount, TaxAmount, DiscountAmount, NetAmount, PaymentType, CreatedBy, QRCode)
                                              VALUES (@InvoiceNumber, @CustomerID, @StoreID, @TotalAmount, @TaxAmount, @DiscountAmount, @NetAmount, @PaymentType, @CreatedBy, @QRCode);
                                              SELECT CAST(SCOPE_IDENTITY() as int)";

                        int salesId = await db.QuerySingleAsync<int>(invoiceSql, invoice, transaction);

                        decimal costOfGoodsSold = 0;

                        foreach (var item in items)
                        {
                            item.SalesID = salesId;

                            // Get Average Cost for COGS
                            decimal unitCost = await db.QueryFirstOrDefaultAsync<decimal>(
                                "SELECT CostPrice FROM Items WHERE ItemID = @ItemID", new { item.ItemID }, transaction);
                            costOfGoodsSold += (unitCost * item.Quantity);

                            // 2. Save Invoice Items
                            string itemSql = @"INSERT INTO SalesInvoiceItems (SalesID, ItemID, Quantity, UnitPrice, TaxAmount, TotalAmount)
                                                VALUES (@SalesID, @ItemID, @Quantity, @UnitPrice, @TaxAmount, @TotalAmount)";
                            await db.ExecuteAsync(itemSql, item, transaction);

                            // 3. Update Stock
                            string stockSql = @"UPDATE Stock SET Quantity = Quantity - @Quantity WHERE ItemID = @ItemID AND StoreID = @StoreID";
                            await db.ExecuteAsync(stockSql, new { item.ItemID, invoice.StoreID, item.Quantity }, transaction);
                        }

                        // 4. Accounting Entries
                        int journalId = await db.QuerySingleAsync<int>(
                            "INSERT INTO JournalEntries (Description, CreatedBy) VALUES (@Desc, @User); SELECT CAST(SCOPE_IDENTITY() as int)",
                            new { Desc = "Sales Invoice: " + invoice.InvoiceNumber, User = invoice.CreatedBy }, transaction);

                        // Get Account IDs by Codes
                        var accounts = await db.QueryAsync<dynamic>(
                            "SELECT AccountID, AccountNumber FROM ChartOfAccounts WHERE AccountNumber IN ('1101', '4101', '2101', '5101', '1201', '1102', '1103')",
                            null, transaction);

                        int cashAcc = accounts.First(a => a.AccountNumber == "1101").AccountID;
                        int salesAcc = accounts.First(a => a.AccountNumber == "4101").AccountID;
                        int vatAcc = accounts.First(a => a.AccountNumber == "2101").AccountID;
                        int cogsAcc = accounts.First(a => a.AccountNumber == "5101").AccountID;
                        int inventoryAcc = accounts.First(a => a.AccountNumber == "1201").AccountID;
                        int customerAcc = accounts.First(a => a.AccountNumber == "1102").AccountID;
                        int bankAcc = accounts.First(a => a.AccountNumber == "1103").AccountID;

                        if (invoice.PaymentType == "Split")
                        {
                            if (invoice.CashAmount > 0)
                                await db.ExecuteAsync("INSERT INTO JournalEntryDetails (JournalID, AccountID, Debit, Credit) VALUES (@jid, @acc, @amt, 0)", new { jid = journalId, acc = cashAcc, amt = invoice.CashAmount }, transaction);
                            if (invoice.CardAmount > 0)
                                await db.ExecuteAsync("INSERT INTO JournalEntryDetails (JournalID, AccountID, Debit, Credit) VALUES (@jid, @acc, @amt, 0)", new { jid = journalId, acc = bankAcc, amt = invoice.CardAmount }, transaction);
                        }
                        else
                        {
                            int debitAcc = (invoice.PaymentType.Contains("Card") || invoice.PaymentType.Contains("بطاقة")) ? bankAcc : (invoice.PaymentType == "Credit" ? customerAcc : cashAcc);
                            await db.ExecuteAsync("INSERT INTO JournalEntryDetails (JournalID, AccountID, Debit, Credit) VALUES (@jid, @acc, @amt, 0)", new { jid = journalId, acc = debitAcc, amt = invoice.TotalAmount }, transaction);
                        }
                        // Credit: Sales
                        await db.ExecuteAsync("INSERT INTO JournalEntryDetails (JournalID, AccountID, Debit, Credit) VALUES (@jid, @acc, 0, @amt)", new { jid = journalId, acc = salesAcc, amt = invoice.NetAmount }, transaction);
                        // Credit: VAT
                        await db.ExecuteAsync("INSERT INTO JournalEntryDetails (JournalID, AccountID, Debit, Credit) VALUES (@jid, @acc, 0, @amt)", new { jid = journalId, acc = vatAcc, amt = invoice.TaxAmount }, transaction);

                        // Entry: COGS to Inventory
                        await db.ExecuteAsync("INSERT INTO JournalEntryDetails (JournalID, AccountID, Debit, Credit) VALUES (@jid, @acc, @amt, 0)", new { jid = journalId, acc = cogsAcc, amt = costOfGoodsSold }, transaction);
                        await db.ExecuteAsync("INSERT INTO JournalEntryDetails (JournalID, AccountID, Debit, Credit) VALUES (@jid, @acc, 0, @amt)", new { jid = journalId, acc = inventoryAcc, amt = costOfGoodsSold }, transaction);

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
}
