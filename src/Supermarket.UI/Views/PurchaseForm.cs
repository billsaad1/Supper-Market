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
        private TextBox txtBarcode, txtQty, txtPrice, txtDiscount, txtExpiry;
        private TextBox txtOtherCharges, txtTotalDiscount;
        private Label lblTotal, lblTax, lblSubtotal, lblSupplierBalance;
        private ComboBox cbSupplier, cbStore, cbPaymentType;
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
            bool isArabic = LanguageHelper.TranslationService.CurrentLanguage == Language.Arabic;
            this.Text = isArabic ? "فاتورة مشتريات متطورة" : "Advanced Purchase Invoice";
            this.Size = new Size(1350, 850);
            this.BackColor = UITheme.ContentBg;
            this.StartPosition = FormStartPosition.CenterScreen;
            this.RightToLeft = isArabic ? RightToLeft.Yes : RightToLeft.No;

            // Header Section
            Panel pnlActionBar = new Panel { Dock = DockStyle.Top, Height = 60, BackColor = Color.White, BorderStyle = BorderStyle.FixedSingle };
            Button btnSave = CreateHeaderButton(isArabic ? "حفظ الفاتورة (F5)" : "SAVE (F5)", Color.FromArgb(40, 167, 69), 160);
            btnSave.Click += BtnSave_Click;
            btnSave.Visible = !_isReadOnly;
            Button btnPrintBarcodes = CreateHeaderButton(isArabic ? "طباعة باركود" : "LABELS", Color.FromArgb(108, 117, 125), 120);
            btnPrintBarcodes.Click += (s, e) => MessageBox.Show(isArabic ? "تم إرسال الملصقات للطابعة" : "Labels sent to printer");
            Button btnCancel = CreateHeaderButton(isArabic ? "إغلاق" : "CLOSE", Color.FromArgb(220, 53, 69), 120);
            btnCancel.Click += (s, e) => this.Close();
            pnlActionBar.Controls.AddRange(new Control[] { btnCancel, btnPrintBarcodes, btnSave });

            // Data Entry Header
            Panel pnlData = new Panel { Dock = DockStyle.Top, Height = 180, BackColor = Color.White, Padding = new Padding(20) };
            cbSupplier = CreateTopComboBox(isArabic ? "المورد:" : "Supplier:", 20, 15);
            cbSupplier.SelectedIndexChanged += async (s, e) => {
                if (cbSupplier.SelectedValue is int id) {
                    decimal bal = await _purchaseRepo.GetSupplierBalanceAsync(id);
                    lblSupplierBalance.Text = (isArabic ? "رصيد المورد: " : "Balance: ") + bal.ToString("N2");
                }
            };
            lblSupplierBalance = new Label { Location = new Point(20, 60), AutoSize = true, Font = new Font("Segoe UI", 9, FontStyle.Bold), ForeColor = Color.DarkRed };
            pnlData.Controls.Add(lblSupplierBalance);

            cbStore = CreateTopComboBox(isArabic ? "المستودع:" : "Warehouse:", 260, 15);
            cbPaymentType = CreateTopComboBox(isArabic ? "طريقة الدفع:" : "Payment:", 500, 15);
            cbPaymentType.Items.AddRange(new string[] { isArabic ? "نقدي" : "Cash", isArabic ? "آجل" : "Credit" });
            cbPaymentType.SelectedIndex = 0;

            Panel pnlEntry = new Panel { Top = 90, Left = 20, Width = 1300, Height = 80 };
            txtBarcode = CreateEntryField(isArabic ? "باركود:" : "Barcode:", 0, 0, 200, pnlEntry);
            txtQty = CreateEntryField(isArabic ? "الكمية:" : "Qty:", 210, 0, 80, pnlEntry);
            txtPrice = CreateEntryField(isArabic ? "سعر:" : "Price:", 300, 0, 100, pnlEntry);
            txtDiscount = CreateEntryField(isArabic ? "خصم %:" : "Disc%:", 410, 0, 80, pnlEntry);
            txtExpiry = CreateEntryField(isArabic ? "الصلاحية:" : "Expiry:", 500, 0, 120, pnlEntry);
            txtExpiry.PlaceholderText = "YYYY-MM-DD";

            Button btnAdd = new Button {
                Text = isArabic ? "+ إضافة" : "+ ADD",
                Location = new Point(640, 18), Width = 100, Height = 35,
                BackColor = UITheme.PrimaryColor, ForeColor = Color.White, FlatStyle = FlatStyle.Flat, Font = new Font("Segoe UI", 9, FontStyle.Bold)
            };
            btnAdd.Click += BtnAdd_Click;
            pnlEntry.Controls.Add(btnAdd);
            pnlData.Controls.AddRange(new Control[] { pnlEntry, cbSupplier.Parent, cbStore.Parent, cbPaymentType.Parent });

            // Grid Overhaul
            dgvItems = new DataGridView { Dock = DockStyle.Fill, AllowUserToAddRows = false, SelectionMode = DataGridViewSelectionMode.FullRowSelect };
            UITheme.ApplyModernStyle(dgvItems);
            dgvItems.Columns.Add(new DataGridViewTextBoxColumn { Name = "ItemID", Visible = false });
            dgvItems.Columns.Add(new DataGridViewTextBoxColumn { Name = "Name", HeaderText = isArabic ? "الصنف" : "Item", AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill });
            dgvItems.Columns.Add(new DataGridViewTextBoxColumn { Name = "Qty", HeaderText = isArabic ? "الكمية" : "Qty", Width = 80 });
            dgvItems.Columns.Add(new DataGridViewTextBoxColumn { Name = "Price", HeaderText = isArabic ? "السعر" : "Price", Width = 100 });
            dgvItems.Columns.Add(new DataGridViewTextBoxColumn { Name = "Disc", HeaderText = isArabic ? "خصم" : "Disc", Width = 80 });
            dgvItems.Columns.Add(new DataGridViewTextBoxColumn { Name = "Tax", HeaderText = isArabic ? "ضريبة" : "Tax", Width = 80 });
            dgvItems.Columns.Add(new DataGridViewTextBoxColumn { Name = "Total", HeaderText = isArabic ? "الإجمالي" : "Total", Width = 120, ReadOnly = true });
            dgvItems.Columns.Add(new DataGridViewTextBoxColumn { Name = "Expiry", HeaderText = isArabic ? "الصلاحية" : "Expiry", Width = 120 });

            DataGridViewButtonColumn btnDel = new DataGridViewButtonColumn { Name = "Delete", HeaderText = "", Text = "X", UseColumnTextForButtonValue = true, Width = 40, FlatStyle = FlatStyle.Flat };
            btnDel.DefaultCellStyle.ForeColor = Color.Red;
            dgvItems.Columns.Add(btnDel);
            dgvItems.CellContentClick += (s, e) => { if (e.ColumnIndex == dgvItems.Columns["Delete"].Index && !_isReadOnly) { dgvItems.Rows.RemoveAt(e.RowIndex); UpdateTotal(); } };

            // Advanced Summary Footer
            Panel pnlFooter = new Panel { Dock = DockStyle.Bottom, Height = 180, BackColor = Color.White, Padding = new Padding(20) };

            Panel pnlSummaryGrid = new Panel { Dock = DockStyle.Left, Width = 600 };
            txtTotalDiscount = CreateFooterField(isArabic ? "خصم إضافي:" : "Global Discount:", 0, 10, pnlSummaryGrid);
            txtOtherCharges = CreateFooterField(isArabic ? "تكاليف أخرى (نقل):" : "Other Charges:", 0, 50, pnlSummaryGrid);
            txtTotalDiscount.TextChanged += (s, e) => UpdateTotal();
            txtOtherCharges.TextChanged += (s, e) => UpdateTotal();

            lblSubtotal = new Label { Text = "Sub: 0.00", Font = UITheme.MainFont, Location = new Point(20, 100), AutoSize = true };
            lblTax = new Label { Text = "Tax: 0.00", Font = UITheme.MainFont, Location = new Point(20, 130), AutoSize = true, ForeColor = Color.Gray };
            lblTotal = new Label { Text = "0.00", Font = new Font("Segoe UI", 40, FontStyle.Bold), ForeColor = UITheme.SuccessColor, Dock = DockStyle.Right, TextAlign = ContentAlignment.MiddleRight, Width = 500 };

            pnlFooter.Controls.AddRange(new Control[] { pnlSummaryGrid, lblSubtotal, lblTax, lblTotal });

            this.Controls.Add(dgvItems);
            this.Controls.Add(pnlData);
            this.Controls.Add(pnlFooter);
            this.Controls.Add(pnlActionBar);

            txtBarcode.KeyDown += async (s, e) => {
                if (e.KeyCode == Keys.Enter && !string.IsNullOrEmpty(txtBarcode.Text)) {
                    var item = await _itemRepo.GetItemByBarcodeAsync(txtBarcode.Text);
                    if (item != null) { txtPrice.Text = item.CostPrice.ToString(); txtQty.Text = "1"; txtQty.Focus(); }
                }
            };

            LanguageHelper.ApplyLanguage(this);
        }

        private Button CreateHeaderButton(string text, Color color, int width)
        {
            Button btn = new Button { Text = text, Width = width, Dock = DockStyle.Right, BackColor = color, ForeColor = Color.White, FlatStyle = FlatStyle.Flat, Font = new Font("Segoe UI", 9, FontStyle.Bold), Margin = new Padding(5) };
            btn.FlatAppearance.BorderSize = 0;
            return btn;
        }

        private TextBox CreateEntryField(string label, int x, int y, int width, Panel parent)
        {
            parent.Controls.Add(new Label { Text = label, Location = new Point(x, y), AutoSize = true, Font = UITheme.GridFont });
            TextBox tb = new TextBox { Location = new Point(x, y + 25), Width = width, Font = UITheme.MainFont };
            parent.Controls.Add(tb);
            return tb;
        }

        private TextBox CreateFooterField(string label, int x, int y, Panel parent)
        {
            parent.Controls.Add(new Label { Text = label, Location = new Point(x, y + 5), AutoSize = true, Font = UITheme.GridFont });
            TextBox tb = new TextBox { Location = new Point(x + 150, y), Width = 120, Font = UITheme.MainFont, Text = "0" };
            parent.Controls.Add(tb);
            return tb;
        }

        private ComboBox CreateTopComboBox(string label, int x, int y)
        {
            Panel p = new Panel { Location = new Point(x, y), Width = 230, Height = 60 };
            p.Controls.Add(new Label { Text = label, Location = new Point(0, 0), AutoSize = true, Font = UITheme.GridFont });
            ComboBox cb = new ComboBox { Location = new Point(0, 25), Width = 210, DropDownStyle = ComboBoxStyle.DropDownList, Font = UITheme.MainFont };
            p.Controls.Add(cb);
            return cb;
        }

        private async void BtnAdd_Click(object sender, EventArgs e)
        {
            var item = await _itemRepo.GetItemByBarcodeAsync(txtBarcode.Text);
            if (item != null) {
                decimal q = decimal.TryParse(txtQty.Text, out decimal val) ? val : 1;
                decimal p = decimal.TryParse(txtPrice.Text, out decimal prc) ? prc : item.CostPrice;
                decimal dRate = decimal.TryParse(txtDiscount.Text, out decimal d) ? d : 0;
                decimal dAmt = (p * q) * (dRate / 100);
                decimal sub = (p * q) - dAmt;
                decimal tax = sub * 0.15m;
                dgvItems.Rows.Add(item.ItemID, item.ItemName, q, p, dAmt, tax, sub + tax, txtExpiry.Text);
                UpdateTotal();
                txtBarcode.Clear(); txtQty.Text = "1"; txtPrice.Clear(); txtDiscount.Text = "0"; txtBarcode.Focus();
            }
        }

        private void UpdateTotal()
        {
            bool isArabic = LanguageHelper.TranslationService.CurrentLanguage == Language.Arabic;
            decimal itemsTotal = dgvItems.Rows.Cast<DataGridViewRow>().Sum(r => Convert.ToDecimal(r.Cells["Total"].Value));
            decimal globalDisc = decimal.TryParse(txtTotalDiscount.Text, out decimal gd) ? gd : 0;
            decimal other = decimal.TryParse(txtOtherCharges.Text, out decimal oc) ? oc : 0;

            decimal net = itemsTotal - globalDisc + other;
            decimal tax = net * 0.15m; // Simplified: assumes global totals logic

            lblSubtotal.Text = (isArabic ? "المجموع: " : "Subtotal: ") + itemsTotal.ToString("N2");
            lblTotal.Text = net.ToString("N2") + (isArabic ? " ريال" : " YER");
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
            txtOtherCharges.Text = header.OtherCharges.ToString();
            txtTotalDiscount.Text = header.DiscountAmount.ToString();
            foreach (var i in items) dgvItems.Rows.Add(i.ItemID, i.ItemName, i.Quantity, i.UnitPrice, i.DiscountAmount, i.TaxAmount, i.TotalAmount, i.ExpiryDate?.ToShortDateString());
            UpdateTotal();
        }

        private async void BtnSave_Click(object sender, EventArgs e)
        {
            if (dgvItems.Rows.Count == 0 || cbSupplier.SelectedValue == null) return;

            var items = dgvItems.Rows.Cast<DataGridViewRow>().Select(r => new PurchaseInvoiceItem {
                ItemID = (int)r.Cells["ItemID"].Value,
                Quantity = Convert.ToDecimal(r.Cells["Qty"].Value),
                UnitPrice = Convert.ToDecimal(r.Cells["Price"].Value),
                DiscountAmount = Convert.ToDecimal(r.Cells["Disc"].Value),
                TaxAmount = Convert.ToDecimal(r.Cells["Tax"].Value),
                TotalAmount = Convert.ToDecimal(r.Cells["Total"].Value),
                ExpiryDate = DateTime.TryParse(r.Cells["Expiry"].Value?.ToString(), out DateTime d) ? d : (DateTime?)null
            }).ToList();

            decimal sub = items.Sum(i => i.TotalAmount);
            decimal gd = decimal.TryParse(txtTotalDiscount.Text, out decimal val) ? val : 0;
            decimal oc = decimal.TryParse(txtOtherCharges.Text, out decimal chg) ? chg : 0;

            var inv = new PurchaseInvoice {
                PurchaseID = _existingId ?? 0,
                InvoiceNumber = "PUR-" + DateTime.Now.Ticks,
                StoreID = (int)cbStore.SelectedValue,
                SupplierID = (int)cbSupplier.SelectedValue,
                TotalAmount = sub,
                DiscountAmount = gd,
                OtherCharges = oc,
                NetAmount = sub - gd + oc,
                CreatedBy = 1,
                PaymentType = cbPaymentType.SelectedIndex == 1 ? "Credit" : "Cash"
            };

            await _purchaseRepo.SavePurchaseInvoiceAsync(inv, items);
            MessageBox.Show(LanguageHelper.TranslationService.CurrentLanguage == Language.Arabic ? "تم الحفظ بنجاح" : "Saved successfully");
            this.Close();
        }

        private void InitializeComponent() { }
    }
}
