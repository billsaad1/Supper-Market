using Supermarket.UI.Helpers;
using System;
using System.Drawing;
using System.Windows.Forms;
using System.Collections.Generic;
using Supermarket.DAL;
using Supermarket.BLL.Services;
using System.Linq;

namespace Supermarket.UI.Views
{
    public partial class WasteForm : Form
    {
        private DataGridView dgvItems;
        private TextBox txtBarcode;
        private StockOperationRepository _stockRepo;
        private MasterDataRepository _itemRepo;

        public WasteForm()
        {
            InitializeComponent();
            string conn = AppSettings.ConnectionString;
            _stockRepo = new StockOperationRepository(conn);
            _itemRepo = new MasterDataRepository(conn);
            SetupUI();
        }

        private void SetupUI()
        {
            bool isArabic = LanguageHelper.TranslationService.CurrentLanguage == Language.Arabic;
            this.Text = isArabic ? "تسجيل التوالف والهالك" : "Waste & Damaged Goods";
            this.Size = new Size(900, 650);
            this.BackColor = UITheme.ContentBg;
            this.RightToLeft = isArabic ? RightToLeft.Yes : RightToLeft.No;

            Panel top = new Panel { Dock = DockStyle.Top, Height = 80, BackColor = Color.White, Padding = new Padding(20) };
            txtBarcode = new TextBox { Width = 300, Font = new Font("Segoe UI", 12), PlaceholderText = isArabic ? "باركود الصنف التالف..." : "Barcode for Waste Item..." };
            txtBarcode.KeyDown += async (s, e) => {
                if (e.KeyCode == Keys.Enter && !string.IsNullOrEmpty(txtBarcode.Text)) {
                    var item = await _itemRepo.GetItemByBarcodeAsync(txtBarcode.Text);
                    if (item != null) {
                        dgvItems.Rows.Add(item.ItemID, item.ItemName, 1, item.CostPrice, item.CostPrice);
                        txtBarcode.Clear();
                    }
                }
            };
            top.Controls.Add(txtBarcode);

            dgvItems = new DataGridView { Dock = DockStyle.Fill, AllowUserToAddRows = false };
            UITheme.ApplyModernStyle(dgvItems);
            dgvItems.Columns.Add(new DataGridViewTextBoxColumn { Name = "ItemID", Visible = false });
            dgvItems.Columns.Add(new DataGridViewTextBoxColumn { Name = "Name", HeaderText = isArabic ? "الصنف" : "Item", AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill });
            dgvItems.Columns.Add(new DataGridViewTextBoxColumn { Name = "Qty", HeaderText = isArabic ? "الكمية" : "Qty", Width = 120 });
            dgvItems.Columns.Add(new DataGridViewTextBoxColumn { Name = "Cost", HeaderText = isArabic ? "التكلفة" : "Cost", Width = 120 });
            dgvItems.Columns.Add(new DataGridViewTextBoxColumn { Name = "Total", HeaderText = isArabic ? "الإجمالي" : "Total", Width = 150 });

            Panel pnlFooter = new Panel { Dock = DockStyle.Bottom, Height = 80, BackColor = Color.White, Padding = new Padding(15) };
            Button btnSave = new Button {
                Text = isArabic ? "ترحيل التوالف" : "POST WASTE",
                Dock = DockStyle.Fill,
                BackColor = Color.FromArgb(255, 152, 0), ForeColor = Color.White,
                Font = new Font("Segoe UI", 12, FontStyle.Bold),
                FlatStyle = FlatStyle.Flat
            };
            btnSave.Click += async (s, e) => {
                var items = dgvItems.Rows.Cast<DataGridViewRow>()
                    .Select(r => new {
                        ItemID = (int)r.Cells["ItemID"].Value,
                        Variance = -Convert.ToDecimal(r.Cells["Qty"].Value)
                    }).ToList<dynamic>();

                if (items.Any()) {
                    await _stockRepo.ProcessAdjustmentsAsync(1, items, 1); // Store 1, User 1
                    MessageBox.Show(isArabic ? "تم تسجيل التوالف وخصمها من المخزن" : "Waste recorded and inventory adjusted!");
                    this.Close();
                }
            };
            pnlFooter.Controls.Add(btnSave);

            this.Controls.Add(dgvItems);
            this.Controls.Add(top);
            this.Controls.Add(pnlFooter);
            LanguageHelper.ApplyLanguage(this);
        }

        private void InitializeComponent() { }
    }
}
