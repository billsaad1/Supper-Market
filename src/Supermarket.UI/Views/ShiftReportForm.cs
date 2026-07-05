using Supermarket.UI.Helpers;
using System;
using System.Drawing;
using System.Windows.Forms;

namespace Supermarket.UI.Views
{
    public partial class ShiftReportForm : Form
    {
        public ShiftReportForm(int shiftId)
        {
            InitializeComponent();
            SetupUI(shiftId);
        }

        private void SetupUI(int shiftId)
        {
            this.Text = "Z-Report / تقرير الوردية";
            this.Size = new Size(400, 500);

            Label lbl = new Label {
                Text = $"Shift ID: {shiftId}\n" +
                       "---------------------------\n" +
                       "Cash Sales: 1,200.00\n" +
                       "Card Sales: 800.00\n" +
                       "Credit Sales: 200.00\n" +
                       "---------------------------\n" +
                       "Total Sales: 2,200.00\n" +
                       "VAT: 330.00\n" +
                       "Net: 2,530.00",
                Dock = DockStyle.Fill,
                Font = new Font("Courier New", 12),
                Padding = new Padding(20)
            };

            this.Controls.Add(lbl);
            LanguageHelper.ApplyLanguage(this);
        }

        private void InitializeComponent() { }
    }
}
