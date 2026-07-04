using System;
using System.Drawing;
using System.Windows.Forms;
using Supermarket.BLL;

namespace Supermarket.UI
{
    public partial class PosForm : Form
    {
        private TextBox txtBarcode;
        private DataGridView dgvInvoice;
        private Label lblTotal;
        private Button btnPay;

        public PosForm()
        {
            InitializeComponent();
            SetupUI();
        }

        private void SetupUI()
        {
            this.Text = "POS / نقطة البيع";
            this.Size = new Size(1000, 700);

            txtBarcode = new TextBox { Dock = DockStyle.Top, Font = new Font("Arial", 14), PlaceholderText = "Scan Barcode / امسح الباركود" };
            dgvInvoice = new DataGridView { Dock = DockStyle.Fill, AutoGenerateColumns = true };

            Panel footer = new Panel { Dock = DockStyle.Bottom, Height = 100, BackColor = Color.LightGray };
            lblTotal = new Label { Text = "Total: 0.00", Font = new Font("Arial", 20, FontStyle.Bold), Location = new Point(20, 30), AutoSize = true };
            btnPay = new Button { Text = "Pay / دفع (F5)", Dock = DockStyle.Right, Width = 200, BackColor = Color.Green, ForeColor = Color.White, Font = new Font("Arial", 16, FontStyle.Bold) };

            footer.Controls.Add(lblTotal);
            footer.Controls.Add(btnPay);

            this.Controls.Add(dgvInvoice);
            this.Controls.Add(txtBarcode);
            this.Controls.Add(footer);

            LanguageHelper.ApplyLanguage(this);
        }

        private void InitializeComponent() { }
    }
}
