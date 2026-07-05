using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;
using Supermarket.BLL;
using Supermarket.DAL;

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
        private PromotionEngine _promoEngine;

        public PosForm()
        {
            InitializeComponent();
            _flowService = new BusinessFlowService(AppSettings.ConnectionString);
            _promoEngine = new PromotionEngine();
            SetupUI();
        }

        private void SetupUI()
        {
            this.KeyPreview = true;
            this.KeyDown += (s, e) => {
                if (e.KeyCode == Keys.F5) btnPay.PerformClick();
                if (e.KeyCode == Keys.F1) OpenQuickSearch();
                if (e.KeyCode == Keys.F12) OpenCashDrawer();
            };

            this.Text = "POS / نقطة البيع";
            this.Size = new Size(1200, 800);
            this.WindowState = FormWindowState.Maximized;

            txtBarcode = new TextBox { Dock = DockStyle.Top, Font = new Font("Arial", 16), PlaceholderText = "Scan Barcode / امسح الباركود" };
            txtBarcode.KeyDown += TxtBarcode_KeyDown;

            dgvInvoice = new DataGridView {
                Dock = DockStyle.Left,
                Width = 550,
                AutoGenerateColumns = false,
                Font = new Font("Arial", 12),
                AllowUserToAddRows = false,
                BackgroundColor = Color.White
            };
            dgvInvoice.Columns.Add(new DataGridViewTextBoxColumn { Name = "Item", HeaderText = "Item / الصنف", Width = 200 });
            dgvInvoice.Columns.Add(new DataGridViewTextBoxColumn { Name = "Qty", HeaderText = "Qty / الكمية", Width = 70 });
            dgvInvoice.Columns.Add(new DataGridViewTextBoxColumn { Name = "Price", HeaderText = "Price / السعر", Width = 90 });
            dgvInvoice.Columns.Add(new DataGridViewTextBoxColumn { Name = "Disc", HeaderText = "Disc / خصم", Width = 70 });
            dgvInvoice.Columns.Add(new DataGridViewTextBoxColumn { Name = "Total", HeaderText = "Total / الإجمالي", Width = 100 });

            pnlItems = new FlowLayoutPanel { Dock = DockStyle.Fill, AutoScroll = true, BackColor = Color.WhiteSmoke, Padding = new Padding(10) };
            LoadCategories();

            Panel footer = new Panel { Dock = DockStyle.Bottom, Height = 120, BackColor = Color.FromArgb(45, 45, 48) };
            lblTotal = new Label { Text = "Total: 0.00 SR", ForeColor = Color.Yellow, Font = new Font("Segoe UI", 32, FontStyle.Bold), Location = new Point(20, 30), AutoSize = true };
            btnPay = new Button { Text = "PAY / دفع (F5)", Dock = DockStyle.Right, Width = 280, BackColor = Color.FromArgb(30, 150, 70), ForeColor = Color.White, Font = new Font("Segoe UI", 22, FontStyle.Bold), FlatStyle = FlatStyle.Flat };
            btnPay.FlatAppearance.BorderSize = 0;
            btnPay.Click += BtnPay_Click;

            footer.Controls.Add(lblTotal);
            footer.Controls.Add(btnPay);

            this.Controls.Add(pnlItems);
            this.Controls.Add(dgvInvoice);
            this.Controls.Add(txtBarcode);
            this.Controls.Add(footer);

            LanguageHelper.ApplyLanguage(this);
        }

        private void TxtBarcode_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter && !string.IsNullOrEmpty(txtBarcode.Text))
            {
                AddItemToGrid(txtBarcode.Text);
                txtBarcode.Clear();
            }
        }

        private void AddItemToGrid(string barcode)
        {
            decimal price = 10.00m;
            decimal qty = 1;
            decimal discount = _promoEngine.CalculateDiscount(0, qty, price);
            decimal total = (price * qty) - discount;

            dgvInvoice.Rows.Add("Sample Product", qty, price, discount, total);
            CalculateTotals();
        }

        private void CalculateTotals()
        {
            decimal total = 0;
            foreach (DataGridViewRow row in dgvInvoice.Rows)
            {
                total += Convert.ToDecimal(row.Cells["Total"].Value);
            }
            lblTotal.Text = $"Total: {total:F2} SR";
        }

        private void BtnPay_Click(object sender, EventArgs e)
        {
            decimal totalValue = decimal.Parse(lblTotal.Text.Replace("Total: ", "").Replace(" SR", ""));
            PaymentForm pay = new PaymentForm(totalValue);
            if (pay.ShowDialog() == DialogResult.OK)
            {
                // Print Receipt
                ReceiptPrinter printer = new ReceiptPrinter();
                printer.PrintReceipt("POS-001", "Admin", new List<ReceiptItem>(), totalValue, totalValue * 0.15m, totalValue * 1.15m, "QRDATA");

                MessageBox.Show("Sale Recorded & Printed Successfully! / تم تسجيل عملية البيع والطباعة بنجاح");
                dgvInvoice.Rows.Clear();
                CalculateTotals();
            }
        }

        private void OpenQuickSearch()
        {
            QuickSearchForm search = new QuickSearchForm();
            if (search.ShowDialog() == DialogResult.OK) AddItemToGrid(search.SelectedBarcode);
        }

        private void OpenCashDrawer() { MessageBox.Show("Cash Drawer Opened / تم فتح درج النقود"); }

        private void LoadCategories()
        {
            string[] cats = { "Beverages", "Snacks", "Dairy", "Bakery", "Vegetables", "Meat" };
            foreach (var cat in cats)
            {
                Button btn = new Button { Text = cat, Width = 130, Height = 60, BackColor = Color.FromArgb(0, 122, 204), ForeColor = Color.White, Font = new Font("Segoe UI", 10, FontStyle.Bold), FlatStyle = FlatStyle.Flat };
                btn.FlatAppearance.BorderSize = 0;
                pnlItems.Controls.Add(btn);
            }
        }

        private void InitializeComponent() { }
    }
}
