using Supermarket.UI.Helpers;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;
using Supermarket.DAL;
using Supermarket.Models.Entities;
using Supermarket.BLL.Services;

namespace Supermarket.UI.Views
{
    public partial class PurchaseForm : Form
    {
        private DataGridView dgvItems;
        private TextBox txtBarcode, txtQty, txtPrice;
        private Label lblTotal;
        private ComboBox cbSupplier, cbStore;
        private IntegratedPurchaseRepository _purchaseRepo;
        private MasterDataRepository _itemRepo;
        private ContactRepository _contactRepo;
        private int? _existingId;
        private bool _isReadOnly;

        public PurchaseForm(int? invoiceId = null, bool readOnly = false)
        {
            InitializeComponent();
            _existingId = invoiceId;
            _isReadOnly = readOnly;
            string conn = AppSettings.ConnectionString;
            _purchaseRepo = new IntegratedPurchaseRepository(conn);
            _itemRepo = new MasterDataRepository(conn);
            _contactRepo = new ContactRepository(conn);
            SetupUI();
            LoadMetadata();
            if (_existingId.HasValue) LoadExistingInvoice();
        }

        private void SetupUI()
        {
            bool isArabic = LanguageHelper.TranslationService.CurrentLanguage == Supermarket.BLL.Services.Language.Arabic;
            this.Text = isArabic ? "فاتورة مشتريات" : "Purchase Invoice";
            this.Size = new Size(1100, 750);
            this.StartPosition = FormStartPosition.CenterScreen;
            this.RightToLeft = isArabic ? RightToLeft.Yes : RightToLeft.No;

            Panel top = new Panel { Dock = DockStyle.Top, Height = 150, BackColor = Color.FromArgb(248, 249, 250), Padding = new Padding(15) };

            cbSupplier = CreateTopComboBox(isArabic ? "المورد:" : "Supplier:", 20, 10);
            cbStore = CreateTopComboBox(isArabic ? "المخزن:" : "Store:", 250, 10);

            Panel pnlEntry = new Panel { Top = 70, Left = 15, Width = 1000, Height = 70 };

            Label lblBarcode = new Label { Text = isArabic ? "الباركود:" : "Barcode:", Location = new Point(5, 5), AutoSize = true };
            txtBarcode = new TextBox { Location = new Point(5, 25), Width = 220, Font = new Font("Segoe UI", 12) };

            Label lblQty = new Label { Text = isArabic ? "الكمية:" : "Qty:", Location = new Point(240, 5), AutoSize = true };
            txtQty = new TextBox { Location = new Point(240, 25), Width = 100, Font = new Font("Segoe UI", 12) };

            Label lblPrice = new Label { Text = isArabic ? "سعر الشراء:" : "Price:", Location = new Point(355, 5), AutoSize = true };
            txtPrice = new TextBox { Location = new Point(355, 25), Width = 120, Font = new Font("Segoe UI", 12) };

            Button btnAdd = new Button {
                Text = isArabic ? "إضافة (Enter)" : "Add (Enter)",
                Location = new Point(490, 22),
                Width = 130,
                Height = 35,
                BackColor = Color.FromArgb(0, 122, 204),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 10, FontStyle.Bold)
            };
            btnAdd.Click += BtnAdd_Click;

            txtBarcode.KeyDown += async (s, e) => {
                if (e.KeyCode == Keys.Enter && !string.IsNullOrEmpty(txtBarcode.Text)) {
                    var item = await _itemRepo.GetItemByBarcodeAsync(txtBarcode.Text);
                    if (item != null) {
                        txtPrice.Text = item.CostPrice.ToString();
                        txtQty.Focus();
                    } else MessageBox.Show(isArabic ? "الصنف غير موجود" : "Item not found");
                }
            };
            txtQty.KeyDown += (s, e) => { if (e.KeyCode == Keys.Enter) btnAdd.PerformClick(); };

            pnlEntry.Controls.AddRange(new Control[] { lblBarcode, txtBarcode, lblQty, txtQty, lblPrice, txtPrice, btnAdd });
            top.Controls.Add(pnlEntry);
            top.Controls.AddRange(new Control[] { cbSupplier.Parent, cbStore.Parent });

            dgvItems = new DataGridView {
                Dock = DockStyle.Fill,
                AllowUserToAddRows = false,
                AutoGenerateColumns = false,
                BackgroundColor = Color.White,
                BorderStyle = BorderStyle.None,
                RowTemplate = { Height = 35 },
                Font = new Font("Segoe UI", 10)
            };
            dgvItems.Columns.Add(new DataGridViewTextBoxColumn { Name = "ItemID", Visible = false });
            dgvItems.Columns.Add(new DataGridViewTextBoxColumn { Name = "Name", HeaderText = isArabic ? "الصنف" : "Item", Width = 350, ReadOnly = true });
            dgvItems.Columns.Add(new DataGridViewTextBoxColumn { Name = "Qty", HeaderText = isArabic ? "الكمية" : "Qty", Width = 100 });
            dgvItems.Columns.Add(new DataGridViewTextBoxColumn { Name = "Price", HeaderText = isArabic ? "السعر" : "Price", Width = 120 });
            dgvItems.Columns.Add(new DataGridViewTextBoxColumn { Name = "Total", HeaderText = isArabic ? "الإجمالي" : "Total", Width = 150, ReadOnly = true });

            if (!_isReadOnly) {
                var btnDelCol = new DataGridViewButtonColumn {
                    Name = "Delete",
                    Text = "X",
                    UseColumnTextForButtonValue = true,
                    Width = 50,
                    FlatStyle = FlatStyle.Flat
                };
                btnDelCol.DefaultCellStyle.ForeColor = Color.Red;
                dgvItems.Columns.Add(btnDelCol);
                dgvItems.CellClick += (s, e) => {
                    if (e.RowIndex >= 0 && e.ColumnIndex == dgvItems.Columns["Delete"].Index) {
                        dgvItems.Rows.RemoveAt(e.RowIndex);
                        UpdateTotal();
                    }
                };
            }

            dgvItems.CellValueChanged += (s, e) => {
                if (e.RowIndex >= 0 && (e.ColumnIndex == 2 || e.ColumnIndex == 3)) {
                    var row = dgvItems.Rows[e.RowIndex];
                    decimal q = Convert.ToDecimal(row.Cells["Qty"].Value);
                    decimal p = Convert.ToDecimal(row.Cells["Price"].Value);
                    row.Cells["Total"].Value = q * p;
                    UpdateTotal();
                }
            };

            Panel footer = new Panel { Dock = DockStyle.Bottom, Height = 100, BackColor = Color.FromArgb(33, 37, 41), Padding = new Padding(20) };
            lblTotal = new Label {
                Text = isArabic ? "الإجمالي: 0.00 ريال" : "Total: 0.00 YER",
                ForeColor = Color.Yellow,
                Font = new Font("Segoe UI", 24, FontStyle.Bold),
                Dock = DockStyle.Left,
                TextAlign = ContentAlignment.MiddleLeft,
                AutoSize = true
            };

            Button btnSave = new Button {
                Text = isArabic ? "حفظ الفاتورة (F5)" : "SAVE INVOICE (F5)",
                Dock = DockStyle.Right,
                Width = 300,
                BackColor = Color.FromArgb(40, 167, 69),
                ForeColor = Color.White,
                Font = new Font("Segoe UI", 18, FontStyle.Bold),
                FlatStyle = FlatStyle.Flat,
                Visible = !_isReadOnly
            };
            btnSave.FlatAppearance.BorderSize = 0;
            btnSave.Click += BtnSave_Click;

            Button btnPrint = new Button {
                Text = isArabic ? "طباعة" : "PRINT",
                Dock = DockStyle.Right,
                Width = 150,
                BackColor = Color.FromArgb(108, 117, 125),
                ForeColor = Color.White,
                Font = new Font("Segoe UI", 14, FontStyle.Bold),
                FlatStyle = FlatStyle.Flat,
                Margin = new Padding(0, 0, 10, 0)
            };
            btnPrint.Click += (s, e) => {
                List<ReceiptItem> printItems = new List<ReceiptItem>();
                foreach(DataGridViewRow row in dgvItems.Rows) {
                    printItems.Add(new ReceiptItem {
                        Name = row.Cells["Name"].Value.ToString(),
                        Qty = Convert.ToDecimal(row.Cells["Qty"].Value),
                        Price = Convert.ToDecimal(row.Cells["Price"].Value)
                    });
                }
                string invNum = _existingId.HasValue ? "PUR-" + _existingId.Value : "PUR-NEW";
                new ReceiptPrinter().PrintPurchaseInvoice(
                    invNum,
                    cbSupplier.Text,
                    cbStore.Text,
                    printItems,
                    decimal.Parse(lblTotal.Text.Split(':')[1].Replace("ريال", "").Trim())
                );
            };

            footer.Controls.Add(lblTotal);
            footer.Controls.Add(btnPrint);
            footer.Controls.Add(btnSave);

            if (_isReadOnly) {
                top.Enabled = false;
                dgvItems.ReadOnly = true;
            }

            this.Controls.Add(dgvItems);
            this.Controls.Add(top);
            this.Controls.Add(footer);

            this.KeyPreview = true;
            this.KeyDown += (s, e) => { if (e.KeyCode == Keys.F5) btnSave.PerformClick(); };

            LanguageHelper.ApplyLanguage(this);
        }

        private ComboBox CreateTopComboBox(string label, int x, int y)
        {
            Panel p = new Panel { Location = new Point(x, y), Width = 220, Height = 55 };
            p.Controls.Add(new Label { Text = label, Location = new Point(0, 0), AutoSize = true });
            ComboBox cb = new ComboBox { Location = new Point(0, 20), Width = 200, DropDownStyle = ComboBoxStyle.DropDownList, Font = new Font("Segoe UI", 10) };
            p.Controls.Add(cb);
            return cb;
        }

        private async void BtnAdd_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrEmpty(txtBarcode.Text)) return;
            var item = await _itemRepo.GetItemByBarcodeAsync(txtBarcode.Text);
            if (item != null)
            {
                decimal qty = decimal.TryParse(txtQty.Text, out decimal q) ? q : 1;
                decimal price = decimal.TryParse(txtPrice.Text, out decimal p) ? p : item.CostPrice;
                decimal total = qty * price;

                dgvItems.Rows.Add(item.ItemID, item.ItemName, qty, price, total);

                UpdateTotal();
                txtBarcode.Clear(); txtQty.Text = "1"; txtPrice.Clear(); txtBarcode.Focus();
            }
        }

        private void UpdateTotal()
        {
            bool isArabic = LanguageHelper.TranslationService.CurrentLanguage == Supermarket.BLL.Services.Language.Arabic;
            decimal total = 0;
            foreach (DataGridViewRow row in dgvItems.Rows) total += Convert.ToDecimal(row.Cells["Total"].Value);
            lblTotal.Text = isArabic ? $"الإجمالي: {total:F2} ريال" : $"Total: {total:F2} YER";
        }

        private async void LoadMetadata()
        {
            var suppliers = await _contactRepo.GetAllSuppliersAsync();
            cbSupplier.DataSource = suppliers;
            cbSupplier.DisplayMember = "SupplierName";
            cbSupplier.ValueMember = "SupplierID";

            var stores = await _itemRepo.GetAllStoresAsync();
            cbStore.DataSource = stores;
            cbStore.DisplayMember = "StoreName";
            cbStore.ValueMember = "StoreID";
        }

        private async void LoadExistingInvoice()
        {
            var header = await _purchaseRepo.GetPurchaseInvoiceHeaderAsync(_existingId.Value);
            var items = await _purchaseRepo.GetPurchaseInvoiceItemsAsync(_existingId.Value);

            cbSupplier.SelectedValue = header.SupplierID;
            cbStore.SelectedValue = header.StoreID;

            dgvItems.Rows.Clear();
            foreach (var item in items)
            {
                dgvItems.Rows.Add(item.ItemID, item.ItemName, item.Quantity, item.UnitPrice, item.TotalAmount);
            }
            UpdateTotal();
        }

        private async void BtnSave_Click(object sender, EventArgs e)
        {
            bool isArabic = LanguageHelper.TranslationService.CurrentLanguage == Supermarket.BLL.Services.Language.Arabic;
            if (dgvItems.Rows.Count == 0) return;
            if (cbSupplier.SelectedValue == null || cbStore.SelectedValue == null)
            {
                MessageBox.Show(isArabic ? "يرجى اختيار المورد والمخزن" : "Please select supplier and store");
                return;
            }

            decimal subtotal = 0;
            List<PurchaseInvoiceItem> items = new List<PurchaseInvoiceItem>();
            foreach (DataGridViewRow row in dgvItems.Rows) {
                decimal q = Convert.ToDecimal(row.Cells["Qty"].Value);
                decimal p = Convert.ToDecimal(row.Cells["Price"].Value);
                subtotal += (q * p);
                items.Add(new PurchaseInvoiceItem {
                    ItemID = (int)row.Cells["ItemID"].Value,
                    Quantity = q,
                    UnitPrice = p,
                    TotalAmount = q * p,
                    TaxAmount = 0 // Purchase price usually net for inventory, tax handled separately in ledger
                });
            }

            var invoice = new PurchaseInvoice {
                InvoiceNumber = "PUR-" + DateTime.Now.Ticks,
                StoreID = (int)cbStore.SelectedValue,
                SupplierID = (int)cbSupplier.SelectedValue,
                TotalAmount = subtotal, TaxAmount = 0, NetAmount = subtotal,
                CreatedBy = 1,
                PaymentType = "Cash"
            };

            try
            {
                if (_existingId.HasValue)
                {
                    await _purchaseRepo.DeletePurchaseInvoiceAsync(_existingId.Value);
                }

                await _purchaseRepo.SavePurchaseInvoiceAsync(invoice, items);
                MessageBox.Show(isArabic ? "تم حفظ فاتورة المشتريات وتحديث المخزن والقيود" : "Purchase Invoice Saved Successfully");
                this.DialogResult = DialogResult.OK;
                this.Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error: {ex.Message}");
            }
        }

        private void InitializeComponent() { }
    }
}
