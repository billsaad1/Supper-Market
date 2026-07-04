using System;
using System.Drawing;
using System.Windows.Forms;

namespace Supermarket.UI
{
    public partial class DashboardView : Form
    {
        public DashboardView()
        {
            InitializeComponent();
            SetupUI();
        }

        private void SetupUI()
        {
            this.Text = "Dashboard / لوحة المعلومات";
            this.BackColor = Color.White;

            TableLayoutPanel layout = new TableLayoutPanel { Dock = DockStyle.Top, Height = 150, ColumnCount = 4, RowCount = 1 };
            layout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 25f));
            layout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 25f));
            layout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 25f));
            layout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 25f));

            layout.Controls.Add(CreateStatCard("Today Sales / مبيعات اليوم", "5,420.00", Color.Teal), 0, 0);
            layout.Controls.Add(CreateStatCard("Invoices / الفواتير", "42", Color.DodgerBlue), 1, 0);
            layout.Controls.Add(CreateStatCard("Shortages / النواقص", "12", Color.Crimson), 2, 0);
            layout.Controls.Add(CreateStatCard("Cash / الصندوق", "12,300.00", Color.Green), 3, 0);

            this.Controls.Add(layout);

            DataGridView dgvRecent = new DataGridView { Dock = DockStyle.Fill, AutoGenerateColumns = true, BackgroundColor = Color.White, BorderStyle = BorderStyle.None };
            Label lblRecent = new Label { Text = "Recent Transactions / آخر العمليات", Dock = DockStyle.Top, Height = 40, Font = new Font("Arial", 12, FontStyle.Bold), TextAlign = ContentAlignment.BottomLeft };

            this.Controls.Add(dgvRecent);
            this.Controls.Add(lblRecent);

            LanguageHelper.ApplyLanguage(this);
        }

        private Panel CreateStatCard(string title, string value, Color color)
        {
            Panel p = new Panel { Margin = new Padding(10), BackColor = color };
            Label lblVal = new Label { Text = value, ForeColor = Color.White, Font = new Font("Arial", 18, FontStyle.Bold), Dock = DockStyle.Fill, TextAlign = ContentAlignment.MiddleCenter };
            Label lblTitle = new Label { Text = title, ForeColor = Color.White, Font = new Font("Arial", 10), Dock = DockStyle.Top, Height = 30, TextAlign = ContentAlignment.MiddleCenter };
            p.Controls.Add(lblVal);
            p.Controls.Add(lblTitle);
            return p;
        }

        private void InitializeComponent() { }
    }
}
