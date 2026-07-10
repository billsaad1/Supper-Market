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
        private TextBox txtCash, txtCard;
        private CheckBox chkSplit;
        private ComboBox cbMethod;
        private Panel pnlBody;

        public decimal CashAmount { get; set; }
        public decimal CardAmount { get; set; }
        public string PaymentMethod { get; set; }

        public PaymentForm(decimal total)
        {
            InitializeComponent();
            _total = total;
            SetupUI();
        }

        private void SetupUI()
        {
            bool isArabic = LanguageHelper.TranslationService.CurrentLanguage == Supermarket.BLL.Services.Language.Arabic;
            pnlBody = new Panel { Dock = DockStyle.Fill, Padding = new Padding(25) };
            this.Text = isArabic ? "شاشة الدفع" : "Secure Payment";
            this.Size = new Size(550, 600);
            this.StartPosition = FormStartPosition.CenterParent;
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.BackColor = Color.White;
            this.RightToLeft = isArabic ? RightToLeft.Yes : RightToLeft.No;

            Panel pnlHeader = new Panel { Dock = DockStyle.Top, Height = 90, BackColor = Color.FromArgb(248, 249, 250), Padding = new Padding(20) };
            Label lblTotalText = new Label { Text = (isArabic ? "المبلغ المستحق:" : "Total Due:"), Location = new Point(20, 30), Font = new Font("Segoe UI", 14), AutoSize = true };
            Label lblTotalVal = new Label {
                Text = $"{_total:N2} ريال",
                Dock = DockStyle.Right,
                Font = new Font("Segoe UI", 28, FontStyle.Bold),
                ForeColor = Color.FromArgb(0, 122, 255),
                TextAlign = ContentAlignment.MiddleRight,
                Width = 300
            };
            pnlHeader.Controls.Add(lblTotalText);
            pnlHeader.Controls.Add(lblTotalVal);

            chkSplit = new CheckBox { Text = isArabic ? "دفع متعدد (نقدي + بطاقة)" : "Split Payment (Cash + Card)", Location = new Point(30, 15), AutoSize = true, Font = new Font("Segoe UI", 10, FontStyle.Bold) };
            chkSplit.CheckedChanged += (s, e) => ToggleSplitMode();

            // Single Method Group
            Label lblMethodText = new Label { Text = isArabic ? "طريقة الدفع:" : "Payment Method:", Location = new Point(30, 50), AutoSize = true, Font = UITheme.MainFont };
            cbMethod = new ComboBox { Location = new Point(30, 75), Width = 470, Font = new Font("Segoe UI", 14), DropDownStyle = ComboBoxStyle.DropDownList };
            cbMethod.Items.AddRange(new string[] { "Cash / نقدي", "Card / بطاقة", "Credit / آجل" });
            cbMethod.SelectedIndex = 0;

            // Split Group (Hidden by default)
            Label lblCashText = new Label { Text = isArabic ? "المبلغ النقدي:" : "Cash Amount:", Location = new Point(30, 120), AutoSize = true, Font = UITheme.MainFont, Visible = false };
            txtCash = new TextBox { Location = new Point(30, 145), Width = 220, Font = new Font("Segoe UI", 18, FontStyle.Bold), Text = _total.ToString("F2"), TextAlign = HorizontalAlignment.Center, Visible = false };

            Label lblCardText = new Label { Text = isArabic ? "مبلغ البطاقة:" : "Card Amount:", Location = new Point(280, 120), AutoSize = true, Font = UITheme.MainFont, Visible = false };
            txtCard = new TextBox { Location = new Point(280, 145), Width = 220, Font = new Font("Segoe UI", 18, FontStyle.Bold), Text = "0.00", TextAlign = HorizontalAlignment.Center, Visible = false };

            txtCash.TextChanged += (s, e) => UpdateChange();
            txtCard.TextChanged += (s, e) => UpdateChange();

            lblChange = new Label {
                Text = (isArabic ? "الباقي / الفارق: " : "Change / Balance: ") + "0.00",
                Location = new Point(30, 220),
                Font = new Font("Segoe UI", 18, FontStyle.Bold),
                AutoSize = true,
                ForeColor = Color.FromArgb(40, 167, 69)
            };

            FlowLayoutPanel pnlFastCash = new FlowLayoutPanel { Location = new Point(30, 280), Size = new Size(480, 60) };
            int[] bills = { 500, 1000, 2000, 5000 };
            foreach (int bill in bills) {
                Button btnBill = new Button { Text = bill.ToString(), Width = 105, Height = 45, BackColor = Color.FromArgb(240, 240, 240), FlatStyle = FlatStyle.Flat, Font = new Font("Segoe UI", 10, FontStyle.Bold), Margin = new Padding(0,0,10,0) };
                btnBill.FlatAppearance.BorderSize = 0;
                btnBill.Click += (s, e) => { txtCash.Text = bill.ToString(); UpdateChange(); };
                pnlFastCash.Controls.Add(btnBill);
            }

            Button btnConfirm = new Button {
                Text = isArabic ? "تأكيد العملية وإصدار الإيصال" : "CONFIRM & PRINT RECEIPT",
                Dock = DockStyle.Bottom,
                Height = 80,
                BackColor = Color.FromArgb(40, 167, 69),
                ForeColor = Color.White,
                Font = new Font("Segoe UI", 18, FontStyle.Bold),
                FlatStyle = FlatStyle.Flat
            };
            btnConfirm.FlatAppearance.BorderSize = 0;
            btnConfirm.Click += (s, e) => {
                if (decimal.TryParse(txtCash.Text, out decimal cash) && decimal.TryParse(txtCard.Text, out decimal card)) {
                    CashAmount = cash;
                    CardAmount = card;
                    PaymentMethod = chkSplit.Checked ? "Split" : cbMethod.SelectedItem.ToString();
                    this.DialogResult = DialogResult.OK;
                    this.Close();
                }
            };

            pnlBody.Controls.AddRange(new Control[] { chkSplit, lblMethodText, cbMethod, lblCashText, txtCash, lblCardText, txtCard, lblChange, pnlFastCash });

            this.Controls.Add(pnlBody);
            this.Controls.Add(btnConfirm);
            this.Controls.Add(pnlHeader);

            LanguageHelper.ApplyLanguage(this);
        }

        private void ToggleSplitMode()
        {
            bool split = chkSplit.Checked;
            cbMethod.Visible = !split;
            txtCash.Visible = txtCard.Visible = split;
            foreach(Control c in pnlBody.Controls) {
                if (c is Label l && (l.Text.Contains("Cash") || l.Text.Contains("Card") || l.Text.Contains("نقدي") || l.Text.Contains("بطاقة")))
                    l.Visible = split;
                if (c is Label l2 && (l2.Text.Contains("Payment Method") || l2.Text.Contains("طريقة الدفع")))
                    l2.Visible = !split;
            }
            if (!split) { txtCash.Text = _total.ToString("F2"); txtCard.Text = "0.00"; }
            UpdateChange();
        }

        private void UpdateChange()
        {
            bool isArabic = LanguageHelper.TranslationService.CurrentLanguage == Supermarket.BLL.Services.Language.Arabic;
            if (decimal.TryParse(txtCash.Text, out decimal cash) && decimal.TryParse(txtCard.Text, out decimal card))
            {
                decimal totalPaid = cash + card;
                decimal change = totalPaid - _total;
                lblChange.Text = (isArabic ? "الباقي / الفارق: " : "Change / Balance: ") + $"{change:F2}";
                lblChange.ForeColor = change >= 0 ? Color.FromArgb(40, 167, 69) : Color.FromArgb(220, 53, 69);
            }
        }

        private void InitializeComponent() { }
    }
}
