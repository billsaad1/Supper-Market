using Supermarket.UI.Helpers;
using System;
using System.Drawing;
using System.Windows.Forms;
using Supermarket.DAL;
using Supermarket.Models.Entities;
using Supermarket.BLL.Services;
using System.Collections.Generic;
using System.Linq;

namespace Supermarket.UI.Views
{
    public partial class VouchersForm : Form
    {
        private ComboBox cbType, cbAccount, cbPaymentMethod;
        private TextBox txtAmount, txtNotes;
        private DataGridView dgvRecent;
        private AccountingRepository _accRepo;
        private AccountRepository _acctListRepo;

        public VouchersForm()
        {
            InitializeComponent();
            _accRepo = new AccountingRepository(AppSettings.ConnectionString);
            _acctListRepo = new AccountRepository(AppSettings.ConnectionString);
            SetupUI();
            LoadAccounts();
        }

        private void SetupUI()
        {
            bool isArabic = LanguageHelper.TranslationService.CurrentLanguage == Language.Arabic;
            this.Text = isArabic ? "سندات القبض والصرف" : "Vouchers (Receipt/Payment)";
            this.Size = new Size(900, 600);
            this.BackColor = UITheme.ContentBg;
            this.RightToLeft = isArabic ? RightToLeft.Yes : RightToLeft.No;

            Panel pnlEntry = new Panel { Dock = DockStyle.Left, Width = 350, BackColor = Color.White, Padding = new Padding(20) };

            cbType = CreateField<ComboBox>(isArabic ? "نوع السند:" : "Voucher Type:", pnlEntry);
            cbType.Items.AddRange(new string[] { isArabic ? "صرف (مصروف/مورد)" : "Payment (Expense/Supplier)", isArabic ? "قبض (عميل)" : "Receipt (Customer)" });
            cbType.SelectedIndex = 0;

            cbAccount = CreateField<ComboBox>(isArabic ? "الحساب المستهدف:" : "Target Account:", pnlEntry);
            txtAmount = CreateField<TextBox>(isArabic ? "المبلغ:" : "Amount:", pnlEntry);
            cbPaymentMethod = CreateField<ComboBox>(isArabic ? "طريقة الدفع:" : "Payment Method:", pnlEntry);
            cbPaymentMethod.Items.AddRange(new string[] { isArabic ? "نقد" : "Cash", isArabic ? "بنك / شبكة" : "Bank / Card" });
            cbPaymentMethod.SelectedIndex = 0;

            txtNotes = CreateField<TextBox>(isArabic ? "ملاحظات:" : "Notes:", pnlEntry);
            txtNotes.Multiline = true; txtNotes.Height = 80;

            Button btnSave = new Button { Text = isArabic ? "حفظ السند" : "SAVE VOUCHER", Dock = DockStyle.Bottom, Height = 45, BackColor = UITheme.SuccessColor, ForeColor = Color.White, FlatStyle = FlatStyle.Flat, Font = new Font("Segoe UI", 10, FontStyle.Bold) };
            btnSave.Click += BtnSave_Click;
            pnlEntry.Controls.Add(btnSave);

            dgvRecent = new DataGridView { Dock = DockStyle.Fill, BackColor = Color.White };
            UITheme.ApplyModernStyle(dgvRecent);

            this.Controls.Add(dgvRecent);
            this.Controls.Add(pnlEntry);
        }

        private T CreateField<T>(string label, Panel p) where T : Control, new()
        {
            p.Controls.Add(new Label { Text = label, Dock = DockStyle.Top, Height = 25, Font = UITheme.GridFont });
            T field = new T { Dock = DockStyle.Top, Height = 30, Font = UITheme.MainFont };
            p.Controls.Add(field);
            p.Controls.Add(new Panel { Dock = DockStyle.Top, Height = 15 }); // Spacer
            return field;
        }

        private async void LoadAccounts()
        {
            var accounts = await _acctListRepo.GetFullChartOfAccountsAsync();
            cbAccount.DataSource = accounts.ToList();
            cbAccount.DisplayMember = "AccountName";
            cbAccount.ValueMember = "AccountID";
        }

        private async void BtnSave_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrEmpty(txtAmount.Text) || cbAccount.SelectedValue == null) return;

            var voucher = new Voucher {
                VoucherType = cbType.SelectedIndex == 0 ? "Payment" : "Receipt",
                VoucherDate = DateTime.Now,
                Amount = decimal.Parse(txtAmount.Text),
                AccountID = (int)cbAccount.SelectedValue,
                PaymentType = cbPaymentMethod.SelectedIndex == 0 ? "Cash" : "Bank",
                Notes = txtNotes.Text,
                CreatedBy = 1
            };

            await _accRepo.SaveVoucherAsync(voucher);
            MessageBox.Show(LanguageHelper.TranslationService.CurrentLanguage == Language.Arabic ? "تم حفظ السند والقيود المحاسبية" : "Voucher and Journal Entries saved");
            this.Close();
        }

        private void InitializeComponent() { }
    }
}
