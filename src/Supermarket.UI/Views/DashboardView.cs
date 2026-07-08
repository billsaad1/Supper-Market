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
            this.Text = "Dashboard";
            this.BackColor = UITheme.ContentBg;
            bool isArabic = LanguageHelper.TranslationService.CurrentLanguage == Supermarket.BLL.Services.Language.Arabic;
            this.RightToLeft = isArabic ? RightToLeft.Yes : RightToLeft.No;

            TableLayoutPanel layout = new TableLayoutPanel { Dock = DockStyle.Top, Height = 160, ColumnCount = 4, RowCount = 1, Padding = new Padding(10), RightToLeft = isArabic ? RightToLeft.Yes : RightToLeft.No };
            layout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 25f));
            layout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 25f));
            layout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 25f));
            layout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 25f));

            layout.Controls.Add(CreateStatCard(isArabic ? "مبيعات اليوم" : "Today Sales", out lblSalesValue, UITheme.PrimaryColor), 0, 0);
            layout.Controls.Add(CreateStatCard(isArabic ? "الفواتير" : "Invoices", out lblInvoiceCount, UITheme.SuccessColor), 1, 0);
            layout.Controls.Add(CreateStatCard(isArabic ? "النواقص" : "Shortages", out lblShortages, UITheme.DangerColor), 2, 0);
            layout.Controls.Add(CreateStatCard(isArabic ? "رصيد الصندوق" : "Cash Balance", out lblCash, UITheme.WarningColor), 3, 0);

            this.Controls.Add(layout);

            Panel pnlRecent = new Panel { Dock = DockStyle.Fill, Padding = new Padding(20) };
            Label lblRecent = new Label { Text = isArabic ? "آخر المبيعات" : "Recent Sales", Dock = DockStyle.Top, Height = 40, Font = UITheme.HeaderFont };
            DataGridView dgvRecent = new DataGridView { Dock = DockStyle.Fill };
            UITheme.ApplyModernStyle(dgvRecent);

            pnlRecent.Controls.Add(dgvRecent);
            pnlRecent.Controls.Add(lblRecent);
            this.Controls.Add(pnlRecent);

            LanguageHelper.ApplyLanguage(this);
        }

        private Panel CreateStatCard(string title, out Label valueLabel, Color color)
        {
            Panel p = new Panel { Margin = new Padding(10), BackColor = Color.White, Height = 120 };
            p.Tag = "Card";

            Panel topBar = new Panel { Dock = DockStyle.Top, Height = 4, BackColor = color };
            valueLabel = new Label { Text = "0.00", ForeColor = UITheme.TextPrimary, Font = new Font("Segoe UI", 18, FontStyle.Bold), Dock = DockStyle.Fill, TextAlign = ContentAlignment.MiddleCenter };
            Label lblTitle = new Label { Text = title, ForeColor = UITheme.TextSecondary, Font = UITheme.MainFont, Dock = DockStyle.Top, Height = 35, TextAlign = ContentAlignment.MiddleCenter };

            p.Controls.Add(valueLabel);
            p.Controls.Add(lblTitle);
            p.Controls.Add(topBar);
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
                        bool isArabic = LanguageHelper.TranslationService.CurrentLanguage == Supermarket.BLL.Services.Language.Arabic;
                        lblSalesValue.Text = isArabic ? $"{stats.TodaySales:F2} ريال" : $"{stats.TodaySales:F2} YER";
                        lblInvoiceCount.Text = stats.TodayCount.ToString();
                        lblShortages.Text = isArabic ? $"{stats.ShortCount} أصناف" : $"{stats.ShortCount} Items";
                        lblCash.Text = isArabic ? $"{stats.CashBalance:F2} ريال" : $"{stats.CashBalance:F2} YER";
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
