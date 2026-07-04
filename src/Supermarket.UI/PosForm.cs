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
        private FlowLayoutPanel pnlItems;
        private BusinessFlowService _flowService;

        public PosForm()
        {
            InitializeComponent();
            _flowService = new BusinessFlowService(AppSettings.ConnectionString);
            SetupUI();
        }

        private void SetupUI()
        {
            this.Text = "POS / نقطة البيع";
            this.Size = new Size(1200, 800);
            this.WindowState = FormWindowState.Maximized;

            txtBarcode = new TextBox { Dock = DockStyle.Top, Font = new Font("Arial", 14), PlaceholderText = "Scan Barcode / امسح الباركود" };

            dgvInvoice = new DataGridView {
                Dock = DockStyle.Left,
                Width = 500,
                AutoGenerateColumns = false,
                Font = new Font("Arial", 12)
            };
            dgvInvoice.Columns.Add(new DataGridViewTextBoxColumn { Name = "Item", HeaderText = "Item / الصنف", Width = 200 });
            dgvInvoice.Columns.Add(new DataGridViewTextBoxColumn { Name = "Qty", HeaderText = "Qty / الكمية", Width = 80 });
            dgvInvoice.Columns.Add(new DataGridViewTextBoxColumn { Name = "Price", HeaderText = "Price / السعر", Width = 100 });
            dgvInvoice.Columns.Add(new DataGridViewTextBoxColumn { Name = "Total", HeaderText = "Total / الإجمالي", Width = 100 });

            pnlItems = new FlowLayoutPanel { Dock = DockStyle.Fill, AutoScroll = true, BackColor = Color.WhiteSmoke, Padding = new Padding(10) };

            Panel footer = new Panel { Dock = DockStyle.Bottom, Height = 100, BackColor = Color.FromArgb(45, 45, 48) };
            lblTotal = new Label { Text = "Total: 0.00", ForeColor = Color.Yellow, Font = new Font("Arial", 24, FontStyle.Bold), Location = new Point(20, 30), AutoSize = true };
            btnPay = new Button { Text = "Pay / دفع (F5)", Dock = DockStyle.Right, Width = 250, BackColor = Color.Green, ForeColor = Color.White, Font = new Font("Arial", 18, FontStyle.Bold), FlatStyle = FlatStyle.Flat };
            btnPay.FlatAppearance.BorderSize = 0;

            footer.Controls.Add(lblTotal);
            footer.Controls.Add(btnPay);

            this.Controls.Add(pnlItems);
            this.Controls.Add(dgvInvoice);
            this.Controls.Add(txtBarcode);
            this.Controls.Add(footer);

            LanguageHelper.ApplyLanguage(this);
        }

        private void InitializeComponent() { }
    }
}
