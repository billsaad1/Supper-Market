using Supermarket.UI.Helpers;
using System;
using System.Drawing;
using System.Windows.Forms;

namespace Supermarket.UI.Views
{
    public partial class PaymentForm : Form
    {
        private decimal _total;
        private Label lblChange;
        private TextBox txtPaid;
        private ComboBox cbMethod;
        public decimal PaidAmount { get; set; }
        public string PaymentMethod { get; set; }

        public PaymentForm(decimal total)
        {
            InitializeComponent();
            _total = total;
            SetupUI();
        }

        private void SetupUI()
        {
            this.Text = "Payment / الدفع";
            this.Size = new Size(450, 400);
            this.StartPosition = FormStartPosition.CenterParent;
            this.FormBorderStyle = FormBorderStyle.FixedDialog;

            bool isArabic = LanguageHelper.TranslationService.CurrentLanguage == Supermarket.BLL.Services.Language.Arabic;
            Label lblTotal = new Label { Text = (isArabic ? "المبلغ المطلوب: " : "Total Due: ") + $"{_total:F2}", Location = new Point(20, 20), Font = new Font("Segoe UI", 18, FontStyle.Bold), AutoSize = true, ForeColor = Color.FromArgb(0, 122, 204) };
            this.Controls.Add(lblTotal);

            this.Controls.Add(new Label { Text = isArabic ? "المبلغ المدفوع:" : "Paid Amount:", Location = new Point(20, 80), AutoSize = true, Font = new Font("Segoe UI", 12) });
            txtPaid = new TextBox { Location = new Point(200, 78), Width = 200, Font = new Font("Segoe UI", 16), Text = _total.ToString() };
            txtPaid.TextChanged += (s, e) => UpdateChange();
            this.Controls.Add(txtPaid);

            this.Controls.Add(new Label { Text = isArabic ? "طريقة الدفع:" : "Method:", Location = new Point(20, 135), AutoSize = true, Font = new Font("Segoe UI", 12) });
            cbMethod = new ComboBox { Location = new Point(200, 133), Width = 200, Font = new Font("Segoe UI", 12), DropDownStyle = ComboBoxStyle.DropDownList };
            cbMethod.Items.AddRange(new string[] { "Cash / نقدي", "Card / بطاقة", "Credit / آجل" });

            if (PaymentMethod != null) {
                int idx = cbMethod.FindString(PaymentMethod);
                if (idx >= 0) cbMethod.SelectedIndex = idx;
                else cbMethod.SelectedIndex = 0;
            } else {
                cbMethod.SelectedIndex = 0;
            }
            this.Controls.Add(cbMethod);

            lblChange = new Label { Text = (isArabic ? "الباقي: " : "Change: ") + "0.00", Location = new Point(20, 185), Font = new Font("Segoe UI", 16, FontStyle.Bold), AutoSize = true, ForeColor = Color.FromArgb(220, 53, 69) };
            this.Controls.Add(lblChange);

            FlowLayoutPanel pnlFastCash = new FlowLayoutPanel { Location = new Point(20, 220), Size = new Size(400, 50) };
            int[] bills = { 500, 1000, 2000, 5000 };
            foreach (int bill in bills) {
                Button btnBill = new Button { Text = bill.ToString(), Width = 80, Height = 40, BackColor = Color.LightGray, FlatStyle = FlatStyle.Flat };
                btnBill.Click += (s, e) => { txtPaid.Text = bill.ToString(); UpdateChange(); };
                pnlFastCash.Controls.Add(btnBill);
            }
            this.Controls.Add(pnlFastCash);

            Button btnConfirm = new Button { Text = isArabic ? "تأكيد (F5)" : "Confirm (F5)", Location = new Point(20, 280), Width = 380, Height = 60, BackColor = Color.FromArgb(40, 167, 69), ForeColor = Color.White, Font = new Font("Segoe UI", 16, FontStyle.Bold), FlatStyle = FlatStyle.Flat };
            btnConfirm.Click += (s, e) => {
                PaidAmount = decimal.Parse(txtPaid.Text);
                PaymentMethod = cbMethod.SelectedItem.ToString();
                this.DialogResult = DialogResult.OK;
                this.Close();
            };
            this.Controls.Add(btnConfirm);

            LanguageHelper.ApplyLanguage(this);
        }

        private void UpdateChange()
        {
            bool isArabic = LanguageHelper.TranslationService.CurrentLanguage == Supermarket.BLL.Services.Language.Arabic;
            if (decimal.TryParse(txtPaid.Text, out decimal paid))
            {
                decimal change = paid - _total;
                lblChange.Text = (isArabic ? "الباقي: " : "Change: ") + $"{change:F2}";
                lblChange.ForeColor = change >= 0 ? Color.FromArgb(40, 167, 69) : Color.FromArgb(220, 53, 69);
            }
        }

        private void InitializeComponent() { }
    }
}
