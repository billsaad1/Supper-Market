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
                    SELECT
                        ISNULL(SUM(CASE WHEN PaymentType LIKE '%Cash%' OR PaymentType LIKE '%نقدي%' THEN TotalAmount ELSE 0 END), 0) as CashTotal,
                        ISNULL(SUM(CASE WHEN PaymentType LIKE '%Card%' OR PaymentType LIKE '%بطاقة%' THEN TotalAmount ELSE 0 END), 0) as CardTotal,
                        ISNULL(SUM(TotalAmount), 0) as GrandTotal
                    FROM SalesInvoices
                    WHERE InvoiceDate >= (SELECT StartTime FROM CashierShifts WHERE Status = 'Open' AND UserID = 1)");

                if (data != null) {
                    if (isArabic)
                        lblExpected.Text = $"المتوقع (نقدي): {data.CashTotal:N2} ريال\n" +
                                         $"المتوقع (بطاقة): {data.CardTotal:N2} ريال\n" +
                                         $"الإجمالي العام: {data.GrandTotal:N2} ريال";
                    else
                        lblExpected.Text = $"Expected (Cash): {data.CashTotal:N2} YER\n" +
                                         $"Expected (Card): {data.CardTotal:N2} YER\n" +
                                         $"Total Sales: {data.GrandTotal:N2} YER";
                }
            }
        }

        private void InitializeComponent() { }
    }
}
