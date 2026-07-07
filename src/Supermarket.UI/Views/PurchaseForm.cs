using Supermarket.UI.Helpers;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;
using Supermarket.DAL;
using Supermarket.Models.Entities;
using Supermarket.BLL.Services;
using System.Linq;

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
            this.BackColor = UITheme.ContentBg;
            this.StartPosition = FormStartPosition.CenterScreen;
            this.RightToLeft = isArabic ? RightToLeft.Yes : RightToLeft.No;

            // Header Section (Modern ActionBar)
            Panel pnlActionBar = new Panel { Dock = DockStyle.Top, Height = 55, BackColor = Color.White, BorderStyle = BorderStyle.FixedSingle };

            Button btnSave = CreateHeaderButton(isArabic ? "حفظ الفاتورة (F5)" : "SAVE (F5)", UITheme.SuccessColor, 150);
            btnSave.Click += BtnSave_Click;
            btnSave.Visible = !_isReadOnly;

            Button btnPrint = CreateHeaderButton(isArabic ? "طباعة" : "PRINT", UITheme.InfoColor, 120);
            btnPrint.Click += BtnPrint_Click;

            Button btnCancel = CreateHeaderButton(isArabic ? "إلغاء" : "CANCEL", UITheme.DangerColor, 120);
            btnCancel.Click += (s, e) => this.Close();

            pnlActionBar.Controls.AddRange(new Control[] { btnCancel, btnPrint, btnSave });

            // Data Entry Section
            Panel pnlData = new Panel { Dock = DockStyle.Top, Height = 140, BackColor = Color.White, Padding = new Padding(15) };

            cbSupplier = CreateTopComboBox(isArabic ? "المورد:" : "Supplier:", 20, 10);
            cbStore = CreateTopComboBox(isArabic ? "المخزن:" : "Store:", 250, 10);

            Panel pnlEntry = new Panel { Top = 70, Left = 15, Width = 1000, Height = 70 };
            txtBarcode = CreateEntryField(isArabic ? "الباركود:" : "Barcode:", 0, 0, 220, pnlEntry);
            txtQty = CreateEntryField(isArabic ? "الكمية:" : "Qty:", 230, 0, 80, pnlEntry);
            txtPrice = CreateEntryField(isArabic ? "سعر الشراء:" : "Price:", 320, 0, 100, pnlEntry);

            Button btnAdd = new Button {
                Text = isArabic ? "إضافة" : "ADD",
                Location = new Point(430, 20),
                Width = 100, Height = 35,
                BackColor = UITheme.PrimaryColor,
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat
            };
            btnAdd.Click += BtnAdd_Click;
            pnlEntry.Controls.Add(btnAdd);

            pnlData.Controls.AddRange(new Control[] { pnlEntry, cbSupplier.Parent, cbStore.Parent });

            // Main Grid
            dgvItems = new DataGridView { Dock = DockStyle.Fill };
            UITheme.ApplyModernStyle(dgvItems);
            dgvItems.Columns.Add(new DataGridViewTextBoxColumn { Name = "ItemID", Visible = false });
            dgvItems.Columns.Add(new DataGridViewTextBoxColumn { Name = "Name", HeaderText = isArabic ? "الصنف" : "Item", Width = 350 });
            dgvItems.Columns.Add(new DataGridViewTextBoxColumn { Name = "Qty", HeaderText = isArabic ? "الكمية" : "Qty", Width = 100 });
            dgvItems.Columns.Add(new DataGridViewTextBoxColumn { Name = "Price", HeaderText = isArabic ? "السعر" : "Price", Width = 120 });
            dgvItems.Columns.Add(new DataGridViewTextBoxColumn { Name = "Total", HeaderText = isArabic ? "الإجمالي" : "Total", Width = 150, ReadOnly = true });

            // Summary Footer
            Panel pnlFooter = new Panel { Dock = DockStyle.Bottom, Height = 80, BackColor = Color.White, Padding = new Padding(20) };
            lblTotal = new Label { Text = "0.00", Font = new Font("Segoe UI", 24, FontStyle.Bold), ForeColor = UITheme.PrimaryColor, Dock = DockStyle.Right, TextAlign = ContentAlignment.MiddleRight, Width = 300 };
            pnlFooter.Controls.Add(lblTotal);

            this.Controls.Add(dgvItems);
            this.Controls.Add(pnlData);
            this.Controls.Add(pnlFooter);
            this.Controls.Add(pnlActionBar);

            txtBarcode.KeyDown += async (s, e) => {
                if (e.KeyCode == Keys.Enter && !string.IsNullOrEmpty(txtBarcode.Text)) {
                    var item = await _itemRepo.GetItemByBarcodeAsync(txtBarcode.Text);
                    if (item != null) { txtPrice.Text = item.CostPrice.ToString(); txtQty.Focus(); }
                }
            };

            this.KeyPreview = true;
            this.KeyDown += (s, e) => { if (e.KeyCode == Keys.F5) btnSave.PerformClick(); };
            LanguageHelper.ApplyLanguage(this);
            UpdateTotal();
        }

        private Button CreateHeaderButton(string text, Color color, int width)
        {
            Button btn = new Button { Text = text, Width = width, Dock = DockStyle.Right, BackColor = color, ForeColor = Color.White, FlatStyle = FlatStyle.Flat, Font = new Font("Segoe UI", 9, FontStyle.Bold) };
            btn.FlatAppearance.BorderSize = 0;
            return btn;
        }

        private TextBox CreateEntryField(string label, int x, int y, int width, Panel parent)
        {
            parent.Controls.Add(new Label { Text = label, Location = new Point(x, y), AutoSize = true, Font = UITheme.GridFont });
            TextBox tb = new TextBox { Location = new Point(x, y + 20), Width = width, Font = UITheme.MainFont };
            parent.Controls.Add(tb);
            return tb;
        }

        private ComboBox CreateTopComboBox(string label, int x, int y)
        {
            Panel p = new Panel { Location = new Point(x, y), Width = 220, Height = 55 };
            p.Controls.Add(new Label { Text = label, Location = new Point(0, 0), AutoSize = true, Font = UITheme.GridFont });
            ComboBox cb = new ComboBox { Location = new Point(0, 20), Width = 200, DropDownStyle = ComboBoxStyle.DropDownList, Font = UITheme.MainFont };
            p.Controls.Add(cb);
            return cb;
        }

        private async void BtnAdd_Click(object sender, EventArgs e)
        {
            var item = await _itemRepo.GetItemByBarcodeAsync(txtBarcode.Text);
            if (item != null) {
                decimal q = decimal.TryParse(txtQty.Text, out decimal val) ? val : 1;
                decimal p = decimal.TryParse(txtPrice.Text, out decimal prc) ? prc : item.CostPrice;
                dgvItems.Rows.Add(item.ItemID, item.ItemName, q, p, q * p);
                UpdateTotal();
                txtBarcode.Clear(); txtQty.Text = "1"; txtPrice.Clear(); txtBarcode.Focus();
            }
        }

        private void UpdateTotal()
        {
            decimal total = dgvItems.Rows.Cast<DataGridViewRow>().Sum(r => Convert.ToDecimal(r.Cells["Total"].Value));
            lblTotal.Text = total.ToString("N2") + (LanguageHelper.TranslationService.CurrentLanguage == Language.Arabic ? " ريال" : " YER");
        }

        private async void LoadMetadata()
        {
            cbSupplier.DataSource = await _contactRepo.GetAllSuppliersAsync();
            cbSupplier.DisplayMember = "SupplierName"; cbSupplier.ValueMember = "SupplierID";
            cbStore.DataSource = await _itemRepo.GetAllStoresAsync();
            cbStore.DisplayMember = "StoreName"; cbStore.ValueMember = "StoreID";
        }

        private async void LoadExistingInvoice()
        {
            var header = await _purchaseRepo.GetPurchaseInvoiceHeaderAsync(_existingId.Value);
            var items = await _purchaseRepo.GetPurchaseInvoiceItemsAsync(_existingId.Value);
            cbSupplier.SelectedValue = header.SupplierID; cbStore.SelectedValue = header.StoreID;
            foreach (var i in items) dgvItems.Rows.Add(i.ItemID, i.ItemName, i.Quantity, i.UnitPrice, i.TotalAmount);
            UpdateTotal();
        }

        private async void BtnSave_Click(object sender, EventArgs e)
        {
            if (dgvItems.Rows.Count == 0) return;
            var items = dgvItems.Rows.Cast<DataGridViewRow>().Select(r => new PurchaseInvoiceItem {
                ItemID = (int)r.Cells["ItemID"].Value,
                Quantity = Convert.ToDecimal(r.Cells["Qty"].Value),
                UnitPrice = Convert.ToDecimal(r.Cells["Price"].Value),
                TotalAmount = Convert.ToDecimal(r.Cells["Total"].Value)
            }).ToList();

            var inv = new PurchaseInvoice {
                InvoiceNumber = "PUR-" + DateTime.Now.Ticks,
                StoreID = (int)cbStore.SelectedValue,
                SupplierID = (int)cbSupplier.SelectedValue,
                TotalAmount = items.Sum(i => i.TotalAmount),
                CreatedBy = 1, PaymentType = "Cash"
            };

            await _purchaseRepo.SavePurchaseInvoiceAsync(inv, items);
            MessageBox.Show("Saved!");
            this.Close();
        }

        private void BtnPrint_Click(object sender, EventArgs e) { /* Print logic */ }
        private void InitializeComponent() { }
    }
}
