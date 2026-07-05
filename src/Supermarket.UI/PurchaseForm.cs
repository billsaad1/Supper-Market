using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;
using Supermarket.DAL;
using Supermarket.Models.Entities;

namespace Supermarket.UI
{
    public partial class PurchaseForm : Form
    {
        private DataGridView dgvItems;
        private TextBox txtBarcode, txtQty, txtPrice;
        private Label lblTotal;
        private IntegratedPurchaseRepository _purchaseRepo;
        private MasterDataRepository _itemRepo;
        private List<PurchaseInvoiceItem> _itemList = new List<PurchaseInvoiceItem>();

        public PurchaseForm()
        {
            InitializeComponent();
            string conn = AppSettings.ConnectionString;
            _purchaseRepo = new IntegratedPurchaseRepository(conn);
            _itemRepo = new MasterDataRepository(conn);
            SetupUI();
        }

        private void SetupUI()
        {
            this.Text = "Purchase Invoice / فاتورة مشتريات";
            this.Size = new Size(1000, 700);

            Panel top = new Panel { Dock = DockStyle.Top, Height = 100, BackColor = Color.WhiteSmoke };
            txtBarcode = new TextBox { Location = new Point(20, 20), Width = 200, Font = new Font("Arial", 14), PlaceholderText = "Barcode..." };
            txtQty = new TextBox { Location = new Point(230, 20), Width = 80, Font = new Font("Arial", 14), PlaceholderText = "Qty" };
            txtPrice = new TextBox { Location = new Point(320, 20), Width = 100, Font = new Font("Arial", 14), PlaceholderText = "Price" };
            Button btnAdd = new Button { Text = "Add / إضافة", Location = new Point(430, 18), Width = 100, Height = 35, BackColor = Color.Teal, ForeColor = Color.White };
            btnAdd.Click += BtnAdd_Click;

            txtBarcode.KeyDown += async (s, e) => {
                if (e.KeyCode == Keys.Enter && !string.IsNullOrEmpty(txtBarcode.Text)) {
                    var item = await _itemRepo.GetItemByBarcodeAsync(txtBarcode.Text);
                    if (item != null) {
                        txtPrice.Text = item.CostPrice.ToString();
                        txtQty.Focus();
                    }
                }
            };

            top.Controls.AddRange(new Control[] { txtBarcode, txtQty, txtPrice, btnAdd });

            dgvItems = new DataGridView { Dock = DockStyle.Fill, AllowUserToAddRows = false, AutoGenerateColumns = false, BackgroundColor = Color.White };
            dgvItems.Columns.Add(new DataGridViewTextBoxColumn { Name = "Name", HeaderText = "Item Name", Width = 300 });
            dgvItems.Columns.Add(new DataGridViewTextBoxColumn { Name = "Qty", HeaderText = "Qty", Width = 80 });
            dgvItems.Columns.Add(new DataGridViewTextBoxColumn { Name = "Price", HeaderText = "Cost Price", Width = 100 });
            dgvItems.Columns.Add(new DataGridViewTextBoxColumn { Name = "Total", HeaderText = "Total", Width = 100 });

            Panel footer = new Panel { Dock = DockStyle.Bottom, Height = 100, BackColor = Color.FromArgb(45, 45, 48) };
            lblTotal = new Label { Text = "Total: 0.00 SR", ForeColor = Color.Yellow, Font = new Font("Segoe UI", 24, FontStyle.Bold), Location = new Point(20, 30), AutoSize = true };
            Button btnSave = new Button { Text = "SAVE INVOICE / حفظ الفاتورة", Dock = DockStyle.Right, Width = 280, BackColor = Color.FromArgb(0, 122, 204), ForeColor = Color.White, Font = new Font("Segoe UI", 18, FontStyle.Bold), FlatStyle = FlatStyle.Flat };
            btnSave.Click += BtnSave_Click;

            footer.Controls.Add(lblTotal);
            footer.Controls.Add(btnSave);

            this.Controls.Add(dgvItems);
            this.Controls.Add(top);
            this.Controls.Add(footer);

            LanguageHelper.ApplyLanguage(this);
        }

        private async void BtnAdd_Click(object sender, EventArgs e)
        {
            var item = await _itemRepo.GetItemByBarcodeAsync(txtBarcode.Text);
            if (item != null)
            {
                decimal qty = decimal.Parse(txtQty.Text);
                decimal price = decimal.Parse(txtPrice.Text);
                decimal total = qty * price;

                dgvItems.Rows.Add(item.ItemName, qty, price, total);
                _itemList.Add(new PurchaseInvoiceItem { ItemID = item.ItemID, Quantity = qty, UnitPrice = price, TotalAmount = total });

                UpdateTotal();
                txtBarcode.Clear(); txtQty.Clear(); txtPrice.Clear(); txtBarcode.Focus();
            }
        }

        private void UpdateTotal()
        {
            decimal total = 0;
            foreach (DataGridViewRow row in dgvItems.Rows) total += Convert.ToDecimal(row.Cells["Total"].Value);
            lblTotal.Text = $"Total: {total:F2} SR";
        }

        private async void BtnSave_Click(object sender, EventArgs e)
        {
            if (_itemList.Count == 0) return;

            decimal total = decimal.Parse(lblTotal.Text.Replace("Total: ", "").Replace(" SR", ""));
            var invoice = new PurchaseInvoice {
                InvoiceNumber = "PUR-" + DateTime.Now.Ticks,
                StoreID = 1, SupplierID = 1,
                TotalAmount = total, TaxAmount = 0, NetAmount = total,
                CreatedBy = 1
            };

            await _purchaseRepo.SavePurchaseInvoiceAsync(invoice, _itemList);
            MessageBox.Show("Purchase Invoice Saved & Ledger Updated! / تم حفظ الفاتورة وترحيل الحسابات والمخزن");
            this.Close();
        }

        private void InitializeComponent() { }
    }
}
