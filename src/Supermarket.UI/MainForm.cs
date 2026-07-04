using System;
using System.Drawing;
using System.Windows.Forms;

namespace Supermarket.UI
{
    public partial class MainForm : Form
    {
        private Panel sidePanel;
        private Panel headerPanel;
        private Label lblTitle;

        public MainForm()
        {
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
            headerPanel.Controls.Add(lblTitle);

            int y = 0;
            AddSectionLabel("--- OPERATIONS ---", ref y);
            AddMenuButton("POS / نقطة البيع", ref y, (s, e) => OpenForm(new PosForm()));
            AddMenuButton("Purchases / المشتريات", ref y, (s, e) => OpenForm(new PurchaseForm()));
            AddMenuButton("Shifts / الورديات", ref y, (s, e) => OpenForm(new ShiftClosingForm()));

            AddSectionLabel("--- INVENTORY ---", ref y);
            AddMenuButton("Items / الأصناف", ref y, (s, e) => OpenForm(new ItemsForm()));
            AddMenuButton("Categories / المجموعات", ref y, (s, e) => OpenForm(new CategoriesForm()));
            AddMenuButton("Adjustments / تسويات", ref y, (s, e) => OpenForm(new AdjustmentForm()));

            AddSectionLabel("--- ACCOUNTING ---", ref y);
            AddMenuButton("Accounts / الحسابات", ref y, (s, e) => OpenForm(new AccountsForm()));
            AddMenuButton("Vouchers / السندات", ref y, (s, e) => OpenForm(new VouchersForm()));

            AddSectionLabel("--- MANAGEMENT ---", ref y);
            AddMenuButton("Contacts / الجهات", ref y, (s, e) => OpenForm(new ContactsForm()));
            AddMenuButton("HR / الموظفين", ref y, (s, e) => OpenForm(new HRForm()));
            AddMenuButton("Users / المستخدمين", ref y, (s, e) => OpenForm(new UsersForm()));
            AddMenuButton("Settings / الإعدادات", ref y, (s, e) => OpenForm(new CompanySettingsForm()));

            AddSectionLabel("--- REPORTS ---", ref y);
            AddMenuButton("Reports / التقارير", ref y, (s, e) => OpenForm(new ReportsForm()));

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
