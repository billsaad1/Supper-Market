using Supermarket.UI.Helpers;
using System;
using System.Drawing;
using System.Windows.Forms;
using Supermarket.BLL.Services;

namespace Supermarket.UI.Views
{
    public partial class MainForm : Form
    {
        private Panel sidePanel;
        private Panel headerPanel;
        private Label lblTitle;
        private string _userRole;

        public MainForm(string userRole = "Admin")
        {
            _userRole = userRole;
            InitializeComponent();
            SetupUI();
        }

        private void SetupUI()
        {
            this.Text = "Supermarket System / نظام السوبر ماركت";
            this.Size = new Size(1200, 800);
            this.StartPosition = FormStartPosition.CenterScreen;
            this.IsMdiContainer = true;
            this.Load += (s, e) => OpenForm(new DashboardView());

            sidePanel = new Panel { Dock = DockStyle.Left, Width = 220, BackColor = Color.FromArgb(30, 30, 30), AutoScroll = true };

            headerPanel = new Panel { Dock = DockStyle.Top, Height = 60, BackColor = Color.FromArgb(45, 45, 48) };
            lblTitle = new Label { Text = "DASHBOARD", ForeColor = Color.White, Font = new Font("Segoe UI", 16, FontStyle.Bold), Location = new Point(20, 15), AutoSize = true };

            Button btnLang = new Button
            {
                Text = LanguageHelper.TranslationService.CurrentLanguage == Language.Arabic ? "English" : "العربية",
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Width = 80,
                Height = 30,
                Top = 15,
                Left = 1100, // Approximate, will be anchored
                Anchor = AnchorStyles.Top | AnchorStyles.Right
            };
            btnLang.Click += (s, e) => {
                LanguageHelper.TranslationService.CurrentLanguage =
                    LanguageHelper.TranslationService.CurrentLanguage == Language.Arabic ? Language.English : Language.Arabic;
                Application.Restart();
            };

            headerPanel.Controls.Add(btnLang);
            headerPanel.Controls.Add(lblTitle);

            int y = 0;
            if (PermissionsManager.CanAccess(_userRole, "POS")) {
                AddSectionLabel("--- OPERATIONS ---", ref y);
                AddMenuButton("POS / نقطة البيع", ref y, (s, e) => OpenForm(new PosForm()));
                AddMenuButton("Returns / المرتجعات", ref y, (s, e) => OpenForm(new SalesReturnForm()));
                AddMenuButton("Shifts / الورديات", ref y, (s, e) => OpenForm(new ShiftClosingForm()));
            }

            if (PermissionsManager.CanAccess(_userRole, "Purchases")) {
                AddMenuButton("Purchases / المشتريات", ref y, (s, e) => OpenForm(new PurchaseForm()));
            }

            if (PermissionsManager.CanAccess(_userRole, "Items")) {
                AddSectionLabel("--- INVENTORY ---", ref y);
                AddMenuButton("Items / الأصناف", ref y, (s, e) => OpenForm(new ItemsForm()));
                AddMenuButton("Categories / المجموعات", ref y, (s, e) => OpenForm(new CategoriesForm()));
                AddMenuButton("Adjustments / تسويات", ref y, (s, e) => OpenForm(new AdjustmentForm()));
                AddMenuButton("Waste / التوالف", ref y, (s, e) => OpenForm(new WasteForm()));
                AddMenuButton("Promotions / العروض", ref y, (s, e) => OpenForm(new PromotionsForm()));
            }

            if (PermissionsManager.CanAccess(_userRole, "Accounting")) {
                AddSectionLabel("--- ACCOUNTING ---", ref y);
                AddMenuButton("Accounts / الحسابات", ref y, (s, e) => OpenForm(new AccountsForm()));
                AddMenuButton("Vouchers / السندات", ref y, (s, e) => OpenForm(new VouchersForm()));
                AddMenuButton("Contacts / الجهات", ref y, (s, e) => OpenForm(new ContactsForm()));
            }

            if (PermissionsManager.CanAccess(_userRole, "HR")) {
                AddSectionLabel("--- HUMAN RESOURCES ---", ref y);
                AddMenuButton("HR / الموظفين", ref y, (s, e) => OpenForm(new HRForm()));
            }

            if (PermissionsManager.CanAccess(_userRole, "Reports")) {
                AddSectionLabel("--- REPORTS ---", ref y);
                AddMenuButton("Reports / التقارير", ref y, (s, e) => OpenForm(new DetailedReportsForm()));
            }

            if (PermissionsManager.CanAccess(_userRole, "Settings")) {
                AddSectionLabel("--- SYSTEM ---", ref y);
                AddMenuButton("Company / الشركة", ref y, (s, e) => OpenForm(new CompanySettingsForm()));
                AddMenuButton("Users / المستخدمين", ref y, (s, e) => OpenForm(new UsersForm()));
                AddMenuButton("Stores / المخازن", ref y, (s, e) => OpenForm(new StoresForm()));
                AddMenuButton("Permissions / الصلاحيات", ref y, (s, e) => OpenForm(new PermissionsForm()));
                AddMenuButton("Database / قاعدة البيانات", ref y, (s, e) => {
                    using (var dbSet = new DatabaseSettingsForm()) dbSet.ShowDialog();
                });
            }

            this.Controls.Add(headerPanel);
            this.Controls.Add(sidePanel);

            LanguageHelper.ApplyLanguage(this);
        }

        private void AddSectionLabel(string text, ref int y)
        {
            Label lbl = new Label { Text = text, ForeColor = Color.Gray, Font = new Font("Arial", 9, FontStyle.Bold), Top = y + 10, Left = 10, Width = 200, AutoSize = false };
            sidePanel.Controls.Add(lbl);
            y += 30;
        }

        private void AddMenuButton(string text, ref int y, EventHandler onClick)
        {
            Button btn = new Button
            {
                Text = text,
                Top = y,
                Width = 220,
                Height = 45,
                FlatStyle = FlatStyle.Flat,
                ForeColor = Color.Gainsboro,
                TextAlign = ContentAlignment.MiddleLeft,
                Padding = new Padding(15, 0, 0, 0),
                Font = new Font("Segoe UI", 10)
            };
            btn.FlatAppearance.BorderSize = 0;
            btn.Click += (s, e) => { lblTitle.Text = text.ToUpper(); onClick(s, e); };
            sidePanel.Controls.Add(btn);
            y += 45;
        }

        private void OpenForm(Form frm)
        {
            foreach (Form openForm in this.MdiChildren) { openForm.Close(); }
            frm.MdiParent = this;
            frm.FormBorderStyle = FormBorderStyle.None;
            frm.Dock = DockStyle.Fill;
            frm.Show();
        }

        private void InitializeComponent() { this.SuspendLayout(); this.Name = "MainForm"; this.ResumeLayout(false); }
    }
}
