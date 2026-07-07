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

            btnClear = CreateStyledButton(isArabic ? "جديد (Esc)" : "NEW (Esc)", Color.FromArgb(220, 53, 69), 110);
            btnClear.Click += (s, e) => ResetInvoice();

            // Order matching image: Save, Discount, Credit, Hold, Resume, New (from right to left in RTL)
            pnlTopActions.Controls.AddRange(new Control[] { btnClear, btnResume, btnHold, btnCredit, btnDiscount, btnPay });

            // --- Secondary Header (Search & Customer) ---
            Panel pnlHeader = new Panel { Dock = DockStyle.Top, Height = 100, BackColor = Color.White, Padding = new Padding(15) };

            Label lblCust = new Label { Text = isArabic ? "العميل:" : "Customer:", Location = new Point(15, 15), AutoSize = true, Font = UITheme.MainFont };
            cbCustomer = new ComboBox { Width = 250, Font = new Font("Segoe UI", 11), DropDownStyle = ComboBoxStyle.DropDownList, Location = new Point(15, 40) };

            Label lblBarcode = new Label { Text = isArabic ? "باركود الصنف:" : "Item Barcode:", Location = new Point(280, 15), AutoSize = true, Font = UITheme.MainFont };
            txtBarcode = new TextBox { Width = 300, Font = new Font("Segoe UI", 14, FontStyle.Bold), Location = new Point(280, 40) };
            txtBarcode.KeyDown += async (s, e) => {
                if (e.KeyCode == Keys.Enter && !string.IsNullOrEmpty(txtBarcode.Text)) {
                    await AddItemByBarcode(txtBarcode.Text);
                    txtBarcode.Clear();
                }
            };

            txtSearch = new TextBox { Width = 300, Font = new Font("Segoe UI", 11), PlaceholderText = isArabic ? "بحث سريع بالاسم..." : "Quick Search by Name...", Location = new Point(600, 40) };
            txtSearch.TextChanged += async (s, e) => await SearchItems(txtSearch.Text);

            pnlHeader.Controls.AddRange(new Control[] { lblCust, cbCustomer, lblBarcode, txtBarcode, txtSearch });

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
            dgvInvoice.Columns.Add(new DataGridViewTextBoxColumn { Name = "Item", HeaderText = isArabic ? "اسم الصنف" : "Item Name", AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill });
            dgvInvoice.Columns.Add(new DataGridViewTextBoxColumn { Name = "Qty", HeaderText = isArabic ? "الكمية" : "Qty", Width = 80 });
            dgvInvoice.Columns.Add(new DataGridViewTextBoxColumn { Name = "UnitPrice", HeaderText = isArabic ? "السعر" : "Price", Width = 100 });
            dgvInvoice.Columns.Add(new DataGridViewTextBoxColumn { Name = "Total", HeaderText = isArabic ? "الإجمالي" : "Total", Width = 120 });

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
            dgvInvoice.CellContentClick += (s, e) => { if(e.ColumnIndex == dgvInvoice.Columns["Delete"].Index) RemoveSelectedItem(); };

            Panel pnlSummary = new Panel { Dock = DockStyle.Bottom, Height = 160, BackColor = Color.White, Padding = new Padding(20) };
            pnlSummary.Paint += (s, e) => { e.Graphics.DrawLine(Pens.LightGray, 0, 0, pnlSummary.Width, 0); };

            lblSubtotal = new Label { Text = "0.00", Font = new Font("Segoe UI", 12, FontStyle.Bold), Location = new Point(20, 20), AutoSize = true };
            lblTax = new Label { Text = "0.00", Font = new Font("Segoe UI", 10), Location = new Point(20, 55), AutoSize = true, ForeColor = Color.Gray };
            lblDiscount = new Label { Text = "0.00", Font = new Font("Segoe UI", 10), Location = new Point(20, 90), AutoSize = true, ForeColor = Color.FromArgb(220, 53, 69) };

            lblTotal = new Label {
                Text = "0.00",
                Font = new Font("Segoe UI", 48, FontStyle.Bold),
                ForeColor = Color.FromArgb(40, 167, 69),
                Dock = DockStyle.Right,
                TextAlign = ContentAlignment.MiddleRight,
                Width = 400
            };

            pnlSummary.Controls.AddRange(new Control[] { lblSubtotal, lblTax, lblDiscount, lblTotal });
            pnlInvoice.Controls.Add(dgvInvoice);
            pnlInvoice.Controls.Add(pnlSummary);

            // PANEL 2 (Left in RTL): Categories & Items
            Panel pnlCatalog = new Panel { Dock = DockStyle.Fill, Padding = new Padding(5) };
            pnlCategories = new FlowLayoutPanel { Dock = DockStyle.Top, Height = 200, AutoScroll = true, BackColor = Color.FromArgb(240, 240, 240) };
            pnlItems = new FlowLayoutPanel { Dock = DockStyle.Fill, AutoScroll = true, BackColor = Color.White };

            pnlCatalog.Controls.Add(pnlItems);
            pnlCatalog.Controls.Add(pnlCategories);

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
            // In image, 'All' is Gray, 'General' is Blue
            Color bg = id == 0 ? Color.FromArgb(108, 117, 125) : Color.FromArgb(0, 123, 255);

            Button btn = new Button {
                Text = text,
                Width = 90,
                Height = 80,
                BackColor = bg,
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 10, FontStyle.Bold),
                Tag = id,
                Margin = new Padding(2)
            };
            btn.FlatAppearance.BorderSize = 0;
            btn.Click += async (s, e) => {
                await LoadItemsByCategory((int)btn.Tag);
                // Visual feedback for selection
                foreach(Control c in pnlCategories.Controls) if(c is Button b) b.FlatAppearance.BorderSize = 0;
                btn.FlatAppearance.BorderSize = 2;
                btn.FlatAppearance.BorderColor = Color.White;
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
            foreach (var item in items)
            {
                Button btn = new Button {
                    Text = $"{item.ItemName}\n{item.SalePrice:F2}",
                    Width = 120,
                    Height = 100,
                    BackColor = Color.White,
                    Tag = item,
                    Font = new Font("Segoe UI", 9, FontStyle.Bold),
                    Cursor = Cursors.Hand,
                    Margin = new Padding(3)
                };
                btn.FlatAppearance.BorderColor = UITheme.PrimaryColor;
                btn.Click += (s, e) => { AddItemToInvoice((Models.Entities.Item)btn.Tag); txtBarcode.Focus(); };
                pnlItems.Controls.Add(btn);
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
            foreach (DataGridViewRow row in dgvInvoice.Rows) {
                if ((int)row.Cells["ItemID"].Value == item.ItemID) {
                    decimal newQty = Convert.ToDecimal(row.Cells["Qty"].Value) + qty;
                    row.Cells["Qty"].Value = newQty;
                    row.Cells["Total"].Value = newQty * (decimal)row.Cells["UnitPrice"].Value;
                    UpdateTotal();
                    return;
                }
            }
            dgvInvoice.Rows.Add(item.ItemID, item.ItemName, qty, item.SalePrice, qty * item.SalePrice);
            UpdateTotal();
        }

        private void UpdateTotal()
        {
            bool isArabic = LanguageHelper.TranslationService.CurrentLanguage == Supermarket.BLL.Services.Language.Arabic;
            decimal subtotal = dgvInvoice.Rows.Cast<DataGridViewRow>().Sum(r => Convert.ToDecimal(r.Cells["Total"].Value));
            decimal tax = subtotal * 0.15m;
            decimal grandTotal = (subtotal + tax) - _totalDiscount;

            lblSubtotal.Text = (isArabic ? "المجموع: " : "Subtotal: ") + subtotal.ToString("N2");
            lblTax.Text = (isArabic ? "الضريبة: " : "Tax: ") + tax.ToString("N2");
            lblDiscount.Text = (isArabic ? "خصم: " : "Discount: ") + _totalDiscount.ToString("N2");
            lblTotal.Text = grandTotal.ToString("N2") + (isArabic ? " ريال" : " YER");
        }

        private void RemoveSelectedItem() { if (dgvInvoice.CurrentRow != null) { dgvInvoice.Rows.Remove(dgvInvoice.CurrentRow); UpdateTotal(); } }
        private void ResetInvoice() { dgvInvoice.Rows.Clear(); _totalDiscount = 0; UpdateTotal(); txtBarcode.Focus(); }

        private async System.Threading.Tasks.Task AddItemByBarcode(string barcode)
        {
            var item = await _itemRepo.GetItemByBarcodeAsync(barcode);
            if (item != null) AddItemToInvoice(item);
            else MessageBox.Show(LanguageHelper.TranslationService.CurrentLanguage == Supermarket.BLL.Services.Language.Arabic ? "غير موجود" : "Not Found");
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
            foreach (var r in held.Rows) dgvInvoice.Rows.Add(r.ItemID, r.Name, r.Qty, r.Price, r.Total);
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

            var invoice = new SalesInvoice {
                InvoiceNumber = "POS-" + DateTime.Now.Ticks,
                StoreID = 1, NetAmount = subtotal, TaxAmount = tax, DiscountAmount = _totalDiscount,
                TotalAmount = grandTotal, PaymentType = method ?? "Cash",
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
            MessageBox.Show(LanguageHelper.TranslationService.CurrentLanguage == Language.Arabic ? "تم حفظ العملية بنجاح" : "Sale saved successfully!");
            ResetInvoice();
        }

        private void InitializeComponent() { }
    }
}
