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
            bool isArabic = LanguageHelper.TranslationService.CurrentLanguage == Supermarket.BLL.Services.Language.Arabic;
            this.Text = isArabic ? "تسوية المخزون" : "Stock Adjustment";
            this.Size = new Size(1100, 750);
            this.BackColor = UITheme.ContentBg;
            this.RightToLeft = isArabic ? RightToLeft.Yes : RightToLeft.No;

            Panel pnlHeader = new Panel { Dock = DockStyle.Top, Height = 80, BackColor = Color.White, Padding = new Padding(20), BorderStyle = BorderStyle.FixedSingle };

            Label lblTitle = new Label {
                Text = isArabic ? "تسوية كميات المخزون" : "Inventory Quantity Adjustment",
                Font = new Font("Segoe UI", 14, FontStyle.Bold),
                ForeColor = UITheme.TextPrimary,
                AutoSize = true,
                Location = new Point(20, 25)
            };

            Button btnSave = new Button {
                Text = isArabic ? "حفظ التغييرات" : "SAVE CHANGES",
                Dock = isArabic ? DockStyle.Left : DockStyle.Right,
                Width = 200,
                BackColor = Color.FromArgb(255, 193, 7),
                ForeColor = Color.Black,
                Font = new Font("Segoe UI", 10, FontStyle.Bold),
                FlatStyle = FlatStyle.Flat,
                Height = 40
            };
            btnSave.FlatAppearance.BorderSize = 0;
            btnSave.Click += async (s, e) => {
                int count = 0;
                foreach (DataGridViewRow row in dgv.Rows) {
                    if (row.Cells["New"].Value != null) {
                        decimal current = Convert.ToDecimal(row.Cells["Current"].Value);
                        decimal newValue = Convert.ToDecimal(row.Cells["New"].Value);
                        decimal diff = newValue - current;
                        if (diff != 0) {
                            await _stockRepo.SaveAdjustmentAsync((int)row.Cells["ItemID"].Value, 1, diff, "Adjustment", 1);
                            count++;
                        }
                    }
                }
                if (count > 0) {
                    MessageBox.Show(isArabic ? "تم تحديث المخزن بنجاح" : "Stock updated successfully!");
                    LoadStock();
                }
            };

            pnlHeader.Controls.Add(lblTitle);
            pnlHeader.Controls.Add(btnSave);

            dgv = new DataGridView { Dock = DockStyle.Fill, AllowUserToAddRows = false, AutoGenerateColumns = false };
            UITheme.ApplyModernStyle(dgv);

            dgv.Columns.Add(new DataGridViewTextBoxColumn { Name = "ItemID", HeaderText = "ID", Width = 80, DataPropertyName = "ItemID", ReadOnly = true });
            dgv.Columns.Add(new DataGridViewTextBoxColumn { Name = "Name", HeaderText = isArabic ? "اسم الصنف" : "Item Name", AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill, DataPropertyName = "ItemName", ReadOnly = true });
            dgv.Columns.Add(new DataGridViewTextBoxColumn { Name = "Current", HeaderText = isArabic ? "الكمية الحالية" : "Current Qty", Width = 150, DataPropertyName = "Qty", ReadOnly = true });
            dgv.Columns.Add(new DataGridViewTextBoxColumn { Name = "New", HeaderText = isArabic ? "الكمية الفعلية (الجديدة)" : "Actual Qty (New)", Width = 150 });

            dgv.DefaultCellStyle.Font = UITheme.MainFont;

            this.Controls.Add(dgv);
            this.Controls.Add(pnlHeader);

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
