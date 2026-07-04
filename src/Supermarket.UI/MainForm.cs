using System;
using System.Drawing;
using System.Windows.Forms;

namespace Supermarket.UI
{
    public partial class MainForm : Form
    {
        private Panel sidePanel;
        private Panel headerPanel;
        private Panel contentPanel;
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

            // Side Panel
            sidePanel = new Panel { Dock = DockStyle.Left, Width = 200, BackColor = Color.FromArgb(45, 45, 48) };

            // Header Panel
            headerPanel = new Panel { Dock = DockStyle.Top, Height = 60, BackColor = Color.FromArgb(28, 28, 28) };
            lblTitle = new Label { Text = "Dashboard / اللوحة الرئيسية", ForeColor = Color.White, Font = new Font("Arial", 16, FontStyle.Bold), Location = new Point(20, 15), AutoSize = true };
            headerPanel.Controls.Add(lblTitle);

            // Menu Buttons
            AddMenuButton("POS / نقطة البيع", 0, (s, e) => {
                PosForm frm = new PosForm();
                frm.MdiParent = this;
                frm.Show();
            });
            AddMenuButton("Purchases / المشتريات", 50, (s, e) => MessageBox.Show("Opening Purchases..."));
            AddMenuButton("Items / الأصناف", 100, (s, e) => {
                ItemsForm frm = new ItemsForm();
                frm.MdiParent = this;
                frm.Show();
            });
            AddMenuButton("Reports / التقارير", 150, (s, e) => MessageBox.Show("Opening Reports..."));
            AddMenuButton("Settings / الإعدادات", 200, (s, e) => MessageBox.Show("Opening Settings..."));

            this.Controls.Add(headerPanel);
            this.Controls.Add(sidePanel);

            LanguageHelper.ApplyLanguage(this);
        }

        private void AddMenuButton(string text, int top, EventHandler onClick)
        {
            Button btn = new Button
            {
                Text = text,
                Top = top,
                Width = 200,
                Height = 50,
                FlatStyle = FlatStyle.Flat,
                ForeColor = Color.White,
                TextAlign = ContentAlignment.MiddleLeft,
                Padding = new Padding(10, 0, 0, 0)
            };
            btn.FlatAppearance.BorderSize = 0;
            btn.Click += onClick;
            sidePanel.Controls.Add(btn);
        }

        private void InitializeComponent()
        {
            this.SuspendLayout();
            this.ClientSize = new System.Drawing.Size(1184, 761);
            this.Name = "MainForm";
            this.ResumeLayout(false);
        }
    }
}
