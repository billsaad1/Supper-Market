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
            this.Size = new Size(800, 600);

            dgv = new DataGridView { Dock = DockStyle.Fill, AllowUserToAddRows = false };
            dgv.Columns.Add("ItemID", "ID");
            dgv.Columns.Add("Name", "Item / الصنف");
            dgv.Columns.Add("Current", "Current / الحالي");
            dgv.Columns.Add("New", "New / الجديد");

            Button btnSave = new Button { Text = "SAVE ADJUSTMENT / حفظ التسوية", Dock = DockStyle.Bottom, Height = 50, BackColor = Color.Orange, Font = new Font("Arial", 12, FontStyle.Bold) };
            btnSave.Click += async (s, e) => {
                foreach (DataGridViewRow row in dgv.Rows) {
                    if (row.Cells["New"].Value != null) {
                        decimal diff = Convert.ToDecimal(row.Cells["New"].Value) - Convert.ToDecimal(row.Cells["Current"].Value);
                        if (diff != 0) await _stockRepo.SaveAdjustmentAsync((int)row.Cells["ItemID"].Value, 1, diff, "Adjustment", 1);
                    }
                }
                MessageBox.Show("Adjustment Saved! / تم حفظ التسوية وتحديث المخزن");
            };

            this.Controls.Add(dgv);
            this.Controls.Add(btnSave);

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
