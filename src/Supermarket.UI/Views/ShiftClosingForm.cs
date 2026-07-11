using Supermarket.UI.Helpers;
using System;
using System.Drawing;
using System.Windows.Forms;
using Supermarket.DAL;
using Supermarket.BLL.Services;

namespace Supermarket.UI.Views
{
    public partial class ShiftClosingForm : Form
    {
        private ShiftRepository _shiftRepo;
        private Label lblOpening, lblExpected, lblVariance;
        private TextBox txtActual;
        private int _currentShiftId = 0;
        private decimal _expectedAmount = 0;

        public ShiftClosingForm()
        {
            InitializeComponent();
            _shiftRepo = new ShiftRepository(AppSettings.ConnectionString);
            SetupUI();
            LoadShiftData();
        }

        private void SetupUI()
        {
            bool isArabic = LanguageHelper.TranslationService.CurrentLanguage == Language.Arabic;
            this.Text = isArabic ? "إغلاق الوردية - تقرير Z" : "Shift Closing - Z Report";
            this.Size = new Size(500, 600);
            this.BackColor = UITheme.ContentBg;
            this.StartPosition = FormStartPosition.CenterScreen;
            this.RightToLeft = isArabic ? RightToLeft.Yes : RightToLeft.No;

            Panel pnlMain = new Panel { Dock = DockStyle.Fill, Padding = new Padding(30), BackColor = Color.White };

            lblOpening = CreateReportRow(isArabic ? "رصيد الافتتاح:" : "Opening Balance:", pnlMain);
            lblExpected = CreateReportRow(isArabic ? "المبلغ المتوقع (مبيعات):" : "Expected (Sales):", pnlMain);

            pnlMain.Controls.Add(new Label { Text = isArabic ? "المبلغ الفعلي في الدرج:" : "Actual Cash in Drawer:", Dock = DockStyle.Top, Height = 30, Font = UITheme.MainFont });
            txtActual = new TextBox { Dock = DockStyle.Top, Height = 40, Font = new Font("Segoe UI", 18, FontStyle.Bold), TextAlign = HorizontalAlignment.Center };
            txtActual.TextChanged += (s, e) => CalculateVariance();
            pnlMain.Controls.Add(txtActual);

            pnlMain.Controls.Add(new Panel { Dock = DockStyle.Top, Height = 20 });
            lblVariance = CreateReportRow(isArabic ? "الفارق (عجز/زيادة):" : "Variance (Short/Over):", pnlMain);
            lblVariance.Font = new Font("Segoe UI", 14, FontStyle.Bold);

            Button btnClose = new Button {
                Text = isArabic ? "إغلاق الوردية وترحيل الحسابات" : "CLOSE SHIFT & POST",
                Dock = DockStyle.Bottom, Height = 60,
                BackColor = UITheme.PrimaryColor, ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat, Font = new Font("Segoe UI", 12, FontStyle.Bold)
            };
            btnClose.Click += BtnClose_Click;

            this.Controls.Add(pnlMain);
            this.Controls.Add(btnClose);
            LanguageHelper.ApplyLanguage(this);
        }

        private Label CreateReportRow(string label, Panel p)
        {
            Panel row = new Panel { Dock = DockStyle.Top, Height = 40 };
            Label lblL = new Label { Text = label, Dock = DockStyle.Left, Width = 200, TextAlign = ContentAlignment.MiddleLeft, Font = UITheme.GridFont };
            Label lblV = new Label { Text = "0.00", Dock = DockStyle.Fill, TextAlign = ContentAlignment.MiddleRight, Font = UITheme.MainFont };
            row.Controls.Add(lblV);
            row.Controls.Add(lblL);
            p.Controls.Add(row);
            return lblV;
        }

        private async void LoadShiftData()
        {
            var shift = await _shiftRepo.GetCurrentShiftAsync(1); // Demo User 1
            if (shift != null)
            {
                _currentShiftId = shift.ShiftID;
                lblOpening.Text = shift.OpeningBalance.ToString("N2");
                _expectedAmount = await _shiftRepo.CalculateExpectedAmountAsync(_currentShiftId);
                lblExpected.Text = _expectedAmount.ToString("N2");
            }
        }

        private void CalculateVariance()
        {
            if (decimal.TryParse(txtActual.Text, out decimal actual))
            {
                decimal variance = actual - _expectedAmount;
                lblVariance.Text = variance.ToString("N2");
                lblVariance.ForeColor = variance < 0 ? Color.Red : (variance > 0 ? Color.Green : Color.Black);
            }
        }

        private async void BtnClose_Click(object sender, EventArgs e)
        {
            if (decimal.TryParse(txtActual.Text, out decimal actual))
            {
                await _shiftRepo.CloseShiftAsync(_currentShiftId, actual);
                MessageBox.Show(LanguageHelper.TranslationService.CurrentLanguage == Language.Arabic ? "تم إغلاق الوردية وترحيل الفوارق بنجاح" : "Shift closed and variances posted!");
                this.Close();
            }
        }

        private void InitializeComponent() { }
    }
}
