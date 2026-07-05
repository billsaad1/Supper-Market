using System;
using System.Drawing;
using System.Windows.Forms;
using System.Collections.Generic;
using System.Linq;
using Supermarket.BLL;

namespace Supermarket.UI
{
    public partial class DetailedReportsForm : Form
    {
        private BusinessFlowService _flowService;

        public DetailedReportsForm()
        {
            InitializeComponent();
            _flowService = new BusinessFlowService(AppSettings.ConnectionString);
            SetupUI();
        }

        private void SetupUI()
        {
            this.Text = "Advanced Financial & Inventory Reports / تقارير متقدمة";
            this.Size = new Size(1100, 750);

            TabControl tabs = new TabControl { Dock = DockStyle.Fill, Font = new Font("Segoe UI", 11) };

            tabs.TabPages.Add(CreateReportTab("Sales / المبيعات", async (dgv, from, to) => {
                var data = await _flowService.GetFinancialSummaryAsync(from, to);
                dgv.DataSource = new List<dynamic> { data };
            }));

            tabs.TabPages.Add(CreateReportTab("Inventory / المخزون", async (dgv, from, to) => {
                // Fetch stock data
            }));

            this.Controls.Add(tabs);
            LanguageHelper.ApplyLanguage(this);
        }

        private TabPage CreateReportTab(string title, Action<DataGridView, DateTime, DateTime> loadDataAction)
        {
            TabPage tp = new TabPage(title);
            Panel main = new Panel { Dock = DockStyle.Fill, Padding = new Padding(15) };

            Panel filters = new Panel { Dock = DockStyle.Top, Height = 70, BackColor = Color.WhiteSmoke };
            DateTimePicker dtFrom = new DateTimePicker { Location = new Point(10, 20), Width = 150 };
            DateTimePicker dtTo = new DateTimePicker { Location = new Point(180, 20), Width = 150 };
            Button btnView = new Button { Text = "Show / عرض", Location = new Point(350, 15), Width = 130, Height = 40, BackColor = Color.FromArgb(0, 122, 204), ForeColor = Color.White, FlatStyle = FlatStyle.Flat };

            DataGridView dgv = new DataGridView { Dock = DockStyle.Fill, BackgroundColor = Color.White, AutoGenerateColumns = true, ReadOnly = true };
            btnView.Click += (s, e) => loadDataAction(dgv, dtFrom.Value, dtTo.Value);

            filters.Controls.AddRange(new Control[] { dtFrom, dtTo, btnView });
            main.Controls.Add(dgv);
            main.Controls.Add(filters);
            tp.Controls.Add(main);

            return tp;
        }

        private void InitializeComponent() { }
    }
}
