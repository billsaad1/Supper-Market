using Supermarket.UI.Helpers;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using Supermarket.BLL.Services;
using Supermarket.DAL;
using Supermarket.Models.Entities;

namespace Supermarket.UI.Views
{
    public partial class PosForm : Form
    {
        private TextBox txtBarcode, txtSearch;
        private DataGridView dgvInvoice;
        private Label lblTotal, lblTax, lblSubtotal, lblDiscount, lblItemsCount, lblTotalQty;
        private Button btnPay, btnDiscount, btnClear, btnCredit, btnHold, btnResume, btnReturn, btnPrintLast, btnDrawer, btnPriceCheck;
        private FlowLayoutPanel pnlItems, pnlCategories, pnlQuick;
        private BusinessFlowService _flowService;
        private MasterDataRepository _itemRepo;
        private ContactRepository _contactRepo;
        private PromotionEngine _promoEngine;
        private ComboBox cbCustomer;
        private decimal _totalDiscount = 0;
        private bool _isReturnMode = false;
        private (SalesInvoice invoice, List<SalesInvoiceItem> items) _lastInvoice;

        public PosForm()
        {
            InitializeComponent();
            string conn = AppSettings.ConnectionString;
            _flowService = new BusinessFlowService(conn);
            _itemRepo = new MasterDataRepository(conn);
            _contactRepo = new ContactRepository(conn);
            _promoEngine = new PromotionEngine();
            SetupUI();
            LoadMetadata();
        }

        private void SetupUI()
        {
            this.KeyPreview = true;
            this.KeyDown += (s, e) => {
                if (e.KeyCode == Keys.F5) btnPay.PerformClick();
                if (e.KeyCode == Keys.F10) btnDiscount.PerformClick();
                if (e.KeyCode == Keys.F12) btnCredit.PerformClick();
                if (e.KeyCode == Keys.Escape) btnClear.PerformClick();
            };

            this.Text = "Point of Sale / نقطة البيع";
            this.BackColor = UITheme.ContentBg;

            bool isArabic = LanguageHelper.TranslationService.CurrentLanguage == Supermarket.BLL.Services.Language.Arabic;
            this.RightToLeft = isArabic ? RightToLeft.Yes : RightToLeft.No;

            // --- Top Panel (Action Bar) ---
            Panel pnlTopActions = new Panel { Dock = DockStyle.Top, Height = 55, BackColor = Color.White, BorderStyle = BorderStyle.FixedSingle };

            btnPay = CreateStyledButton(isArabic ? "حفظ (F5)" : "SAVE (F5)", Color.FromArgb(40, 167, 69), 120);
            btnPay.Click += BtnPay_Click;

            btnDiscount = CreateStyledButton(isArabic ? "خصم (F10)" : "DISCOUNT (F10)", Color.FromArgb(255, 193, 7), 130);
            btnDiscount.Click += BtnDiscount_Click;

            btnCredit = CreateStyledButton(isArabic ? "آجل (F12)" : "CREDIT (F12)", Color.FromArgb(23, 162, 184), 120);
            btnCredit.Click += BtnCredit_Click;

            btnHold = CreateStyledButton(isArabic ? "تعليق" : "HOLD", Color.FromArgb(108, 117, 125), 100);
            btnHold.Click += BtnHold_Click;

            btnResume = CreateStyledButton(isArabic ? "استعادة" : "RESUME", Color.FromArgb(108, 117, 125), 100);
            btnResume.Click += BtnResume_Click;

            btnReturn = CreateStyledButton(isArabic ? "مرتجع" : "RETURN", Color.FromArgb(153, 102, 255), 100);
            btnReturn.Click += (s, e) => {
                _isReturnMode = !_isReturnMode;
                btnReturn.BackColor = _isReturnMode ? Color.Maroon : Color.FromArgb(153, 102, 255);
                btnReturn.Text = _isReturnMode ? (isArabic ? "بيع" : "SALE") : (isArabic ? "مرتجع" : "RETURN");
                this.Text = _isReturnMode ? (isArabic ? "مرتجع مبيعات" : "Sales Return Mode") : (isArabic ? "نقطة البيع" : "Point of Sale");
            };

            btnPrintLast = CreateStyledButton(isArabic ? "آخر إيصال" : "LAST RPT", Color.FromArgb(100, 100, 100), 100);
            btnPrintLast.Click += (s, e) => {
                if (_lastInvoice.invoice != null) {
                    var printer = new ReceiptPrinter();
                    var receiptItems = _lastInvoice.items.Select(i => new ReceiptItem { Name = "Item", Qty = i.Quantity, Price = i.UnitPrice }).ToList();
                    printer.PrintReceipt(_lastInvoice.invoice.InvoiceNumber, "Admin", receiptItems, _lastInvoice.invoice.NetAmount, _lastInvoice.invoice.TaxAmount, _lastInvoice.invoice.TotalAmount, _lastInvoice.invoice.QRCode);
                } else MessageBox.Show(isArabic ? "لا توجد فاتورة سابقة" : "No last invoice found");
            };

            btnDrawer = CreateStyledButton(isArabic ? "الدرج" : "DRAWER", Color.FromArgb(100, 100, 100), 90);
            btnDrawer.Click += (s, e) => MessageBox.Show(isArabic ? "تم فتح درج النقود" : "Cash drawer opened");

            btnClear = CreateStyledButton(isArabic ? "جديد (Esc)" : "NEW (Esc)", Color.FromArgb(220, 53, 69), 110);
            btnClear.Click += (s, e) => ResetInvoice();

            btnPriceCheck = CreateStyledButton(isArabic ? "فحص سعر" : "PRICE CHK", Color.FromArgb(0, 150, 136), 100);
            btnPriceCheck.Click += async (s, e) => {
                string barcode = InputDialog.Show(isArabic ? "فحص سعر" : "Price Check", isArabic ? "امسح الباركود:" : "Scan Barcode:", "");
                if (!string.IsNullOrEmpty(barcode)) {
                    var item = await _itemRepo.GetItemByBarcodeAsync(barcode);
                    if (item != null) {
                        MessageBox.Show(isArabic ?
                            $"الصنف: {item.ItemName}\nالسعر: {item.SalePrice:N2} ريال\nالباركود: {item.Barcode}" :
                            $"Item: {item.ItemName}\nPrice: {item.SalePrice:N2} YER\nBarcode: {item.Barcode}",
                            isArabic ? "معلومات الصنف" : "Item Info");
                    } else {
                        MessageBox.Show(isArabic ? "الصنف غير موجود" : "Item not found");
                    }
                }
            };

            // From right to left: Save, Discount, Credit, Hold, Resume, Return, Last, Drawer, PriceChk, New
            pnlTopActions.Controls.AddRange(new Control[] { btnClear, btnPriceCheck, btnDrawer, btnPrintLast, btnReturn, btnResume, btnHold, btnCredit, btnDiscount, btnPay });

            // --- Secondary Header (Search & Customer) ---
            Panel pnlHeader = new Panel { Dock = DockStyle.Top, Height = 100, BackColor = Color.White, Padding = new Padding(15) };

            Label lblCust = new Label { Text = isArabic ? "العميل:" : "Customer:", Location = new Point(15, 15), AutoSize = true, Font = UITheme.MainFont };
            cbCustomer = new ComboBox { Width = 250, Font = new Font("Segoe UI", 11), DropDownStyle = ComboBoxStyle.DropDownList, Location = new Point(15, 40) };

            Button btnAddCust = new Button {
                Text = "+",
                Location = new Point(270, 40),
                Width = 35, Height = 30,
                BackColor = UITheme.PrimaryColor,
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 12, FontStyle.Bold)
            };
            btnAddCust.FlatAppearance.BorderSize = 0;
            btnAddCust.Click += async (s, e) => {
                string name = InputDialog.Show(isArabic ? "إضافة عميل" : "Add Customer", isArabic ? "اسم العميل:" : "Customer Name:", "");
                if (!string.IsNullOrEmpty(name)) {
                    int id = await _contactRepo.AddCustomerAsync(name);
                    LoadMetadata();
                    cbCustomer.SelectedValue = id;
                }
            };

            Label lblBarcode = new Label { Text = isArabic ? "باركود الصنف:" : "Item Barcode:", Location = new Point(320, 15), AutoSize = true, Font = UITheme.MainFont };
            txtBarcode = new TextBox { Width = 300, Font = new Font("Segoe UI", 14, FontStyle.Bold), Location = new Point(320, 40) };
            txtBarcode.KeyDown += async (s, e) => {
                if (e.KeyCode == Keys.Enter && !string.IsNullOrEmpty(txtBarcode.Text)) {
                    await AddItemByBarcode(txtBarcode.Text);
                    txtBarcode.Clear();
                }
            };

            txtSearch = new TextBox { Width = 300, Font = new Font("Segoe UI", 11), PlaceholderText = isArabic ? "بحث سريع بالاسم..." : "Quick Search by Name...", Location = new Point(640, 40) };
            txtSearch.TextChanged += async (s, e) => await SearchItems(txtSearch.Text);

            pnlHeader.Controls.AddRange(new Control[] { lblCust, cbCustomer, btnAddCust, lblBarcode, txtBarcode, txtSearch });

            // --- Main Layout ---
            SplitContainer mainSplit = new SplitContainer {
                Dock = DockStyle.Fill,
                SplitterDistance = 900,
                RightToLeft = isArabic ? RightToLeft.Yes : RightToLeft.No
            };

            // PANEL 1 (Right in RTL): Invoice Grid & Summary
            Panel pnlInvoice = new Panel { Dock = DockStyle.Fill, Padding = new Padding(10) };

            dgvInvoice = new DataGridView {
                Dock = DockStyle.Fill,
                AllowUserToAddRows = false,
                AutoGenerateColumns = false,
                RowTemplate = { Height = 45 }
            };
            UITheme.ApplyModernStyle(dgvInvoice);

            dgvInvoice.Columns.Add(new DataGridViewTextBoxColumn { Name = "ItemID", Visible = false });
            dgvInvoice.Columns.Add(new DataGridViewTextBoxColumn { Name = "Item", HeaderText = isArabic ? "اسم الصنف" : "Item Name", AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill, ReadOnly = true });

            DataGridViewButtonColumn btnMinus = new DataGridViewButtonColumn { Name = "Minus", HeaderText = "", Text = "-", UseColumnTextForButtonValue = true, Width = 30 };
            dgvInvoice.Columns.Add(btnMinus);

            dgvInvoice.Columns.Add(new DataGridViewTextBoxColumn { Name = "Qty", HeaderText = isArabic ? "الكمية" : "Qty", Width = 70 });

            DataGridViewButtonColumn btnPlus = new DataGridViewButtonColumn { Name = "Plus", HeaderText = "", Text = "+", UseColumnTextForButtonValue = true, Width = 30 };
            dgvInvoice.Columns.Add(btnPlus);

            DataGridViewComboBoxColumn colUnit = new DataGridViewComboBoxColumn { Name = "Unit", HeaderText = isArabic ? "الوحدة" : "Unit", Width = 80, FlatStyle = FlatStyle.Flat };
            colUnit.Items.AddRange(new string[] { "PCS", "Box", "KG" });
            dgvInvoice.Columns.Add(colUnit);

            dgvInvoice.Columns.Add(new DataGridViewTextBoxColumn { Name = "UnitPrice", HeaderText = isArabic ? "السعر" : "Price", Width = 90 });
            dgvInvoice.Columns.Add(new DataGridViewTextBoxColumn { Name = "Total", HeaderText = isArabic ? "الإجمالي" : "Total", Width = 120, ReadOnly = true });

            DataGridViewButtonColumn btnDel = new DataGridViewButtonColumn {
                Name = "Delete",
                HeaderText = "",
                Text = "X",
                UseColumnTextForButtonValue = true,
                Width = 40,
                FlatStyle = FlatStyle.Flat
            };
            btnDel.DefaultCellStyle.ForeColor = Color.Red;
            dgvInvoice.Columns.Add(btnDel);

            dgvInvoice.CellContentClick += (s, e) => {
                if (e.RowIndex < 0) return;
                if (e.ColumnIndex == dgvInvoice.Columns["Delete"].Index) RemoveSelectedItem();
                else if (e.ColumnIndex == dgvInvoice.Columns["Plus"].Index) AdjustQty(e.RowIndex, 1);
                else if (e.ColumnIndex == dgvInvoice.Columns["Minus"].Index) AdjustQty(e.RowIndex, -1);
            };

            dgvInvoice.CellValueChanged += (s, e) => {
                if (e.RowIndex >= 0 && (e.ColumnIndex == dgvInvoice.Columns["Qty"].Index || e.ColumnIndex == dgvInvoice.Columns["UnitPrice"].Index || e.ColumnIndex == dgvInvoice.Columns["Unit"].Index)) {
                    if (e.ColumnIndex == dgvInvoice.Columns["Unit"].Index) HandleUnitChange(e.RowIndex);
                    RecalculateRow(e.RowIndex);
                }
            };

            Panel pnlSummary = new Panel { Dock = DockStyle.Bottom, Height = 160, BackColor = Color.White, Padding = new Padding(20) };
            pnlSummary.Paint += (s, e) => { e.Graphics.DrawLine(Pens.LightGray, 0, 0, pnlSummary.Width, 0); };

            lblSubtotal = new Label { Text = "0.00", Font = new Font("Segoe UI", 12, FontStyle.Bold), Location = new Point(20, 10), AutoSize = true };
            lblTax = new Label { Text = "0.00", Font = new Font("Segoe UI", 10), Location = new Point(20, 40), AutoSize = true, ForeColor = Color.Gray };
            lblDiscount = new Label { Text = "0.00", Font = new Font("Segoe UI", 10), Location = new Point(20, 70), AutoSize = true, ForeColor = Color.FromArgb(220, 53, 69) };

            lblItemsCount = new Label { Text = "Items: 0", Font = new Font("Segoe UI", 10), Location = new Point(20, 100), AutoSize = true, ForeColor = Color.DimGray };
            lblTotalQty = new Label { Text = "Total Qty: 0", Font = new Font("Segoe UI", 10), Location = new Point(20, 125), AutoSize = true, ForeColor = Color.DimGray };

            lblTotal = new Label {
                Text = "0.00",
                Font = new Font("Segoe UI", 48, FontStyle.Bold),
                ForeColor = Color.FromArgb(40, 167, 69),
                Dock = DockStyle.Right,
                TextAlign = ContentAlignment.MiddleRight,
                Width = 400
            };

            pnlSummary.Controls.AddRange(new Control[] { lblSubtotal, lblTax, lblDiscount, lblItemsCount, lblTotalQty, lblTotal });
            pnlInvoice.Controls.Add(dgvInvoice);
            pnlInvoice.Controls.Add(pnlSummary);

            // PANEL 2 (Left in RTL): Categories & Items
            Panel pnlCatalog = new Panel { Dock = DockStyle.Fill, Padding = new Padding(10) };

            pnlCategories = new FlowLayoutPanel { Dock = DockStyle.Top, Height = 100, AutoScroll = true, BackColor = Color.FromArgb(248, 249, 251), Padding = new Padding(5) };
            pnlItems = new FlowLayoutPanel { Dock = DockStyle.Fill, AutoScroll = true, BackColor = Color.White, Padding = new Padding(5) };

            Label lblCatHeader = new Label { Text = isArabic ? "الأقسام" : "CATEGORIES", Dock = DockStyle.Top, Height = 25, Font = new Font("Segoe UI", 8, FontStyle.Bold), ForeColor = Color.Gray };
            Label lblItemsHeader = new Label { Text = isArabic ? "الأصناف" : "ITEMS", Dock = DockStyle.Top, Height = 25, Font = new Font("Segoe UI", 8, FontStyle.Bold), ForeColor = Color.Gray, Padding = new Padding(0, 5, 0, 0) };

            pnlQuick = new FlowLayoutPanel { Dock = DockStyle.Bottom, Height = 120, AutoScroll = true, BackColor = Color.FromArgb(245, 245, 245), Padding = new Padding(5) };
            Label lblQuickHeader = new Label { Text = isArabic ? "الأصناف السريعة" : "QUICK ITEMS", Dock = DockStyle.Bottom, Height = 25, Font = new Font("Segoe UI", 8, FontStyle.Bold), ForeColor = Color.DimGray, Padding = new Padding(0, 5, 0, 0) };

            pnlCatalog.Controls.Add(pnlItems);
            pnlCatalog.Controls.Add(lblItemsHeader);
            pnlCatalog.Controls.Add(pnlCategories);
            pnlCatalog.Controls.Add(lblCatHeader);
            pnlCatalog.Controls.Add(pnlQuick);
            pnlCatalog.Controls.Add(lblQuickHeader);

            mainSplit.Panel1.Controls.Add(pnlInvoice);
            mainSplit.Panel2.Controls.Add(pnlCatalog);

            this.Controls.Add(mainSplit);
            this.Controls.Add(pnlHeader);
            this.Controls.Add(pnlTopActions);

            LanguageHelper.ApplyLanguage(this);
            UpdateTotal();
        }

        private Button CreateStyledButton(string text, Color color, int width)
        {
            Button btn = new Button {
                Text = text,
                Dock = DockStyle.Right,
                Width = width,
                BackColor = color,
                ForeColor = Color.White,
                Font = new Font("Segoe UI", 10, FontStyle.Bold),
                FlatStyle = FlatStyle.Flat,
                Margin = new Padding(5)
            };
            btn.FlatAppearance.BorderSize = 0;
            return btn;
        }

        private async void LoadMetadata()
        {
            var customers = await _contactRepo.GetAllCustomersAsync();
            cbCustomer.DataSource = customers;
            cbCustomer.DisplayMember = "CustomerName";
            cbCustomer.ValueMember = "CustomerID";

            var allItems = await _itemRepo.GetAllItemsAsync();
            var quickItems = allItems.Take(8); // Dummy logic for quick items
            pnlQuick.Controls.Clear();
            foreach (var item in quickItems) pnlQuick.Controls.Add(CreateQuickItemButton(item));

            var categories = await _itemRepo.GetAllCategoriesAsync();
            pnlCategories.Controls.Clear();

            Button btnAll = CreateCategoryButton(LanguageHelper.TranslationService.CurrentLanguage == Supermarket.BLL.Services.Language.Arabic ? "الكل" : "All", 0);
            pnlCategories.Controls.Add(btnAll);

            foreach (var cat in categories)
            {
                pnlCategories.Controls.Add(CreateCategoryButton(cat.CategoryName, cat.CategoryID));
            }

            await LoadItemsByCategory(0);
        }

        private Button CreateCategoryButton(string text, int id)
        {
            bool isArabic = LanguageHelper.TranslationService.CurrentLanguage == Supermarket.BLL.Services.Language.Arabic;
            Color bg = id == 0 ? Color.FromArgb(80, 80, 85) : Color.FromArgb(0, 122, 255);

            Button btn = new Button {
                Text = text,
                Width = 100,
                Height = 65,
                BackColor = bg,
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 9, FontStyle.Bold),
                Tag = id,
                Margin = new Padding(3)
            };
            btn.FlatAppearance.BorderSize = 0;
            btn.Click += async (s, e) => {
                await LoadItemsByCategory((int)btn.Tag);
                foreach(Control c in pnlCategories.Controls) if(c is Button b) { b.BackColor = (int)b.Tag == 0 ? Color.FromArgb(80, 80, 85) : Color.FromArgb(0, 122, 255); b.ForeColor = Color.White; }
                btn.BackColor = Color.White;
                btn.ForeColor = bg;
            };
            return btn;
        }

        private async System.Threading.Tasks.Task LoadItemsByCategory(int categoryId)
        {
            var items = await _itemRepo.GetAllItemsAsync();
            if (categoryId > 0) items = items.Where(i => i.CategoryID == categoryId);
            DisplayItemButtons(items);
        }

        private void DisplayItemButtons(IEnumerable<Models.Entities.Item> items)
        {
            pnlItems.Controls.Clear();
            bool isArabic = LanguageHelper.TranslationService.CurrentLanguage == Language.Arabic;
            foreach (var item in items)
            {
                Panel pItem = new Panel { Width = 140, Height = 140, BackColor = Color.White, Margin = new Padding(8) };
                pItem.Paint += (s, e) => {
                    e.Graphics.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;
                    e.Graphics.DrawRectangle(new Pen(Color.FromArgb(220, 220, 225), 1), 0, 0, pItem.Width-1, pItem.Height-1);
                };

                Label lblPrice = new Label {
                    Text = item.SalePrice.ToString("N2") + (isArabic ? " ر.ي" : " YER"),
                    Dock = DockStyle.Bottom,
                    Height = 30,
                    BackColor = Color.FromArgb(245, 247, 250),
                    ForeColor = UITheme.PrimaryColor,
                    TextAlign = ContentAlignment.MiddleCenter,
                    Font = new Font("Segoe UI", 10, FontStyle.Bold)
                };

                Label lblStock = new Label {
                    Text = "📦 " + (item.MinimumStockLevel > 0 ? "In Stock" : "In Stock"), // Simplified for demo
                    Dock = DockStyle.Top,
                    Height = 20,
                    Font = new Font("Segoe UI", 7),
                    ForeColor = Color.Gray,
                    TextAlign = ContentAlignment.MiddleCenter
                };

                Button btn = new Button {
                    Text = item.ItemName,
                    Dock = DockStyle.Fill,
                    FlatStyle = FlatStyle.Flat,
                    BackColor = Color.White,
                    Tag = item,
                    Font = new Font("Segoe UI", 10, FontStyle.Bold),
                    Cursor = Cursors.Hand,
                    TextAlign = ContentAlignment.MiddleCenter,
                    Padding = new Padding(5)
                };
                btn.FlatAppearance.BorderSize = 0;
                btn.FlatAppearance.MouseOverBackColor = Color.FromArgb(250, 251, 252);
                btn.Click += (s, e) => { AddItemToInvoice((Models.Entities.Item)btn.Tag); txtBarcode.Focus(); };

                pItem.Controls.Add(btn);
                pItem.Controls.Add(lblStock);
                pItem.Controls.Add(lblPrice);
                pnlItems.Controls.Add(pItem);
            }
        }

        private async System.Threading.Tasks.Task SearchItems(string query)
        {
            if (string.IsNullOrWhiteSpace(query)) { await LoadItemsByCategory(0); return; }
            var items = await _itemRepo.GetAllItemsAsync();
            DisplayItemButtons(items.Where(i => i.ItemName.Contains(query, StringComparison.OrdinalIgnoreCase)));
        }

        private void AddItemToInvoice(Models.Entities.Item item, decimal qty = 1)
        {
            if (_isReturnMode) qty = -Math.Abs(qty);

            foreach (DataGridViewRow row in dgvInvoice.Rows) {
                if ((int)row.Cells["ItemID"].Value == item.ItemID) {
                    decimal currentQty = Convert.ToDecimal(row.Cells["Qty"].Value);
                    decimal newQty = currentQty + qty;
                    row.Cells["Qty"].Value = newQty;
                    return;
                }
            }
            // Add: ItemID, Name, Minus, Qty, Plus, Unit, Price, Total, Delete
            dgvInvoice.Rows.Add(item.ItemID, item.ItemName, "-", qty, "+", "PCS", item.SalePrice, qty * item.SalePrice);
            UpdateTotal();
        }

        private void AdjustQty(int rowIndex, decimal diff)
        {
            decimal current = Convert.ToDecimal(dgvInvoice.Rows[rowIndex].Cells["Qty"].Value);
            decimal newVal = Math.Max(0.01m, current + diff);
            dgvInvoice.Rows[rowIndex].Cells["Qty"].Value = newVal;
        }

        private async void HandleUnitChange(int rowIndex)
        {
            var row = dgvInvoice.Rows[rowIndex];
            string unit = row.Cells["Unit"].Value?.ToString();
            int itemId = (int)row.Cells["ItemID"].Value;
            var items = await _itemRepo.GetAllItemsAsync();
            var item = items.FirstOrDefault(i => i.ItemID == itemId);

            if (item != null)
            {
                if (unit == "Box") row.Cells["UnitPrice"].Value = item.SalePrice * 10; // Dummy multiplier
                else if (unit == "KG") row.Cells["UnitPrice"].Value = item.SalePrice;
                else row.Cells["UnitPrice"].Value = item.SalePrice;
            }
        }

        private void RecalculateRow(int rowIndex)
        {
            var row = dgvInvoice.Rows[rowIndex];
            if (decimal.TryParse(row.Cells["Qty"].Value?.ToString(), out decimal q) &&
                decimal.TryParse(row.Cells["UnitPrice"].Value?.ToString(), out decimal p))
            {
                int itemId = (int)row.Cells["ItemID"].Value;
                decimal promoDiscount = _promoEngine.CalculateDiscount(itemId, q, p);
                row.Cells["Total"].Value = (q * p) - promoDiscount;
                UpdateTotal();
            }
        }

        private void UpdateTotal()
        {
            bool isArabic = LanguageHelper.TranslationService.CurrentLanguage == Supermarket.BLL.Services.Language.Arabic;
            decimal subtotal = dgvInvoice.Rows.Cast<DataGridViewRow>().Sum(r => Convert.ToDecimal(r.Cells["Total"].Value));
            decimal tax = subtotal * 0.15m;
            decimal grandTotal = (subtotal + tax) - _totalDiscount;

            decimal totalQty = dgvInvoice.Rows.Cast<DataGridViewRow>().Sum(r => Convert.ToDecimal(r.Cells["Qty"].Value));
            int itemsCount = dgvInvoice.Rows.Count;

            lblSubtotal.Text = (isArabic ? "المجموع: " : "Subtotal: ") + subtotal.ToString("N2");
            lblTax.Text = (isArabic ? "الضريبة (15%): " : "Tax (15%): ") + tax.ToString("N2");
            lblDiscount.Text = (isArabic ? "خصم: " : "Discount: ") + _totalDiscount.ToString("N2");

            lblItemsCount.Text = (isArabic ? "عدد الأصناف: " : "Total Items: ") + itemsCount;
            lblTotalQty.Text = (isArabic ? "إجمالي الكمية: " : "Total Qty: ") + totalQty.ToString("N2");

            lblTotal.Text = grandTotal.ToString("N2") + (isArabic ? " ريال" : " YER");
        }

        private void RemoveSelectedItem() { if (dgvInvoice.CurrentRow != null) { dgvInvoice.Rows.Remove(dgvInvoice.CurrentRow); UpdateTotal(); } }
        private void ResetInvoice() { dgvInvoice.Rows.Clear(); _totalDiscount = 0; UpdateTotal(); txtBarcode.Focus(); }

        private async System.Threading.Tasks.Task AddItemByBarcode(string barcode)
        {
            // 1. Check for Scale Barcode
            var scaleData = ScaleBarcodeParser.Parse(barcode);
            if (scaleData.IsScaleItem)
            {
                var items = await _itemRepo.GetAllItemsAsync();
                var item = items.FirstOrDefault(i => (i.Barcode ?? "").EndsWith(scaleData.ItemCode));
                if (item != null)
                {
                    AddItemToInvoice(item, scaleData.Weight);
                    return;
                }
            }

            // 2. Standard Barcode
            var standardItem = await _itemRepo.GetItemByBarcodeAsync(barcode);
            if (standardItem != null) AddItemToInvoice(standardItem);
            else MessageBox.Show(LanguageHelper.TranslationService.CurrentLanguage == Supermarket.BLL.Services.Language.Arabic ? "الصنف غير موجود" : "Item Not Found");
        }

        private void BtnDiscount_Click(object sender, EventArgs e)
        {
            string res = InputDialog.Show("Discount", "Enter Amount:", _totalDiscount.ToString());
            if (decimal.TryParse(res, out decimal d)) { _totalDiscount = d; UpdateTotal(); }
        }

        private void BtnCredit_Click(object sender, EventArgs e) => PerformPayment("Credit / آجل");
        private void BtnPay_Click(object sender, EventArgs e) => PerformPayment(null);

        private List<HeldInvoice> _heldInvoices = new List<HeldInvoice>();
        private void BtnHold_Click(object sender, EventArgs e)
        {
            if (dgvInvoice.Rows.Count == 0) return;
            _heldInvoices.Add(new HeldInvoice {
                Rows = dgvInvoice.Rows.Cast<DataGridViewRow>().Select(r => new HeldRow {
                    ItemID = (int)r.Cells["ItemID"].Value, Name = r.Cells["Item"].Value.ToString(),
                    Qty = Convert.ToDecimal(r.Cells["Qty"].Value), Price = Convert.ToDecimal(r.Cells["UnitPrice"].Value),
                    Total = Convert.ToDecimal(r.Cells["Total"].Value)
                }).ToList(),
                Discount = _totalDiscount, CustomerValue = cbCustomer.SelectedValue
            });
            ResetInvoice();
            MessageBox.Show("Invoice Held / تم تعليق الفاتورة");
        }

        private void BtnResume_Click(object sender, EventArgs e)
        {
            if (!_heldInvoices.Any()) return;
            var held = _heldInvoices.Last();
            dgvInvoice.Rows.Clear();
            foreach (var r in held.Rows) dgvInvoice.Rows.Add(r.ItemID, r.Name, "-", r.Qty, "+", r.Price, r.Total);
            _totalDiscount = held.Discount; cbCustomer.SelectedValue = held.CustomerValue;
            _heldInvoices.Remove(held);
            UpdateTotal();
        }

        private class HeldInvoice { public List<HeldRow> Rows; public decimal Discount; public object CustomerValue; }
        private class HeldRow { public int ItemID; public string Name; public decimal Qty; public decimal Price; public decimal Total; }

        private async void PerformPayment(string method)
        {
            if (dgvInvoice.Rows.Count == 0) return;

            decimal subtotal = dgvInvoice.Rows.Cast<DataGridViewRow>().Sum(r => Convert.ToDecimal(r.Cells["Total"].Value));
            decimal tax = subtotal * 0.15m;
            decimal grandTotal = (subtotal + tax) - _totalDiscount;

            decimal cashAmt = grandTotal, cardAmt = 0;

            // Launch Payment Dialog if no specific method provided (like Credit from button)
            if (string.IsNullOrEmpty(method))
            {
                using (var payForm = new PaymentForm(grandTotal))
                {
                    if (payForm.ShowDialog() != DialogResult.OK) return;
                    method = payForm.PaymentMethod;
                    cashAmt = payForm.CashAmount;
                    cardAmt = payForm.CardAmount;
                }
            }

            string invNum = "POS-" + DateTime.Now.Ticks;
            var invoice = new SalesInvoice {
                InvoiceNumber = invNum,
                StoreID = 1, NetAmount = subtotal, TaxAmount = tax, DiscountAmount = _totalDiscount,
                TotalAmount = grandTotal, PaymentType = method,
                CashAmount = cashAmt, CardAmount = cardAmt,
                CustomerID = (cbCustomer.SelectedValue != null && (int)cbCustomer.SelectedValue > 0) ? (int)cbCustomer.SelectedValue : null,
                CreatedBy = 1
            };

            var items = dgvInvoice.Rows.Cast<DataGridViewRow>().Select(r => new SalesInvoiceItem {
                ItemID = (int)r.Cells["ItemID"].Value,
                Quantity = Convert.ToDecimal(r.Cells["Qty"].Value),
                UnitPrice = Convert.ToDecimal(r.Cells["UnitPrice"].Value),
                TotalAmount = Convert.ToDecimal(r.Cells["Total"].Value)
            }).ToList();

            await _flowService.RecordSaleAsync(invoice, items);
            _lastInvoice = (invoice, items);

            // Print Receipt with ZATCA QR
            var printer = new ReceiptPrinter();
            var receiptItems = dgvInvoice.Rows.Cast<DataGridViewRow>().Select(r => new ReceiptItem {
                Name = r.Cells["Item"].Value.ToString(),
                Qty = Convert.ToDecimal(r.Cells["Qty"].Value),
                Price = Convert.ToDecimal(r.Cells["UnitPrice"].Value)
            }).ToList();

            string qr = ZatcaHelper.GenerateQrCode("Supermarket", "1234567890", DateTime.Now, grandTotal, tax);
            printer.PrintReceipt(invNum, "Admin", receiptItems, subtotal, tax, grandTotal, qr);

            MessageBox.Show(LanguageHelper.TranslationService.CurrentLanguage == Language.Arabic ? "تم حفظ العملية بنجاح" : "Sale saved successfully!");
            ResetInvoice();
        }

        private Button CreateQuickItemButton(Models.Entities.Item item)
        {
            Button btn = new Button {
                Text = item.ItemName,
                Width = 100,
                Height = 80,
                BackColor = Color.FromArgb(235, 235, 235),
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 8, FontStyle.Bold),
                Tag = item,
                Margin = new Padding(3)
            };
            btn.FlatAppearance.BorderSize = 0;
            btn.Click += (s, e) => AddItemToInvoice((Models.Entities.Item)btn.Tag);
            return btn;
        }

        private void InitializeComponent() { }
    }
}
