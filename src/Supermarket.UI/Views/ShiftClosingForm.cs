using Supermarket.UI.Helpers;
using System;
using System.Drawing;
using System.Windows.Forms;
using Supermarket.DAL;
using Dapper;

namespace Supermarket.UI.Views
{
    public partial class ShiftClosingForm : Form
    {
        private ShiftRepository _shiftRepo;
        private Label lblExpected;
        private TextBox txtActual;

        public ShiftClosingForm()
        {
            InitializeComponent();
            _shiftRepo = new ShiftRepository(AppSettings.ConnectionString);
            SetupUI();
        }

        private void SetupUI()
        {
            bool isArabic = LanguageHelper.TranslationService.CurrentLanguage == Supermarket.BLL.Services.Language.Arabic;
            LoadShiftData();
            this.Text = "Z-Report & Shift Closing / إغلاق الوردية";
            this.Size = new Size(500, 450);
            this.StartPosition = FormStartPosition.CenterParent;

            lblExpected = new Label { Text = isArabic ? "المبلغ المتوقع: 0.00 ريال" : "Expected Amount: 0.00 YER", Location = new Point(20, 30), Font = new Font("Arial", 14, FontStyle.Bold), AutoSize = true, ForeColor = Color.Blue };
            this.Controls.Add(lblExpected);

            this.Controls.Add(new Label { Text = "Actual Amount / المبلغ الفعلي", Location = new Point(20, 80), AutoSize = true, Font = new Font("Arial", 12) });
            txtActual = new TextBox { Location = new Point(200, 78), Width = 200, Font = new Font("Arial", 14) };
            this.Controls.Add(txtActual);

            Button btnClose = new Button { Text = "CLOSE SHIFT / إغلاق الوردية", Location = new Point(20, 200), Width = 440, Height = 60, BackColor = Color.Red, ForeColor = Color.White, Font = new Font("Arial", 16, FontStyle.Bold) };
            btnClose.Click += async (s, e) => {
                decimal actual = decimal.Parse(txtActual.Text);
                await _shiftRepo.CloseShiftAsync(1, actual, 1); // Dummy IDs
                MessageBox.Show("Shift Closed and Z-Report Printed / تم إغلاق الوردية وطباعة التقرير");
                this.Close();
            };
            this.Controls.Add(btnClose);

            LanguageHelper.ApplyLanguage(this);
        }

        private async void LoadShiftData()
        {
            bool isArabic = LanguageHelper.TranslationService.CurrentLanguage == Supermarket.BLL.Services.Language.Arabic;
            string conn = AppSettings.ConnectionString;
            using (var db = new Microsoft.Data.SqlClient.SqlConnection(conn))
            {
                var data = await db.QueryFirstOrDefaultAsync<dynamic>(@"
                    SELECT ExpectedAmount,
                    (SELECT ISNULL(SUM(NetAmount), 0) FROM SalesInvoices WHERE CreatedBy = s.UserID AND InvoiceDate >= s.StartTime AND PaymentType = 'Cash') as CashTotal,
                    (SELECT ISNULL(SUM(NetAmount), 0) FROM SalesInvoices WHERE CreatedBy = s.UserID AND InvoiceDate >= s.StartTime AND PaymentType = 'Card') as CardTotal
                    FROM CashierShifts s WHERE Status = 'Open' AND UserID = 1", new { userId = 1 });

                if (data != null) {
                    if (isArabic)
                        lblExpected.Text = $"المتوقع (نقدي): {data.CashTotal:F2} ريال\nالمتوقع (بطاقة): {data.CardTotal:F2} ريال\nالإجمالي: {data.ExpectedAmount:F2} ريال";
                    else
                        lblExpected.Text = $"Expected (Cash): {data.CashTotal:F2} YER\nExpected (Card): {data.CardTotal:F2} YER\nTotal: {data.ExpectedAmount:F2} YER";
                }
            }
        }

        private void InitializeComponent() { }
    }
}
