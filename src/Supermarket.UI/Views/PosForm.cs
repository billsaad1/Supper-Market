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
        private TextBox txtBarcode, txtSearch;
        private DataGridView dgvInvoice;
        private Label lblTotal, lblTax, lblSubtotal, lblDiscount, lblItemsCount;
        private Button btnPay, btnDiscount, btnClear, btnCredit, btnHold, btnResume, btnReturn;
        private FlowLayoutPanel pnlItems, pnlCategories;
        private BusinessFlowService _flowService;
        private MasterDataRepository _itemRepo;
        private ContactRepository _contactRepo;
        private ComboBox cbCustomer;
        private decimal _totalDiscount = 0;

        public PosForm()
        {
            InitializeComponent();
            string conn = AppSettings.ConnectionString;
            _flowService = new BusinessFlowService(conn);
            _itemRepo = new MasterDataRepository(conn);
            _contactRepo = new ContactRepository(conn);
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
            this.Size = new Size(1300, 850);
            this.WindowState = FormWindowState.Maximized;

            bool isArabic = LanguageHelper.TranslationService.CurrentLanguage == Supermarket.BLL.Services.Language.Arabic;
            this.RightToLeft = isArabic ? RightToLeft.Yes : RightToLeft.No;

            // Top Panel: Search & Customer
            Panel pnlTop = new Panel { Dock = DockStyle.Top, Height = 60, BackColor = Color.FromArgb(248, 249, 250), Padding = new Padding(10) };
            txtSearch = new TextBox { Width = 300, Font = new Font("Segoe UI", 12), PlaceholderText = isArabic ? "بحث عن صنف..." : "Search Item...", Location = new Point(10, 15) };
            txtSearch.TextChanged += async (s, e) => await SearchItems(txtSearch.Text);

            cbCustomer = new ComboBox { Width = 250, Font = new Font("Segoe UI", 12), DropDownStyle = ComboBoxStyle.DropDownList, Location = new Point(320, 15) };
            pnlTop.Controls.AddRange(new Control[] { txtSearch, cbCustomer });

            // Bottom Panel: Action Buttons
            Panel pnlBottom = new Panel { Dock = DockStyle.Bottom, Height = 80, BackColor = Color.FromArgb(33, 37, 41), Padding = new Padding(10) };

            btnPay = CreateActionButton(isArabic ? "دفع (F5)" : "PAY (F5)", Color.FromArgb(40, 167, 69), 200);
            btnPay.Click += BtnPay_Click;

            btnDiscount = CreateActionButton(isArabic ? "خصم (F10)" : "DISCOUNT (F10)", Color.FromArgb(255, 193, 7), 180);
            btnDiscount.Click += BtnDiscount_Click;

            btnCredit = CreateActionButton(isArabic ? "بيع آجل (F12)" : "CREDIT (F12)", Color.FromArgb(23, 162, 184), 180);
            btnCredit.Click += BtnCredit_Click;

            btnHold = CreateActionButton(isArabic ? "تعليق" : "HOLD", Color.FromArgb(108, 117, 125), 120);
            btnHold.Click += BtnHold_Click;

            btnResume = CreateActionButton(isArabic ? "استعادة" : "RESUME", Color.FromArgb(108, 117, 125), 120);
            btnResume.Click += BtnResume_Click;

            btnReturn = CreateActionButton(isArabic ? "مرتجع" : "RETURN", Color.FromArgb(108, 117, 125), 120);
            btnReturn.Click += (s, e) => {
                var returnForm = new SalesReturnForm();
                returnForm.ShowDialog();
            };

            btnClear = CreateActionButton(isArabic ? "مسح (ESC)" : "CLEAR (ESC)", Color.FromArgb(220, 53, 69), 150);
            btnClear.Click += (s, e) => {
                if (MessageBox.Show(isArabic ? "هل تريد مسح الفاتورة بالكامل؟" : "Clear whole invoice?", "Clear", MessageBoxButtons.YesNo) == DialogResult.Yes)
                {
                    ResetInvoice();
                }
            };

            Button btnRemove = CreateActionButton(isArabic ? "حذف (DEL)" : "REMOVE (DEL)", Color.FromArgb(255, 87, 34), 150);
            btnRemove.Click += (s, e) => RemoveSelectedItem();

            pnlBottom.Controls.AddRange(new Control[] { btnClear, btnRemove, btnReturn, btnHold, btnResume, btnCredit, btnDiscount, btnPay });

            // Main Content: SplitContainer
            SplitContainer mainSplit = new SplitContainer {
                Dock = DockStyle.Fill,
                SplitterDistance = 650,
                RightToLeft = isArabic ? RightToLeft.Yes : RightToLeft.No
            };

            // Selection Panel (Buttons) - Panel 1
            Panel selectionPanel = new Panel { Dock = DockStyle.Fill };
            pnlCategories = new FlowLayoutPanel { Dock = DockStyle.Top, Height = 80, BackColor = Color.FromArgb(224, 224, 224), AutoScroll = true, Padding = new Padding(5) };
            pnlItems = new FlowLayoutPanel { Dock = DockStyle.Fill, AutoScroll = true, BackColor = Color.White, Padding = new Padding(10) };
            selectionPanel.Controls.Add(pnlItems);
            selectionPanel.Controls.Add(pnlCategories);

            // Invoice Panel (Grid + Summary) - Panel 2
            Panel invoicePanel = new Panel { Dock = DockStyle.Fill, BackColor = Color.White, BorderStyle = BorderStyle.FixedSingle };
            txtBarcode = new TextBox { Dock = DockStyle.Top, Font = new Font("Segoe UI", 18), PlaceholderText = isArabic ? "امسح الباركود (Enter)..." : "Scan Barcode (Enter)...", Height = 50 };
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
                RowTemplate = { Height = 40 },
                Font = new Font("Segoe UI", 11),
                ReadOnly = true
            };
            dgvInvoice.KeyDown += (s, e) => {
                if (e.KeyCode == Keys.Delete) RemoveSelectedItem();
                if (e.KeyCode == Keys.Add || e.KeyCode == Keys.Oemplus) AdjustSelectedQty(1);
                if (e.KeyCode == Keys.Subtract || e.KeyCode == Keys.OemMinus) AdjustSelectedQty(-1);
            };
            dgvInvoice.Columns.Add(new DataGridViewTextBoxColumn { Name = "ItemID", Visible = false });
            dgvInvoice.Columns.Add(new DataGridViewTextBoxColumn { Name = "Item", HeaderText = isArabic ? "الصنف" : "Item", Width = 240 });
            dgvInvoice.Columns.Add(new DataGridViewTextBoxColumn { Name = "Qty", HeaderText = isArabic ? "الكمية" : "Qty", Width = 70 });
            dgvInvoice.Columns.Add(new DataGridViewTextBoxColumn { Name = "UnitPrice", HeaderText = isArabic ? "السعر" : "Price", Width = 90 });
            dgvInvoice.Columns.Add(new DataGridViewTextBoxColumn { Name = "Total", HeaderText = isArabic ? "الإجمالي" : "Total", Width = 110 });

            Panel pnlSummary = new Panel { Dock = DockStyle.Bottom, Height = 180, BackColor = Color.FromArgb(248, 249, 250), BorderStyle = BorderStyle.FixedSingle };

            lblSubtotal = CreateSummaryLabel(isArabic ? "المجموع الفرعي:" : "Subtotal:", 15);
            lblTax = CreateSummaryLabel(isArabic ? "الضريبة (15%):" : "Tax (15%):", 45);
            lblDiscount = CreateSummaryLabel(isArabic ? "الخصم:" : "Discount:", 75);
            lblItemsCount = CreateSummaryLabel(isArabic ? "عدد الأصناف:" : "Items Count:", 105);

            lblTotal = new Label {
                Text = isArabic ? "الإجمالي: 0.00 ريال" : "TOTAL: 0.00 YER",
                ForeColor = Color.Yellow,
                BackColor = Color.FromArgb(0, 128, 0),
                Font = new Font("Segoe UI", 28, FontStyle.Bold),
                Location = new Point(20, 120),
                Padding = new Padding(10),
                AutoSize = true
            };
            pnlSummary.Controls.AddRange(new Control[] { lblSubtotal, lblTax, lblDiscount, lblItemsCount, lblTotal });

            invoicePanel.Controls.Add(dgvInvoice);
            invoicePanel.Controls.Add(txtBarcode);
            invoicePanel.Controls.Add(pnlSummary);

            mainSplit.Panel1.Controls.Add(selectionPanel); // 60%
            mainSplit.Panel2.Controls.Add(invoicePanel);   // 40%

            this.Controls.Add(mainSplit);
            this.Controls.Add(pnlTop);
            this.Controls.Add(pnlBottom);

            LanguageHelper.ApplyLanguage(this);
        }

        private Button CreateActionButton(string text, Color color, int width)
        {
            Button btn = new Button {
                Text = text,
                Dock = DockStyle.Right,
                Width = width,
                BackColor = color,
                ForeColor = Color.White,
                Font = new Font("Segoe UI", 12, FontStyle.Bold),
                FlatStyle = FlatStyle.Flat,
                Margin = new Padding(5)
            };
            btn.FlatAppearance.BorderSize = 0;
            return btn;
        }

        private Label CreateSummaryLabel(string text, int y)
        {
            return new Label { Text = text, Location = new Point(20, y), Font = new Font("Segoe UI", 11), AutoSize = true, ForeColor = Color.FromArgb(73, 80, 87) };
        }

        private async void LoadMetadata()
        {
            // Load Customers
            var customers = await _contactRepo.GetAllCustomersAsync();
            cbCustomer.DataSource = customers;
            cbCustomer.DisplayMember = "CustomerName";
            cbCustomer.ValueMember = "CustomerID";

            // Load Categories
            var categories = await _itemRepo.GetAllCategoriesAsync();
            pnlCategories.Controls.Clear();

            Button btnAll = CreateCategoryButton(LanguageHelper.TranslationService.CurrentLanguage == Supermarket.BLL.Services.Language.Arabic ? "الكل" : "All", 0);
            pnlCategories.Controls.Add(btnAll);

            foreach (var cat in categories)
            {
                pnlCategories.Controls.Add(CreateCategoryButton(cat.CategoryName, cat.CategoryID));
            }

            // Load Initial Items
            await LoadItemsByCategory(0);
        }

        private int _colorIdx = 0;
        private Color[] _catColors = { Color.FromArgb(0, 122, 204), Color.FromArgb(40, 167, 69), Color.FromArgb(255, 193, 7), Color.FromArgb(23, 162, 184), Color.FromArgb(102, 16, 242) };

        private Button CreateCategoryButton(string text, int id)
        {
            Button btn = new Button {
                Text = text,
                Width = 110,
                Height = 60,
                BackColor = id == 0 ? Color.DimGray : _catColors[_colorIdx++ % _catColors.Length],
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 10, FontStyle.Bold),
                Tag = id
            };
            btn.FlatAppearance.BorderSize = 0;
            btn.Click += async (s, e) => await LoadItemsByCategory((int)btn.Tag);
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
            foreach (var item in items)
            {
                Button btn = new Button {
                    Text = $"{item.ItemName}\n{item.SalePrice:F2}",
                    Width = 125,
                    Height = 110,
                    BackColor = Color.White,
                    TextAlign = ContentAlignment.MiddleCenter,
                    FlatStyle = FlatStyle.Flat,
                    Tag = item,
                    Font = new Font("Segoe UI", 9.5f, FontStyle.Bold),
                    Cursor = Cursors.Hand
                };
                btn.FlatAppearance.BorderColor = Color.FromArgb(0, 122, 204);
                btn.FlatAppearance.BorderSize = 1;
                btn.Click += (s, e) => {
                    AddItemToInvoice((Models.Entities.Item)btn.Tag);
                    txtBarcode.Focus();
                };
                pnlItems.Controls.Add(btn);
            }
        }

        private async System.Threading.Tasks.Task SearchItems(string query)
        {
            if (string.IsNullOrWhiteSpace(query)) {
                await LoadItemsByCategory(0);
                return;
            }
            var items = await _itemRepo.GetAllItemsAsync();
            var filtered = items.Where(i => i.ItemName.Contains(query, StringComparison.OrdinalIgnoreCase) || i.Barcode.Contains(query));
            DisplayItemButtons(filtered);
        }

        private PromotionEngine _promoEngine = new PromotionEngine();

        private void AddItemToInvoice(Models.Entities.Item item, decimal qty = 1)
        {
            bool found = false;
            foreach (DataGridViewRow row in dgvInvoice.Rows) {
                if (row.Cells["ItemID"].Value != null && (int)row.Cells["ItemID"].Value == item.ItemID) {
                    decimal newQty = Convert.ToDecimal(row.Cells["Qty"].Value) + qty;
                    row.Cells["Qty"].Value = newQty;

                    // Apply automatic promotions if any
                    decimal promoDiscount = _promoEngine.CalculateDiscount(item.ItemID, newQty, item.SalePrice);
                    row.Cells["Total"].Value = (newQty * (decimal)row.Cells["UnitPrice"].Value) - promoDiscount;

                    found = true;
                    break;
                }
            }

            if (!found) {
                decimal promoDiscount = _promoEngine.CalculateDiscount(item.ItemID, qty, item.SalePrice);
                dgvInvoice.Rows.Add(item.ItemID, item.ItemName, qty, item.SalePrice, (qty * item.SalePrice) - promoDiscount);
            }

            UpdateTotal();
        }

        private void RemoveSelectedItem()
        {
            if (dgvInvoice.SelectedRows.Count > 0) {
                dgvInvoice.Rows.Remove(dgvInvoice.SelectedRows[0]);
                UpdateTotal();
            }
        }

        private void AdjustSelectedQty(decimal delta)
        {
            if (dgvInvoice.SelectedRows.Count > 0) {
                var row = dgvInvoice.SelectedRows[0];
                decimal currentQty = Convert.ToDecimal(row.Cells["Qty"].Value);
                decimal newQty = currentQty + delta;
                if (newQty > 0) {
                    row.Cells["Qty"].Value = newQty;
                    row.Cells["Total"].Value = newQty * (decimal)row.Cells["UnitPrice"].Value;
                    UpdateTotal();
                } else {
                    RemoveSelectedItem();
                }
            }
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
                AddItemToInvoice(item, qty);

                // Focus on Qty cell for editing as per requirement
                if (dgvInvoice.Rows.Count > 0)
                {
                    dgvInvoice.CurrentCell = dgvInvoice.Rows[dgvInvoice.Rows.Count - 1].Cells["Qty"];
                    dgvInvoice.BeginEdit(true);
                }
            } else {
                MessageBox.Show(LanguageHelper.TranslationService.CurrentLanguage == Supermarket.BLL.Services.Language.Arabic ? "الصنف غير موجود" : "Item Not Found!");
            }
        }

        private void UpdateTotal()
        {
            bool isArabic = LanguageHelper.TranslationService.CurrentLanguage == Supermarket.BLL.Services.Language.Arabic;
            decimal subtotal = 0;
            int itemsCount = 0;
            foreach (DataGridViewRow row in dgvInvoice.Rows) {
                subtotal += Convert.ToDecimal(row.Cells["Total"].Value);
                itemsCount += (int)Math.Ceiling(Convert.ToDecimal(row.Cells["Qty"].Value));
            }

            decimal tax = subtotal * 0.15m;
            decimal grandTotal = (subtotal + tax) - _totalDiscount;

            lblSubtotal.Text = (isArabic ? "المجموع الفرعي: " : "Subtotal: ") + $"{subtotal:F2}";
            lblTax.Text = (isArabic ? "الضريبة (15%): " : "Tax (15%): ") + $"{tax:F2}";
            lblDiscount.Text = (isArabic ? "الخصم: " : "Discount: ") + $"{_totalDiscount:F2}";
            lblItemsCount.Text = (isArabic ? "عدد الأصناف: " : "Items Count: ") + itemsCount;
            lblTotal.Text = isArabic ? $"الإجمالي: {grandTotal:F2} ريال" : $"TOTAL: {grandTotal:F2} YER";
        }

        private void ResetInvoice()
        {
            dgvInvoice.Rows.Clear();
            _totalDiscount = 0;
            UpdateTotal();
            txtBarcode.Focus();
        }

        private void BtnDiscount_Click(object sender, EventArgs e)
        {
            bool isArabic = LanguageHelper.TranslationService.CurrentLanguage == Supermarket.BLL.Services.Language.Arabic;
            string input = InputDialog.Show(
                isArabic ? "خصم" : "Discount",
                isArabic ? "أدخل قيمة الخصم:" : "Enter Discount Amount:",
                _totalDiscount.ToString());

            if (decimal.TryParse(input, out decimal discount))
            {
                _totalDiscount = discount;
                UpdateTotal();
            }
        }

        private void BtnCredit_Click(object sender, EventArgs e)
        {
            if (cbCustomer.SelectedIndex <= 0)
            {
                MessageBox.Show(LanguageHelper.TranslationService.CurrentLanguage == Supermarket.BLL.Services.Language.Arabic
                    ? "يرجى اختيار عميل للبيع الآجل"
                    : "Please select a customer for credit sale");
                return;
            }
            PerformPayment("Credit / آجل");
        }

        private void BtnPay_Click(object sender, EventArgs e)
        {
            PerformPayment(null);
        }

        private List<HeldInvoice> _heldInvoices = new List<HeldInvoice>();
        private void BtnHold_Click(object sender, EventArgs e)
        {
            if (dgvInvoice.Rows.Count == 0) return;
            var held = new HeldInvoice {
                Rows = dgvInvoice.Rows.Cast<DataGridViewRow>().Select(r => new HeldRow {
                    ItemID = (int)r.Cells["ItemID"].Value,
                    Name = r.Cells["Item"].Value.ToString(),
                    Qty = Convert.ToDecimal(r.Cells["Qty"].Value),
                    Price = Convert.ToDecimal(r.Cells["UnitPrice"].Value),
                    Total = Convert.ToDecimal(r.Cells["Total"].Value)
                }).ToList(),
                Discount = _totalDiscount,
                CustomerValue = cbCustomer.SelectedValue
            };
            _heldInvoices.Add(held);
            ResetInvoice();
            MessageBox.Show(LanguageHelper.TranslationService.CurrentLanguage == Supermarket.BLL.Services.Language.Arabic ? "تم تعليق الفاتورة" : "Invoice Held");
        }

        private void BtnResume_Click(object sender, EventArgs e)
        {
            if (_heldInvoices.Count == 0) return;
            var held = _heldInvoices.Last();
            dgvInvoice.Rows.Clear();
            foreach (var row in held.Rows) {
                dgvInvoice.Rows.Add(row.ItemID, row.Name, row.Qty, row.Price, row.Total);
            }
            _totalDiscount = held.Discount;
            cbCustomer.SelectedValue = held.CustomerValue;
            _heldInvoices.Remove(held);
            UpdateTotal();
        }

        private class HeldInvoice {
            public List<HeldRow> Rows;
            public decimal Discount;
            public object CustomerValue;
        }

        private class HeldRow {
            public int ItemID;
            public string Name;
            public decimal Qty;
            public decimal Price;
            public decimal Total;
        }

        private async void PerformPayment(string forcedMethod)
        {
            bool isArabic = LanguageHelper.TranslationService.CurrentLanguage == Supermarket.BLL.Services.Language.Arabic;
            if (dgvInvoice.Rows.Count == 0) return;

            decimal subtotal = 0;
            foreach (DataGridViewRow row in dgvInvoice.Rows) subtotal += Convert.ToDecimal(row.Cells["Total"].Value);

            decimal taxableAmount = subtotal - _totalDiscount;
            decimal tax = taxableAmount * 0.15m;
            decimal grandTotal = taxableAmount + tax;

            PaymentForm pay = new PaymentForm(grandTotal);
            if (forcedMethod != null) {
                pay.PaymentMethod = forcedMethod;
            }

            if (pay.ShowDialog() == DialogResult.OK) {
                var invoice = new SalesInvoice {
                    InvoiceNumber = "POS-" + DateTime.Now.Ticks,
                    StoreID = 1,
                    NetAmount = subtotal, // Original Subtotal
                    TaxAmount = tax,
                    DiscountAmount = _totalDiscount,
                    TotalAmount = grandTotal, // Amount to be paid
                    PaymentType = pay.PaymentMethod,
                    CustomerID = (cbCustomer.SelectedValue != null && (int)cbCustomer.SelectedValue > 0) ? (int)cbCustomer.SelectedValue : null,
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
                    subtotal, tax, grandTotal, invoice.QRCode);

                MessageBox.Show(isArabic ? "تم حفظ البيع والطباعة بنجاح" : "Sale Saved & Printed!");
                dgvInvoice.Rows.Clear();
                _totalDiscount = 0;
                UpdateTotal();
                txtBarcode.Focus();
            }
        }

        private void InitializeComponent() { }
    }
}
