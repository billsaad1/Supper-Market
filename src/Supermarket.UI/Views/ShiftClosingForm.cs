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
            this.Text = isArabic ? "إغلاق الوردية" : "Shift Closing";
            this.Size = new Size(600, 500);
            this.StartPosition = FormStartPosition.CenterParent;
            this.BackColor = UITheme.ContentBg;
            this.RightToLeft = isArabic ? RightToLeft.Yes : RightToLeft.No;

            Panel pnlCard = new Panel {
                Width = 540, Height = 400,
                Location = new Point(20, 20),
                BackColor = Color.White,
                Padding = new Padding(25)
            };

            lblExpected = new Label {
                Text = isArabic ? "المبلغ المتوقع: 0.00 ريال" : "Expected Amount: 0.00 YER",
                Location = new Point(25, 25),
                Font = new Font("Segoe UI", 12, FontStyle.Bold),
                AutoSize = true,
                ForeColor = UITheme.PrimaryColor
            };
            pnlCard.Controls.Add(lblExpected);

            Label lblActualPrompt = new Label {
                Text = isArabic ? "المبلغ الفعلي في الدرج:" : "Actual Amount in Drawer:",
                Location = new Point(25, 120),
                AutoSize = true,
                Font = UITheme.MainFont
            };
            pnlCard.Controls.Add(lblActualPrompt);

            txtActual = new TextBox {
                Location = new Point(25, 150),
                Width = 490,
                Font = new Font("Segoe UI", 24, FontStyle.Bold),
                TextAlign = HorizontalAlignment.Center
            };
            pnlCard.Controls.Add(txtActual);

            Button btnClose = new Button {
                Text = isArabic ? "إغلاق الوردية وطباعة التقرير" : "CLOSE SHIFT & PRINT",
                Location = new Point(25, 250),
                Width = 490, Height = 70,
                BackColor = Color.FromArgb(220, 53, 69),
                ForeColor = Color.White,
                Font = new Font("Segoe UI", 16, FontStyle.Bold),
                FlatStyle = FlatStyle.Flat
            };
            btnClose.FlatAppearance.BorderSize = 0;
            btnClose.Click += async (s, e) => {
                if (decimal.TryParse(txtActual.Text, out decimal actual)) {
                    await _shiftRepo.CloseShiftAsync(1, actual, 1); // Using dummy ID 1 for current shift/user
                    MessageBox.Show(isArabic ? "تم إغلاق الوردية بنجاح" : "Shift Closed Successfully!");
                    this.Close();
                }
            };
            pnlCard.Controls.Add(btnClose);

            this.Controls.Add(pnlCard);

            LoadShiftData();
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
