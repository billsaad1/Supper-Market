using Supermarket.UI.Helpers;
using System;
using System.Drawing;
using System.Windows.Forms;
using System.Collections.Generic;
using Supermarket.DAL;
using Dapper;

namespace Supermarket.UI.Views
{
    public partial class AdjustmentForm : Form
    {
        private StockOperationRepository _stockRepo;
        private DataGridView dgv;

        public AdjustmentForm()
        {
            InitializeComponent();
            _stockRepo = new StockOperationRepository(AppSettings.ConnectionString);
            SetupUI();
            LoadStock();
        }

        private void SetupUI()
        {
            this.Text = "Stock Adjustment / تسوية مخزون";
            this.Size = new Size(900, 650);
            this.BackColor = UITheme.ContentBg;
            bool isArabic = LanguageHelper.TranslationService.CurrentLanguage == Supermarket.BLL.Services.Language.Arabic;

            Panel pnlToolbar = new Panel { Dock = DockStyle.Top, Height = 60, BackColor = Color.White, Padding = new Padding(10) };
            Button btnSave = new Button {
                Text = isArabic ? "حفظ التسوية" : "SAVE ADJUSTMENT",
                Dock = isArabic ? DockStyle.Left : DockStyle.Right,
                Width = 200,
                BackColor = UITheme.WarningColor,
                ForeColor = Color.White
            };
            UITheme.ApplyModernStyle(btnSave);
            btnSave.Click += async (s, e) => {
                foreach (DataGridViewRow row in dgv.Rows) {
                    if (row.Cells["New"].Value != null) {
                        decimal diff = Convert.ToDecimal(row.Cells["New"].Value) - Convert.ToDecimal(row.Cells["Current"].Value);
                        if (diff != 0) await _stockRepo.SaveAdjustmentAsync((int)row.Cells["ItemID"].Value, 1, diff, "Adjustment", 1);
                    }
                }
                MessageBox.Show(isArabic ? "تم حفظ التسوية وتحديث المخزن" : "Adjustment Saved!");
            };
            pnlToolbar.Controls.Add(btnSave);

            dgv = new DataGridView { Dock = DockStyle.Fill, AllowUserToAddRows = false };
            UITheme.ApplyModernStyle(dgv);

            dgv.Columns.Add("ItemID", "ID");
            dgv.Columns.Add("Name", isArabic ? "الصنف" : "Item");
            dgv.Columns.Add("Current", isArabic ? "الحالي" : "Current");
            dgv.Columns.Add("New", isArabic ? "الجديد" : "New");

            dgv.Columns["ItemID"].Width = 60;
            dgv.Columns["Name"].Width = 300;
            dgv.Columns["Current"].ReadOnly = true;

            this.Controls.Add(dgv);
            this.Controls.Add(pnlToolbar);

            LanguageHelper.ApplyLanguage(this);
        }

        private async void LoadStock()
        {
            string conn = AppSettings.ConnectionString;
            using (var db = new Microsoft.Data.SqlClient.SqlConnection(conn))
            {
                var data = await db.QueryAsync<dynamic>("SELECT i.ItemID, i.ItemName, ISNULL(s.Quantity, 0) as Qty FROM Items i LEFT JOIN Stock s ON i.ItemID = s.ItemID");
                foreach (var item in data)
                {
                    dgv.Rows.Add(item.ItemID, item.ItemName, item.Qty, item.Qty);
                }
            }
        }

        private void InitializeComponent() { }
    }
}
