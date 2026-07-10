using System;
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

        public async Task<int> SaveVoucherAsync(Supermarket.Models.Entities.Voucher v)
        {
            using (IDbConnection db = new SqlConnection(_connectionString))
            {
                db.Open();
                using (var transaction = db.BeginTransaction())
                {
                    try
                    {
                        string vSql = @"INSERT INTO Vouchers (VoucherType, Amount, AccountID, PaymentType, Notes, CreatedBy)
                                        VALUES (@VoucherType, @Amount, @AccountID, @PaymentType, @Notes, @CreatedBy);
                                        SELECT CAST(SCOPE_IDENTITY() as int)";
                        int vid = await db.QuerySingleAsync<int>(vSql, v, transaction);

                        // Auto-Journal Entry
                        int jid = await db.QuerySingleAsync<int>("INSERT INTO JournalEntries (Description, CreatedBy) VALUES (@desc, @user); SELECT CAST(SCOPE_IDENTITY() as int)",
                            new { desc = $"{v.VoucherType} Voucher #{vid}: {v.Notes}", user = v.CreatedBy }, transaction);

                        // Cash/Bank account
                        var accounts = await db.QueryAsync<dynamic>("SELECT AccountID, AccountNumber FROM ChartOfAccounts WHERE AccountNumber IN ('1101', '1103')", null, transaction);
                        int liquidAcc = v.PaymentType == "Cash" ? accounts.First(a => a.AccountNumber == "1101").AccountID : accounts.First(a => a.AccountNumber == "1103").AccountID;

                        if (v.VoucherType == "Payment")
                        {
                            // Debit Target Account, Credit Cash/Bank
                            await db.ExecuteAsync("INSERT INTO JournalEntryDetails (JournalID, AccountID, Debit, Credit) VALUES (@jid, @acc, @amt, 0)", new { jid, acc = v.AccountID, amt = v.Amount }, transaction);
                            await db.ExecuteAsync("INSERT INTO JournalEntryDetails (JournalID, AccountID, Debit, Credit) VALUES (@jid, @acc, 0, @amt)", new { jid, acc = liquidAcc, amt = v.Amount }, transaction);
                        }
                        else
                        {
                            // Debit Cash/Bank, Credit Target Account
                            await db.ExecuteAsync("INSERT INTO JournalEntryDetails (JournalID, AccountID, Debit, Credit) VALUES (@jid, @acc, @amt, 0)", new { jid, acc = liquidAcc, amt = v.Amount }, transaction);
                            await db.ExecuteAsync("INSERT INTO JournalEntryDetails (JournalID, AccountID, Debit, Credit) VALUES (@jid, @acc, 0, @amt)", new { jid, acc = v.AccountID, amt = v.Amount }, transaction);
                        }

                        transaction.Commit();
                        return vid;
                    }
                    catch { transaction.Rollback(); throw; }
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
