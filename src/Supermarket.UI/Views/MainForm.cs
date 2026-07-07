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
            this.Size = new Size(1366, 768);
            this.StartPosition = FormStartPosition.CenterScreen;
            this.IsMdiContainer = true;
            this.BackColor = UITheme.ContentBg;
            this.Load += (s, e) => {
                var pos = new PosForm();
                OpenForm(pos);
                // Highlight POS button on load
                foreach(Control c in sidePanel.Controls) {
                    if(c is Button b && (b.Text.Contains("نقطة") || b.Text.Contains("POS"))) {
                        b.BackColor = UITheme.SidebarSelected;
                        b.ForeColor = Color.White;
                        b.Font = new Font("Segoe UI", 10f, FontStyle.Bold);
                        lblTitle.Text = b.Text.Trim().ToUpper();
                    }
                }
            };

            bool isArabic = LanguageHelper.TranslationService.CurrentLanguage == Language.Arabic;
            this.RightToLeft = isArabic ? RightToLeft.Yes : RightToLeft.No;

            sidePanel = new Panel {
                Dock = isArabic ? DockStyle.Right : DockStyle.Left,
                Width = 240,
                BackColor = UITheme.SidebarBg,
                AutoScroll = true
            };

            headerPanel = new Panel {
                Dock = DockStyle.Top,
                Height = 60,
                BackColor = Color.White,
                BorderStyle = BorderStyle.FixedSingle
            };

            lblTitle = new Label {
                Text = isArabic ? "لوحة التحكم" : "DASHBOARD",
                ForeColor = UITheme.TextPrimary,
                Font = new Font("Segoe UI", 14, FontStyle.Bold),
                Location = new Point(20, 15),
                AutoSize = true
            };

            // Modern Search/Info bar in header (placeholder)
            Panel userInfoPnl = new Panel { Dock = isArabic ? DockStyle.Left : DockStyle.Right, Width = 300 };
            Label lblUser = new Label {
                Text = $"Welcome, Admin",
                ForeColor = UITheme.TextSecondary,
                Font = UITheme.MainFont,
                Dock = DockStyle.Fill,
                TextAlign = ContentAlignment.MiddleRight,
                Padding = new Padding(0, 0, 20, 0)
            };
            userInfoPnl.Controls.Add(lblUser);

            Button btnLang = new Button
            {
                Text = isArabic ? "English" : "العربية",
                ForeColor = UITheme.PrimaryColor,
                FlatStyle = FlatStyle.Flat,
                Width = 80,
                Height = 30,
                Top = 15,
                Left = isArabic ? 20 : 1100,
                Anchor = isArabic ? AnchorStyles.Top | AnchorStyles.Left : AnchorStyles.Top | AnchorStyles.Right,
                Font = new Font("Segoe UI", 9, FontStyle.Bold)
            };
            btnLang.FlatAppearance.BorderColor = UITheme.PrimaryColor;
            btnLang.Click += (s, e) => {
                LanguageHelper.TranslationService.CurrentLanguage =
                    isArabic ? Language.English : Language.Arabic;
                Application.Restart();
            };

            headerPanel.Controls.Add(btnLang);
            headerPanel.Controls.Add(lblTitle);
            headerPanel.Controls.Add(userInfoPnl);

            // Logo Section
            Panel logoPnl = new Panel { Dock = DockStyle.Top, Height = 80, BackColor = Color.FromArgb(45, 50, 55) };
            Label lblLogo = new Label {
                Text = "BCREATIVE",
                ForeColor = Color.White,
                Font = new Font("Segoe UI", 18, FontStyle.Bold),
                Dock = DockStyle.Fill,
                TextAlign = ContentAlignment.MiddleCenter
            };
            logoPnl.Controls.Add(lblLogo);
            sidePanel.Controls.Add(logoPnl);

            int y = 80;
            if (PermissionsManager.CanAccess(_userRole, "POS")) {
                AddSectionLabel(isArabic ? "--- العمليات ---" : "--- OPERATIONS ---", ref y);
                AddMenuButton(isArabic ? "نقطة البيع" : "POS / Sales", ref y, (s, e) => OpenForm(new PosForm()));
                AddMenuButton(isArabic ? "المرتجعات" : "Returns", ref y, (s, e) => OpenForm(new SalesReturnForm()));
                AddMenuButton(isArabic ? "الورديات" : "Shifts", ref y, (s, e) => OpenForm(new ShiftClosingForm()));
            }

            if (PermissionsManager.CanAccess(_userRole, "Purchases")) {
                AddMenuButton(isArabic ? "المشتريات" : "Purchases", ref y, (s, e) => OpenForm(new PurchaseListForm()));
            }

            if (PermissionsManager.CanAccess(_userRole, "Items")) {
                AddSectionLabel(isArabic ? "--- المخزون ---" : "--- INVENTORY ---", ref y);
                AddMenuButton(isArabic ? "الأصناف" : "Items", ref y, (s, e) => OpenForm(new ItemsForm()));
                AddMenuButton(isArabic ? "المجموعات" : "Categories", ref y, (s, e) => OpenForm(new CategoriesForm()));
                AddMenuButton(isArabic ? "تسويات الجرد" : "Adjustments", ref y, (s, e) => OpenForm(new AdjustmentForm()));
                AddMenuButton(isArabic ? "التوالف" : "Waste", ref y, (s, e) => OpenForm(new WasteForm()));
                AddMenuButton(isArabic ? "العروض" : "Promotions", ref y, (s, e) => OpenForm(new PromotionsForm()));
            }

            if (PermissionsManager.CanAccess(_userRole, "Accounting")) {
                AddSectionLabel(isArabic ? "--- المحاسبة ---" : "--- ACCOUNTING ---", ref y);
                AddMenuButton(isArabic ? "دليل الحسابات" : "Accounts", ref y, (s, e) => OpenForm(new AccountsForm()));
                AddMenuButton(isArabic ? "السندات المادية" : "Vouchers", ref y, (s, e) => OpenForm(new VouchersForm()));
                AddMenuButton(isArabic ? "جهات الاتصال" : "Contacts", ref y, (s, e) => OpenForm(new ContactsForm()));
            }

            if (PermissionsManager.CanAccess(_userRole, "HR")) {
                AddSectionLabel(isArabic ? "--- الموارد البشرية ---" : "--- HUMAN RESOURCES ---", ref y);
                AddMenuButton(isArabic ? "شؤون الموظفين" : "HR / Employees", ref y, (s, e) => OpenForm(new HRForm()));
            }

            if (PermissionsManager.CanAccess(_userRole, "Reports")) {
                AddSectionLabel(isArabic ? "--- التقارير ---" : "--- REPORTS ---", ref y);
                AddMenuButton(isArabic ? "التقارير المفصلة" : "Detailed Reports", ref y, (s, e) => OpenForm(new DetailedReportsForm()));
            }

            if (PermissionsManager.CanAccess(_userRole, "Settings")) {
                AddSectionLabel(isArabic ? "--- النظام ---" : "--- SYSTEM ---", ref y);
                AddMenuButton(isArabic ? "بيانات الشركة" : "Company Info", ref y, (s, e) => OpenForm(new CompanySettingsForm()));
                AddMenuButton(isArabic ? "إدارة المستخدمين" : "Users", ref y, (s, e) => OpenForm(new UsersForm()));
                AddMenuButton(isArabic ? "المخازن" : "Stores", ref y, (s, e) => OpenForm(new StoresForm()));
                AddMenuButton(isArabic ? "الصلاحيات" : "Permissions", ref y, (s, e) => OpenForm(new PermissionsForm()));
                AddMenuButton(isArabic ? "قاعدة البيانات" : "Database", ref y, (s, e) => {
                    using (var dbSet = new DatabaseSettingsForm()) dbSet.ShowDialog();
                });
            }

            this.Controls.Add(headerPanel);
            this.Controls.Add(sidePanel);

            LanguageHelper.ApplyLanguage(this);
        }

        private void AddSectionLabel(string text, ref int y)
        {
            bool isArabic = LanguageHelper.TranslationService.CurrentLanguage == Language.Arabic;
            Label lbl = new Label {
                Text = text,
                ForeColor = Color.FromArgb(108, 117, 125),
                Font = new Font("Segoe UI", 8, FontStyle.Regular),
                Top = y + 10,
                Left = 0,
                Width = 240,
                Height = 30,
                AutoSize = false,
                TextAlign = isArabic ? ContentAlignment.MiddleRight : ContentAlignment.MiddleLeft,
                Padding = isArabic ? new Padding(0, 0, 10, 0) : new Padding(10, 0, 0, 0)
            };
            sidePanel.Controls.Add(lbl);
            y += 35;
        }

        private void AddMenuButton(string text, ref int y, EventHandler onClick)
        {
            bool isArabic = LanguageHelper.TranslationService.CurrentLanguage == Language.Arabic;
            Button btn = new Button
            {
                Text = text,
                Top = y,
                Width = 240,
                Height = 40,
                FlatStyle = FlatStyle.Flat,
                ForeColor = Color.FromArgb(200, 200, 200),
                TextAlign = ContentAlignment.MiddleLeft,
                Padding = isArabic ? new Padding(0, 0, 15, 0) : new Padding(15, 0, 0, 0),
                Font = new Font("Segoe UI", 9.5f),
                Cursor = Cursors.Hand
            };
            if (isArabic) btn.TextAlign = ContentAlignment.MiddleRight;

            btn.FlatAppearance.BorderSize = 0;
            btn.FlatAppearance.MouseOverBackColor = Color.FromArgb(60, 65, 70);

            btn.Click += (s, e) => {
                lblTitle.Text = text.Trim().ToUpper();
                onClick(s, e);
                foreach(Control c in sidePanel.Controls) {
                    if(c is Button b) {
                        b.BackColor = Color.Transparent;
                        b.ForeColor = Color.FromArgb(200, 200, 200);
                        b.Font = new Font("Segoe UI", 9.5f);
                    }
                }
                btn.BackColor = UITheme.SidebarSelected;
                btn.ForeColor = Color.White;
                btn.Font = new Font("Segoe UI", 9.5f, FontStyle.Bold);
            };
            sidePanel.Controls.Add(btn);
            y += 40;
        }

        private void OpenForm(Form frm)
        {
            foreach (Form openForm in this.MdiChildren) { openForm.Close(); }
            frm.MdiParent = this;
            frm.FormBorderStyle = FormBorderStyle.None;
            frm.Dock = DockStyle.Fill;

            // Pass User info if form supports it (duck typing or interface)
            if (frm is PosForm pos) {
                // PosForm uses current user context
            }

            frm.Show();
        }

        private void InitializeComponent() { this.SuspendLayout(); this.Name = "MainForm"; this.ResumeLayout(false); }
    }
}
