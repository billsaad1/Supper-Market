using Supermarket.UI.Helpers;
using System;
using System.Drawing;
using System.Windows.Forms;

namespace Supermarket.UI.Views
{
    public partial class PromotionsForm : Form
    {
        public PromotionsForm()
        {
            InitializeComponent();
            SetupUI();
        }

        private void SetupUI()
        {
            bool isArabic = LanguageHelper.TranslationService.CurrentLanguage == Supermarket.BLL.Services.Language.Arabic;
            this.Text = isArabic ? "إدارة العروض والترويج" : "Promotions Management";
            this.Size = new Size(1100, 750);
            this.BackColor = UITheme.ContentBg;
            this.RightToLeft = isArabic ? RightToLeft.Yes : RightToLeft.No;

            Panel pnlHeader = new Panel { Dock = DockStyle.Top, Height = 80, BackColor = Color.White, Padding = new Padding(20), BorderStyle = BorderStyle.FixedSingle };
            Label lblHeader = new Label { Text = isArabic ? "إعداد العروض الترويجية" : "Setup Promotions", Font = new Font("Segoe UI", 14, FontStyle.Bold), AutoSize = true, Location = new Point(20, 25) };
            pnlHeader.Controls.Add(lblHeader);

            Panel pnlMain = new Panel { Dock = DockStyle.Fill, Padding = new Padding(15) };

            Panel left = new Panel { Dock = isArabic ? DockStyle.Right : DockStyle.Left, Width = 380, BackColor = Color.White, Padding = new Padding(25) };
            left.Paint += (s, e) => { e.Graphics.DrawRectangle(Pens.LightGray, 0, 0, left.Width-1, left.Height-1); };

            Label lblName = new Label { Text = isArabic ? "اسم العرض:" : "Promotion Name:", Location = new Point(25, 20), AutoSize = true, Font = UITheme.MainFont };
            TextBox txtName = new TextBox { Location = new Point(25, 45), Width = 330, Font = new Font("Segoe UI", 11) };

            Label lblType = new Label { Text = isArabic ? "نوع العرض:" : "Discount Type:", Location = new Point(25, 95), AutoSize = true, Font = UITheme.MainFont };
            ComboBox cbType = new ComboBox { Location = new Point(25, 120), Width = 330, Font = new Font("Segoe UI", 11), DropDownStyle = ComboBoxStyle.DropDownList };
            cbType.Items.AddRange(isArabic ? new string[] { "نسبة مئوية (%)", "مبلغ ثابت", "اشتر 1 واحصل على 1 مجاناً" } : new string[] { "Percentage (%)", "Fixed Amount", "Buy 1 Get 1 Free" });
            cbType.SelectedIndex = 0;

            Label lblVal = new Label { Text = isArabic ? "قيمة الخصم:" : "Discount Value:", Location = new Point(25, 170), AutoSize = true, Font = UITheme.MainFont };
            NumericUpDown numVal = new NumericUpDown { Location = new Point(25, 195), Width = 150, Font = new Font("Segoe UI", 11), DecimalPlaces = 2 };

            Button btnSave = new Button {
                Text = isArabic ? "حفظ العرض الترويجي" : "SAVE PROMOTION",
                Location = new Point(25, 260),
                Width = 330, Height = 45,
                BackColor = Color.FromArgb(102, 16, 242),
                ForeColor = Color.White,
                Font = new Font("Segoe UI", 10, FontStyle.Bold),
                FlatStyle = FlatStyle.Flat
            };
            btnSave.FlatAppearance.BorderSize = 0;

            left.Controls.AddRange(new Control[] { lblName, txtName, lblType, cbType, lblVal, numVal, btnSave });

            DataGridView dgv = new DataGridView { Dock = DockStyle.Fill, BackgroundColor = Color.White };
            UITheme.ApplyModernStyle(dgv);
            dgv.Columns.Add("Name", isArabic ? "العرض" : "Promotion");
            dgv.Columns.Add("Type", isArabic ? "النوع" : "Type");
            dgv.Columns.Add("Val", isArabic ? "القيمة" : "Value");
            dgv.Columns["Name"].AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill;

            btnSave.Click += (s, e) => {
                if (!string.IsNullOrEmpty(txtName.Text)) {
                    dgv.Rows.Add(txtName.Text, cbType.Text, numVal.Value);
                    txtName.Clear(); numVal.Value = 0;
                    MessageBox.Show(isArabic ? "تم حفظ العرض بنجاح" : "Promotion saved successfully!");
                }
            };

            pnlMain.Controls.Add(dgv);
            pnlMain.Controls.Add(left);

            this.Controls.Add(pnlMain);
            this.Controls.Add(pnlHeader);

            LanguageHelper.ApplyLanguage(this);
        }

        private void InitializeComponent() { }
    }
}
