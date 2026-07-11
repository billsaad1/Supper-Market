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
    public partial class AdjustmentForm : Form
    {
        private DataGridView dgvItems;
        private ComboBox cbStore;
        private MasterDataRepository _itemRepo;
        private StockOperationRepository _stockRepo;

        public AdjustmentForm()
        {
            InitializeComponent();
            string conn = AppSettings.ConnectionString;
            _itemRepo = new MasterDataRepository(conn);
            _stockRepo = new StockOperationRepository(conn);
            SetupUI();
            LoadMetadata();
        }

        private void SetupUI()
        {
            bool isArabic = LanguageHelper.TranslationService.CurrentLanguage == Language.Arabic;
            this.Text = isArabic ? "جرد وتسوية المخزون" : "Stock Take & Adjustment";
            this.Size = new Size(1000, 750);
            this.BackColor = UITheme.ContentBg;
            this.RightToLeft = isArabic ? RightToLeft.Yes : RightToLeft.No;

            Panel top = new Panel { Dock = DockStyle.Top, Height = 80, BackColor = Color.White, Padding = new Padding(20) };
            cbStore = new ComboBox { Width = 250, DropDownStyle = ComboBoxStyle.DropDownList };
            cbStore.SelectedIndexChanged += async (s, e) => await LoadStockForStore();
            top.Controls.Add(cbStore);

            dgvItems = new DataGridView { Dock = DockStyle.Fill, AllowUserToAddRows = false };
            UITheme.ApplyModernStyle(dgvItems);
            dgvItems.Columns.Add(new DataGridViewTextBoxColumn { Name = "ItemID", Visible = false });
            dgvItems.Columns.Add(new DataGridViewTextBoxColumn { Name = "Name", HeaderText = isArabic ? "الصنف" : "Item", AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill, ReadOnly = true });
            dgvItems.Columns.Add(new DataGridViewTextBoxColumn { Name = "SystemQty", HeaderText = isArabic ? "الكمية الدفترية" : "System Qty", Width = 150, ReadOnly = true });
            dgvItems.Columns.Add(new DataGridViewTextBoxColumn { Name = "PhysicalQty", HeaderText = isArabic ? "الكمية الفعلية" : "Physical Qty", Width = 150 });
            dgvItems.Columns.Add(new DataGridViewTextBoxColumn { Name = "Variance", HeaderText = isArabic ? "الفارق" : "Variance", Width = 150, ReadOnly = true });

            dgvItems.CellValueChanged += (s, e) => {
                if (e.ColumnIndex == dgvItems.Columns["PhysicalQty"].Index && e.RowIndex >= 0) {
                    var row = dgvItems.Rows[e.RowIndex];
                    decimal sys = Convert.ToDecimal(row.Cells["SystemQty"].Value);
                    if (decimal.TryParse(row.Cells["PhysicalQty"].Value?.ToString(), out decimal phys)) {
                        decimal var = phys - sys;
                        row.Cells["Variance"].Value = var;
                        row.DefaultCellStyle.BackColor = var < 0 ? Color.MistyRose : (var > 0 ? Color.Honeydew : Color.White);
                    }
                }
            };

            Panel pnlFooter = new Panel { Dock = DockStyle.Bottom, Height = 80, BackColor = Color.White, Padding = new Padding(15) };
            Button btnSave = new Button {
                Text = isArabic ? "حفظ تسوية الجرد" : "POST ADJUSTMENTS",
                Dock = DockStyle.Fill,
                BackColor = UITheme.PrimaryColor, ForeColor = Color.White,
                Font = new Font("Segoe UI", 12, FontStyle.Bold),
                FlatStyle = FlatStyle.Flat
            };
            btnSave.Click += async (s, e) => {
                var adjustments = dgvItems.Rows.Cast<DataGridViewRow>()
                    .Select(r => new {
                        ItemID = (int)r.Cells["ItemID"].Value,
                        Variance = Convert.ToDecimal(r.Cells["Variance"].Value ?? 0)
                    })
                    .Where(a => a.Variance != 0).ToList<dynamic>();

                if (adjustments.Any()) {
                    await _stockRepo.ProcessAdjustmentsAsync((int)cbStore.SelectedValue, adjustments, 1);
                    MessageBox.Show(isArabic ? "تم حفظ التسوية وتحديث المخزون والقيود" : "Adjustments posted successfully!");
                    this.Close();
                }
            };
            pnlFooter.Controls.Add(btnSave);

            this.Controls.Add(dgvItems);
            this.Controls.Add(top);
            this.Controls.Add(pnlFooter);
        }

        private async void LoadMetadata()
        {
            cbStore.DataSource = await _itemRepo.GetAllStoresAsync();
            cbStore.DisplayMember = "StoreName"; cbStore.ValueMember = "StoreID";
        }

        private async System.Threading.Tasks.Task LoadStockForStore()
        {
            if (cbStore.SelectedValue is int sid) {
                var stock = await _stockRepo.GetStockByStoreAsync(sid);
                dgvItems.Rows.Clear();
                foreach (var s in stock) dgvItems.Rows.Add(s.ItemID, s.ItemName, s.Quantity, s.Quantity, 0);
            }
        }

        private void InitializeComponent() { }
    }
}
