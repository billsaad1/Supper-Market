using Supermarket.UI.Helpers;
using System;
using System.Drawing;
using System.Windows.Forms;

namespace Supermarket.UI.Views
{
    public partial class PaymentForm : Form
    {
        private decimal _total;
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

            Label lblTotal = new Label { Text = $"Total Due / المطلوب: {_total:F2}", Location = new Point(20, 20), Font = new Font("Arial", 16, FontStyle.Bold), AutoSize = true, ForeColor = Color.Blue };
            this.Controls.Add(lblTotal);

            this.Controls.Add(new Label { Text = "Paid Amount / المدفوع", Location = new Point(20, 80), AutoSize = true, Font = new Font("Arial", 12) });
            TextBox txtPaid = new TextBox { Location = new Point(200, 78), Width = 200, Font = new Font("Arial", 14), Text = _total.ToString() };
            this.Controls.Add(txtPaid);

            this.Controls.Add(new Label { Text = "Method / الطريقة", Location = new Point(20, 130), AutoSize = true, Font = new Font("Arial", 12) });
            ComboBox cbMethod = new ComboBox { Location = new Point(200, 128), Width = 200, Font = new Font("Arial", 12) };
            cbMethod.Items.AddRange(new string[] { "Cash / نقدي", "Card / بطاقة", "Credit / آجل" });
            cbMethod.SelectedIndex = 0;
            this.Controls.Add(cbMethod);

            Button btnConfirm = new Button { Text = "Confirm / تأكيد (F5)", Location = new Point(20, 250), Width = 380, Height = 60, BackColor = Color.Green, ForeColor = Color.White, Font = new Font("Arial", 16, FontStyle.Bold) };
            btnConfirm.Click += (s, e) => {
                PaidAmount = decimal.Parse(txtPaid.Text);
                PaymentMethod = cbMethod.SelectedItem.ToString();
                this.DialogResult = DialogResult.OK;
                this.Close();
            };
            this.Controls.Add(btnConfirm);

            LanguageHelper.ApplyLanguage(this);
        }

        private void InitializeComponent() { }
    }
}
