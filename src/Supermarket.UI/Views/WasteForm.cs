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
            bool isArabic = LanguageHelper.TranslationService.CurrentLanguage == Supermarket.BLL.Services.Language.Arabic;
            this.Text = isArabic ? "إدارة التوالف" : "Waste Management";
            this.Size = new Size(1100, 750);
            this.BackColor = UITheme.ContentBg;
            this.RightToLeft = isArabic ? RightToLeft.Yes : RightToLeft.No;

            Panel pnlHeader = new Panel { Dock = DockStyle.Top, Height = 100, BackColor = Color.White, Padding = new Padding(25), BorderStyle = BorderStyle.FixedSingle };
            Label lblHeader = new Label { Text = isArabic ? "تسجيل تالف / منتهي الصلاحية" : "Waste / Expired Goods Registration", Font = new Font("Segoe UI", 14, FontStyle.Bold), AutoSize = true, Location = new Point(20, 30) };
            pnlHeader.Controls.Add(lblHeader);

            Panel pnlEntry = new Panel { Dock = DockStyle.Top, Height = 160, BackColor = Color.White, Padding = new Padding(25) };
            pnlEntry.Paint += (s, e) => { e.Graphics.DrawLine(Pens.LightGray, 0, pnlEntry.Height - 1, pnlEntry.Width, pnlEntry.Height - 1); };

            Label lblBarcode = new Label { Text = isArabic ? "باركود الصنف:" : "Item Barcode:", Location = new Point(25, 20), AutoSize = true, Font = UITheme.MainFont };
            TextBox txtBarcode = new TextBox { Location = new Point(25, 45), Width = 300, Font = new Font("Segoe UI", 12) };

            Label lblQty = new Label { Text = isArabic ? "الكمية:" : "Quantity:", Location = new Point(340, 20), AutoSize = true, Font = UITheme.MainFont };
            TextBox txtQty = new TextBox { Location = new Point(340, 45), Width = 100, Font = new Font("Segoe UI", 12) };

            Label lblReason = new Label { Text = isArabic ? "سبب الإتلاف:" : "Waste Reason:", Location = new Point(460, 20), AutoSize = true, Font = UITheme.MainFont };
            ComboBox cbReason = new ComboBox { Location = new Point(460, 45), Width = 200, Font = new Font("Segoe UI", 12), DropDownStyle = ComboBoxStyle.DropDownList };
            cbReason.Items.AddRange(isArabic ? new string[] { "منتهي الصلاحية", "تالف / كسر", "مفقود / عجز" } : new string[] { "Expired", "Damaged", "Lost/Missing" });
            cbReason.SelectedIndex = 0;

            Button btnSave = new Button {
                Text = isArabic ? "حفظ التالف وتحديث المخزن" : "SAVE & UPDATE STOCK",
                Location = new Point(25, 100),
                Width = 250,
                Height = 40,
                BackColor = Color.FromArgb(220, 53, 69),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 10, FontStyle.Bold)
            };
            btnSave.FlatAppearance.BorderSize = 0;
            btnSave.Click += async (s, e) => {
                if (decimal.TryParse(txtQty.Text, out decimal q)) {
                    await _stockRepo.SaveAdjustmentAsync(1, 1, -q, "Waste: " + cbReason.Text, 1);
                    MessageBox.Show(isArabic ? "تم تسجيل العملية بنجاح" : "Waste recorded successfully!");
                    txtBarcode.Clear(); txtQty.Clear(); txtBarcode.Focus();
                }
            };

            pnlEntry.Controls.AddRange(new Control[] { lblBarcode, txtBarcode, lblQty, txtQty, lblReason, cbReason, btnSave });

            DataGridView dgv = new DataGridView { Dock = DockStyle.Fill, AllowUserToAddRows = false };
            UITheme.ApplyModernStyle(dgv);

            this.Controls.Add(dgv);
            this.Controls.Add(pnlEntry);
            this.Controls.Add(pnlHeader);

            LanguageHelper.ApplyLanguage(this);
        }

        private void InitializeComponent() { }
    }
}
