using Supermarket.UI.Helpers;
using System;
using System.Drawing;
using System.Windows.Forms;
using Supermarket.DAL;
using Dapper;

namespace Supermarket.UI.Views
{
    public partial class VouchersForm : Form
    {
        private IntegratedAccountingRepository _accRepo;

        public VouchersForm()
        {
            InitializeComponent();
            _accRepo = new IntegratedAccountingRepository(AppSettings.ConnectionString);
            SetupUI();
        }

        private void SetupUI()
        {
            this.Text = "Accounting Vouchers / السندات المحاسبية";
            this.Size = new Size(800, 500);

            TabControl tabs = new TabControl { Dock = DockStyle.Fill };

            tabs.TabPages.Add(CreateVoucherTab("Receipt Vouchers / سندات القبض", "Receipt"));
            tabs.TabPages.Add(CreateVoucherTab("Payment Vouchers / سندات الصرف", "Payment"));

            this.Controls.Add(tabs);
            LanguageHelper.ApplyLanguage(this);
        }

        private TabPage CreateVoucherTab(string title, string type)
        {
            TabPage tp = new TabPage(title);
            Panel pnl = new Panel { Dock = DockStyle.Fill, Padding = new Padding(20) };

            pnl.Controls.Add(new Label { Text = "Account / الحساب", Location = new Point(20, 20), AutoSize = true });
            ComboBox cbAcc = new ComboBox { Location = new Point(150, 18), Width = 250 };

            // Load accounts from DB
            LoadAccountsForVoucher(cbAcc);

            pnl.Controls.Add(cbAcc);

            pnl.Controls.Add(new Label { Text = "Amount / المبلغ", Location = new Point(20, 60), AutoSize = true });
            TextBox txtAmt = new TextBox { Location = new Point(150, 58), Width = 150 };
            pnl.Controls.Add(txtAmt);

            Button btnSave = new Button { Text = "SAVE VOUCHER / حفظ السند", Location = new Point(20, 150), Width = 380, Height = 60, BackColor = Color.FromArgb(0, 122, 204), ForeColor = Color.White, Font = new Font("Arial", 12, FontStyle.Bold) };
            btnSave.Click += async (s, e) => {
                if (cbAcc.SelectedItem == null) return;
                int accId = ((AccountItem)cbAcc.SelectedItem).ID;
                await _accRepo.PostVoucherAsync(accId, decimal.Parse(txtAmt.Text), type, 1, $"Voucher ({type}): {cbAcc.Text}");
                MessageBox.Show("Voucher Saved & Ledger Updated! / تم حفظ السند وترحيل القيود للحسابات");
                txtAmt.Clear();
            };
            pnl.Controls.Add(btnSave);

            tp.Controls.Add(pnl);
            return tp;
        }

        private async void LoadAccountsForVoucher(ComboBox cb)
        {
            string conn = AppSettings.ConnectionString;
            using (var db = new Microsoft.Data.SqlClient.SqlConnection(conn))
            {
                var accs = await db.QueryAsync<dynamic>("SELECT AccountID, AccountName FROM ChartOfAccounts WHERE ParentAccountID IS NOT NULL");
                foreach (var acc in accs)
                {
                    cb.Items.Add(new AccountItem { ID = (int)acc.AccountID, Name = (string)acc.AccountName });
                }
            }
        }

        private class AccountItem
        {
            public int ID { get; set; }
            public string Name { get; set; }
            public override string ToString() => Name;
        }

        private void InitializeComponent() { }
    }
}
