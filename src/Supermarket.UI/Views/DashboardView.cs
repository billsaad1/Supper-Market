using Supermarket.UI.Helpers;
using System;
using System.Drawing;
using System.Windows.Forms;
using Supermarket.BLL.Services;
using Dapper;

namespace Supermarket.UI.Views
{
    public partial class DashboardView : Form
    {
        private BusinessFlowService _flowService;
        private Label lblSalesValue, lblInvoiceCount, lblShortages, lblCash;

        public DashboardView()
        {
            InitializeComponent();
            _flowService = new BusinessFlowService(AppSettings.ConnectionString);
            SetupUI();
            LoadLiveStats();
        }

        private void SetupUI()
        {
            this.Text = "Live Dashboard / لوحة المعلومات الحية";
            this.BackColor = Color.White;

            TableLayoutPanel layout = new TableLayoutPanel { Dock = DockStyle.Top, Height = 160, ColumnCount = 4, RowCount = 1, Padding = new Padding(10) };
            layout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 25f));
            layout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 25f));
            layout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 25f));
            layout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 25f));

            layout.Controls.Add(CreateStatCard("Today Sales / مبيعات اليوم", out lblSalesValue, Color.FromArgb(0, 122, 204)), 0, 0);
            layout.Controls.Add(CreateStatCard("Invoices / الفواتير", out lblInvoiceCount, Color.FromArgb(30, 150, 70)), 1, 0);
            layout.Controls.Add(CreateStatCard("Shortages / النواقص", out lblShortages, Color.FromArgb(204, 0, 0)), 2, 0);
            layout.Controls.Add(CreateStatCard("Cash Balance / رصيد الصندوق", out lblCash, Color.FromArgb(200, 150, 0)), 3, 0);

            this.Controls.Add(layout);

            DataGridView dgvRecent = new DataGridView { Dock = DockStyle.Fill, AutoGenerateColumns = true, BackgroundColor = Color.White, BorderStyle = BorderStyle.None, ReadOnly = true };
            Label lblRecent = new Label { Text = "Recent Sales / آخر المبيعات", Dock = DockStyle.Top, Height = 40, Font = new Font("Arial", 12, FontStyle.Bold), TextAlign = ContentAlignment.BottomLeft, Padding = new Padding(10, 0, 0, 5) };

            this.Controls.Add(dgvRecent);
            this.Controls.Add(lblRecent);

            LanguageHelper.ApplyLanguage(this);
        }

        private Panel CreateStatCard(string title, out Label valueLabel, Color color)
        {
            Panel p = new Panel { Margin = new Padding(10), BackColor = color, Height = 120 };
            valueLabel = new Label { Text = "0.00", ForeColor = Color.White, Font = new Font("Segoe UI", 20, FontStyle.Bold), Dock = DockStyle.Fill, TextAlign = ContentAlignment.MiddleCenter };
            Label lblTitle = new Label { Text = title, ForeColor = Color.White, Font = new Font("Segoe UI", 10), Dock = DockStyle.Top, Height = 35, TextAlign = ContentAlignment.MiddleCenter };
            p.Controls.Add(valueLabel);
            p.Controls.Add(lblTitle);
            return p;
        }

        private async void LoadLiveStats()
        {
            try
            {
                string conn = AppSettings.ConnectionString;
                using (var db = new Microsoft.Data.SqlClient.SqlConnection(conn))
                {
                    var stats = await db.QueryFirstOrDefaultAsync<dynamic>(@"
                        SELECT
                            (SELECT ISNULL(SUM(NetAmount), 0) FROM SalesInvoices WHERE CAST(InvoiceDate AS DATE) = CAST(GETDATE() AS DATE)) as TodaySales,
                            (SELECT COUNT(*) FROM SalesInvoices WHERE CAST(InvoiceDate AS DATE) = CAST(GETDATE() AS DATE)) as TodayCount,
                            (SELECT COUNT(*) FROM (SELECT i.ItemID FROM Items i JOIN Stock s ON i.ItemID = s.ItemID GROUP BY i.ItemID, i.MinimumStockLevel HAVING SUM(s.Quantity) <= i.MinimumStockLevel) AS Short) as ShortCount,
                            (SELECT ISNULL(SUM(Debit - Credit), 0) FROM JournalEntryDetails WHERE AccountID = 1) as CashBalance");

                    if (stats != null)
                    {
                        lblSalesValue.Text = $"{stats.TodaySales:F2} SR";
                        lblInvoiceCount.Text = stats.TodayCount.ToString();
                        lblShortages.Text = $"{stats.ShortCount} Items";
                        lblCash.Text = $"{stats.CashBalance:F2} SR";
                    }

                    var recent = await db.QueryAsync<dynamic>("SELECT TOP 10 InvoiceNumber, NetAmount, InvoiceDate FROM SalesInvoices ORDER BY InvoiceDate DESC");
                    // Update grid if needed
                }
            }
            catch { /* Database might not be ready */ }
        }

        private void InitializeComponent() { }
    }
}
