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

            bool isArabic = LanguageHelper.TranslationService.CurrentLanguage == Language.Arabic;
            SplitContainer mainSplit = new SplitContainer {
                Dock = DockStyle.Fill,
                SplitterDistance = 800,
                RightToLeft = isArabic ? RightToLeft.Yes : RightToLeft.No
            };

            // Left (or Right in RTL) Panel: Invoice Details
            Panel invoicePanel = new Panel { Dock = DockStyle.Fill, Padding = new Padding(10) };
            txtBarcode = new TextBox {
                Dock = DockStyle.Top,
                Font = new Font("Segoe UI", 18),
                PlaceholderText = isArabic ? "امسح الباركود هنا..." : "Scan Barcode...",
                Height = 50
            };
            txtBarcode.KeyDown += async (s, e) => {
                if (e.KeyCode == Keys.Enter && !string.IsNullOrEmpty(txtBarcode.Text)) {
                    await AddItemByBarcode(txtBarcode.Text);
                    txtBarcode.Clear();
                }
            };

            dgvInvoice = new DataGridView {
                Dock = DockStyle.Fill,
                AllowUserToAddRows = false,
                AutoGenerateColumns = false,
                SelectionMode = DataGridViewSelectionMode.FullRowSelect,
                BackgroundColor = Color.White,
                BorderStyle = BorderStyle.None,
                RowTemplate = { Height = 35 },
                Font = new Font("Segoe UI", 11)
            };
            dgvInvoice.Columns.Add(new DataGridViewTextBoxColumn { Name = "ItemID", Visible = false });
            dgvInvoice.Columns.Add(new DataGridViewTextBoxColumn { Name = "Item", HeaderText = isArabic ? "الصنف" : "Item", Width = 300 });
            dgvInvoice.Columns.Add(new DataGridViewTextBoxColumn { Name = "Qty", HeaderText = isArabic ? "الكمية" : "Qty", Width = 80 });
            dgvInvoice.Columns.Add(new DataGridViewTextBoxColumn { Name = "UnitPrice", HeaderText = isArabic ? "السعر" : "Price", Width = 100 });
            dgvInvoice.Columns.Add(new DataGridViewTextBoxColumn { Name = "Total", HeaderText = isArabic ? "الإجمالي" : "Total", Width = 120 });

            Panel footer = new Panel { Dock = DockStyle.Bottom, Height = 130, BackColor = Color.FromArgb(33, 37, 41), Padding = new Padding(10) };
            lblTotal = new Label {
                Text = isArabic ? "الإجمالي: 0.00 ريال" : "TOTAL: 0.00 YER",
                ForeColor = Color.FromArgb(255, 193, 7),
                Font = new Font("Segoe UI", 32, FontStyle.Bold),
                Dock = DockStyle.Fill,
                TextAlign = ContentAlignment.MiddleCenter
            };

            btnPay = new Button {
                Text = isArabic ? "دفع (F5)" : "PAY (F5)",
                Dock = DockStyle.Right,
                Width = 250,
                BackColor = Color.FromArgb(40, 167, 69),
                ForeColor = Color.White,
                Font = new Font("Segoe UI", 22, FontStyle.Bold),
                FlatStyle = FlatStyle.Flat
            };
            btnPay.FlatAppearance.BorderSize = 0;
            btnPay.Click += BtnPay_Click;

            footer.Controls.Add(lblTotal);
            footer.Controls.Add(btnPay);

            invoicePanel.Controls.Add(dgvInvoice);
            invoicePanel.Controls.Add(new Panel { Dock = DockStyle.Top, Height = 10 }); // Spacer
            invoicePanel.Controls.Add(txtBarcode);
            invoicePanel.Controls.Add(footer);

            // Right (or Left in RTL) Panel: Categories/Items Grid
            Panel selectionPanel = new Panel { Dock = DockStyle.Fill, BackColor = Color.FromArgb(248, 249, 250) };
            pnlItems = new FlowLayoutPanel { Dock = DockStyle.Fill, AutoScroll = true, Padding = new Padding(10) };
            selectionPanel.Controls.Add(pnlItems);

            mainSplit.Panel1.Controls.Add(invoicePanel);
            mainSplit.Panel2.Controls.Add(selectionPanel);
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
                        decimal newQty = Convert.ToDecimal(row.Cells["Qty"].Value) + qty;
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
            bool isArabic = LanguageHelper.TranslationService.CurrentLanguage == Language.Arabic;
            decimal total = 0;
            foreach (DataGridViewRow row in dgvInvoice.Rows) total += Convert.ToDecimal(row.Cells["Total"].Value);
            lblTotal.Text = isArabic ? $"الإجمالي: {total:F2} ريال" : $"TOTAL: {total:F2} YER";
        }

        private async void BtnPay_Click(object sender, EventArgs e)
        {
            bool isArabic = LanguageHelper.TranslationService.CurrentLanguage == Language.Arabic;
            if (dgvInvoice.Rows.Count == 0) return;

            string totalText = lblTotal.Text;
            if (isArabic) totalText = totalText.Replace("الإجمالي: ", "").Replace(" ريال", "");
            else totalText = totalText.Replace("TOTAL: ", "").Replace(" YER", "");

            decimal netAmount = decimal.Parse(totalText);
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
