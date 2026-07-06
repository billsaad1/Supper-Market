using Supermarket.UI.Helpers;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;
using Supermarket.BLL.Services;
using Supermarket.DAL;
using Supermarket.Models.Entities;

namespace Supermarket.UI.Views
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
            Models.Entities.Item item = null;
            decimal qty = 1;

            // Handle Scale Barcode (starts with 21)
            var scaleData = ScaleBarcodeParser.Parse(barcode);
            if (scaleData.IsScaleItem)
            {
                item = await _itemRepo.GetItemByBarcodeAsync(scaleData.ItemCode);
                qty = scaleData.Weight;
            }
            else
            {
                item = await _itemRepo.GetItemByBarcodeAsync(barcode);
            }

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
                    dgvInvoice.Rows.Add(item.ItemID, item.ItemName, qty, item.SalePrice, qty * item.SalePrice);
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
                List<ReceiptItem> printItems = new List<ReceiptItem>();

                foreach (DataGridViewRow row in dgvInvoice.Rows) {
                    decimal qty = Convert.ToDecimal(row.Cells["Qty"].Value);
                    decimal price = Convert.ToDecimal(row.Cells["UnitPrice"].Value);

                    items.Add(new SalesInvoiceItem {
                        ItemID = (int)row.Cells["ItemID"].Value,
                        Quantity = qty,
                        UnitPrice = price,
                        TaxAmount = (qty * price) * 0.15m,
                        TotalAmount = (qty * price) * 1.15m
                    });

                    printItems.Add(new ReceiptItem {
                        Name = row.Cells["Item"].Value.ToString(),
                        Qty = qty,
                        Price = price
                    });
                }

                await _flowService.RecordSaleAsync(invoice, items);

                // Print Receipt
                new ReceiptPrinter().PrintReceipt(
                    invoice.InvoiceNumber, "Cashier", printItems,
                    netAmount, tax, totalWithTax, invoice.QRCode);

                MessageBox.Show("Sale Saved & Printed! / تم حفظ البيع والطباعة");
                dgvInvoice.Rows.Clear();
                UpdateTotal();
                txtBarcode.Focus();
            }
        }

        private void InitializeComponent() { }
    }
}
