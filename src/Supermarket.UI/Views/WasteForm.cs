using Supermarket.UI.Helpers;
using System;
using System.Drawing;
using System.Windows.Forms;
using Supermarket.DAL;

namespace Supermarket.UI.Views
{
    public partial class WasteForm : Form
    {
        private StockOperationRepository _stockRepo;

        public WasteForm()
        {
            InitializeComponent();
            _stockRepo = new StockOperationRepository(AppSettings.ConnectionString);
            SetupUI();
        }

        private void SetupUI()
        {
            this.Text = "Waste Management / إدارة التوالف";
            this.Size = new Size(800, 600);
            this.BackColor = UITheme.ContentBg;
            bool isArabic = LanguageHelper.TranslationService.CurrentLanguage == Supermarket.BLL.Services.Language.Arabic;

            Panel pnlEntry = new Panel { Dock = DockStyle.Top, Height = 180, BackColor = Color.White, Padding = new Padding(20) };

            Label lblBarcode = new Label { Text = isArabic ? "باركود الصنف:" : "Item Barcode:", Location = new Point(20, 20), AutoSize = true, Font = UITheme.MainFont };
            TextBox txtBarcode = new TextBox { Location = new Point(20, 45), Width = 250, Font = new Font("Segoe UI", 12) };

            Label lblQty = new Label { Text = isArabic ? "كمية التالف:" : "Waste Qty:", Location = new Point(300, 20), AutoSize = true, Font = UITheme.MainFont };
            TextBox txtQty = new TextBox { Location = new Point(300, 45), Width = 100, Font = new Font("Segoe UI", 12) };

            Label lblReason = new Label { Text = isArabic ? "السبب:" : "Reason:", Location = new Point(430, 20), AutoSize = true, Font = UITheme.MainFont };
            ComboBox cbReason = new ComboBox { Location = new Point(430, 45), Width = 200, Font = new Font("Segoe UI", 12), DropDownStyle = ComboBoxStyle.DropDownList };
            cbReason.Items.AddRange(isArabic ? new string[] { "منتهي الصلاحية", "تالف", "مفقود" } : new string[] { "Expired", "Damaged", "Lost" });
            cbReason.SelectedIndex = 0;

            Button btnSave = new Button {
                Text = isArabic ? "تسجيل تالف" : "RECORD WASTE",
                Location = new Point(20, 100),
                Width = 200,
                Height = 45,
                BackColor = UITheme.DangerColor,
                ForeColor = Color.White
            };
            UITheme.ApplyModernStyle(btnSave);
            btnSave.Click += async (s, e) => {
                if (decimal.TryParse(txtQty.Text, out decimal q)) {
                    await _stockRepo.SaveAdjustmentAsync(1, 1, -q, "Waste: " + cbReason.Text, 1);
                    MessageBox.Show(isArabic ? "تم تسجيل التالف وتحديث المخزن" : "Waste Recorded!");
                }
            };

            pnlEntry.Controls.AddRange(new Control[] { lblBarcode, txtBarcode, lblQty, txtQty, lblReason, cbReason, btnSave });

            DataGridView dgv = new DataGridView { Dock = DockStyle.Fill };
            UITheme.ApplyModernStyle(dgv);

            this.Controls.Add(dgv);
            this.Controls.Add(pnlEntry);

            LanguageHelper.ApplyLanguage(this);
        }

        private void InitializeComponent() { }
    }
}
