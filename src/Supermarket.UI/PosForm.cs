using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;
using Supermarket.BLL;
using Supermarket.DAL;
using Supermarket.Models.Entities;

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
        private MasterDataRepository _itemRepo;

        public PosForm()
        {
            InitializeComponent();
            string conn = AppSettings.ConnectionString;
            _flowService = new BusinessFlowService(conn);
            _itemRepo = new MasterDataRepository(conn);
            SetupUI();
        }

        private void SetupUI()
        {
            this.KeyPreview = true;
            this.KeyDown += (s, e) => {
                if (e.KeyCode == Keys.F5) btnPay.PerformClick();
            };

            this.Text = "Point of Sale / نقطة البيع";
            this.Size = new Size(1200, 800);
            this.WindowState = FormWindowState.Maximized;

            SplitContainer mainSplit = new SplitContainer { Dock = DockStyle.Fill, SplitterDistance = 720 };

            Panel leftPanel = new Panel { Dock = DockStyle.Fill };
            pnlItems = new FlowLayoutPanel { Dock = DockStyle.Fill, AutoScroll = true, BackColor = Color.WhiteSmoke };
            txtBarcode = new TextBox { Dock = DockStyle.Top, Font = new Font("Arial", 16), PlaceholderText = "Scan Barcode..." };
            txtBarcode.KeyDown += async (s, e) => {
                if (e.KeyCode == Keys.Enter && !string.IsNullOrEmpty(txtBarcode.Text)) {
                    await AddItemByBarcode(txtBarcode.Text);
                    txtBarcode.Clear();
                }
            };
            leftPanel.Controls.Add(pnlItems);
            leftPanel.Controls.Add(txtBarcode);

            Panel rightPanel = new Panel { Dock = DockStyle.Fill };
            dgvInvoice = new DataGridView {
                Dock = DockStyle.Fill,
                AllowUserToAddRows = false,
                AutoGenerateColumns = false,
                SelectionMode = DataGridViewSelectionMode.FullRowSelect
            };
            dgvInvoice.Columns.Add(new DataGridViewTextBoxColumn { Name = "ItemID", Visible = false });
            dgvInvoice.Columns.Add(new DataGridViewTextBoxColumn { Name = "Item", HeaderText = "Item", Width = 200 });
            dgvInvoice.Columns.Add(new DataGridViewTextBoxColumn { Name = "Qty", HeaderText = "Qty", Width = 70 });
            dgvInvoice.Columns.Add(new DataGridViewTextBoxColumn { Name = "UnitPrice", HeaderText = "Price", Width = 90 });
            dgvInvoice.Columns.Add(new DataGridViewTextBoxColumn { Name = "Total", HeaderText = "Total", Width = 100 });

            Panel footer = new Panel { Dock = DockStyle.Bottom, Height = 120, BackColor = Color.FromArgb(45, 45, 48) };
            lblTotal = new Label { Text = "TOTAL: 0.00 SR", ForeColor = Color.Yellow, Font = new Font("Segoe UI", 28, FontStyle.Bold), Location = new Point(20, 20), AutoSize = true };
            btnPay = new Button { Text = "PAY (F5)", Dock = DockStyle.Right, Width = 200, BackColor = Color.Green, ForeColor = Color.White, Font = new Font("Arial", 20, FontStyle.Bold) };
            btnPay.Click += BtnPay_Click;
            footer.Controls.Add(lblTotal);
            footer.Controls.Add(btnPay);

            rightPanel.Controls.Add(dgvInvoice);
            rightPanel.Controls.Add(footer);

            mainSplit.Panel1.Controls.Add(leftPanel);
            mainSplit.Panel2.Controls.Add(rightPanel);
            this.Controls.Add(mainSplit);

            LanguageHelper.ApplyLanguage(this);
        }

        private async System.Threading.Tasks.Task AddItemByBarcode(string barcode)
        {
            var item = await _itemRepo.GetItemByBarcodeAsync(barcode);
            if (item != null) {
                // Check if item already exists in grid
                bool found = false;
                foreach (DataGridViewRow row in dgvInvoice.Rows) {
                    if ((int)row.Cells["ItemID"].Value == item.ItemID) {
                        decimal newQty = Convert.ToDecimal(row.Cells["Qty"].Value) + 1;
                        row.Cells["Qty"].Value = newQty;
                        row.Cells["Total"].Value = newQty * (decimal)row.Cells["UnitPrice"].Value;
                        found = true;
                        break;
                    }
                }

                if (!found) {
                    dgvInvoice.Rows.Add(item.ItemID, item.ItemName, 1, item.SalePrice, item.SalePrice);
                }

                UpdateTotal();
            } else {
                MessageBox.Show("Item Not Found! / الصنف غير موجود");
            }
        }

        private void UpdateTotal()
        {
            decimal total = 0;
            foreach (DataGridViewRow row in dgvInvoice.Rows) total += Convert.ToDecimal(row.Cells["Total"].Value);
            lblTotal.Text = $"TOTAL: {total:F2} SR";
        }

        private async void BtnPay_Click(object sender, EventArgs e)
        {
            if (dgvInvoice.Rows.Count == 0) return;

            decimal netAmount = decimal.Parse(lblTotal.Text.Replace("TOTAL: ", "").Replace(" SR", ""));
            PaymentForm pay = new PaymentForm(netAmount);
            if (pay.ShowDialog() == DialogResult.OK) {
                decimal tax = netAmount * 0.15m;
                decimal totalWithTax = netAmount + tax;

                var invoice = new SalesInvoice {
                    InvoiceNumber = "POS-" + DateTime.Now.Ticks,
                    StoreID = 1, NetAmount = netAmount, TaxAmount = tax,
                    TotalAmount = totalWithTax, PaymentType = pay.PaymentMethod,
                    CreatedBy = 1
                };

                List<SalesInvoiceItem> items = new List<SalesInvoiceItem>();
                foreach (DataGridViewRow row in dgvInvoice.Rows) {
                    items.Add(new SalesInvoiceItem {
                        ItemID = (int)row.Cells["ItemID"].Value,
                        Quantity = Convert.ToDecimal(row.Cells["Qty"].Value),
                        UnitPrice = Convert.ToDecimal(row.Cells["UnitPrice"].Value),
                        TaxAmount = Convert.ToDecimal(row.Cells["Total"].Value) * 0.15m,
                        TotalAmount = Convert.ToDecimal(row.Cells["Total"].Value) * 1.15m
                    });
                }

                await _flowService.RecordSaleAsync(invoice, items);

                MessageBox.Show("Sale Saved & Printed! / تم حفظ البيع والطباعة");
                dgvInvoice.Rows.Clear();
                UpdateTotal();
            }
        }

        private void InitializeComponent() { }
    }
}
