using Supermarket.UI.Helpers;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;
using Supermarket.DAL;
using Supermarket.BLL.Services;
using System.Linq;

namespace Supermarket.UI.Views
{
    public partial class SupplierStatementForm : Form
    {
        private ComboBox cbSupplier;
        private DateTimePicker dtFrom, dtTo;
        private DataGridView dgvReport;
        private Label lblOpening, lblClosing;
        private FinancialReportRepository _reportRepo;
        private ContactRepository _contactRepo;

        public SupplierStatementForm()
        {
            InitializeComponent();
            string conn = AppSettings.ConnectionString;
            _reportRepo = new FinancialReportRepository(conn);
            _contactRepo = new ContactRepository(conn);
            SetupUI();
            LoadSuppliers();
        }

        private void SetupUI()
        {
            bool isArabic = LanguageHelper.TranslationService.CurrentLanguage == Language.Arabic;
            this.Text = isArabic ? "كشف حساب مورد" : "Supplier Statement";
            this.Size = new Size(1000, 700);
            this.BackColor = UITheme.ContentBg;
            this.RightToLeft = isArabic ? RightToLeft.Yes : RightToLeft.No;

            Panel pnlHeader = new Panel { Dock = DockStyle.Top, Height = 80, BackColor = Color.White, Padding = new Padding(20) };
            cbSupplier = new ComboBox { Width = 200, DropDownStyle = ComboBoxStyle.DropDownList };
            dtFrom = new DateTimePicker { Left = 220, Width = 120, Format = DateTimePickerFormat.Short, Value = DateTime.Now.AddMonths(-1) };
            dtTo = new DateTimePicker { Left = 350, Width = 120, Format = DateTimePickerFormat.Short };

            Button btnRun = new Button { Text = isArabic ? "عرض التقرير" : "RUN REPORT", Left = 480, BackColor = UITheme.PrimaryColor, ForeColor = Color.White, FlatStyle = FlatStyle.Flat };
            btnRun.Click += BtnRun_Click;

            pnlHeader.Controls.AddRange(new Control[] { cbSupplier, dtFrom, dtTo, btnRun });

            dgvReport = new DataGridView { Dock = DockStyle.Fill };
            UITheme.ApplyModernStyle(dgvReport);

            Panel pnlFooter = new Panel { Dock = DockStyle.Bottom, Height = 60, BackColor = Color.White };
            lblClosing = new Label { Dock = DockStyle.Right, Width = 300, Font = UITheme.HeaderFont, TextAlign = ContentAlignment.MiddleRight };
            pnlFooter.Controls.Add(lblClosing);

            this.Controls.Add(dgvReport);
            this.Controls.Add(pnlHeader);
            this.Controls.Add(pnlFooter);
        }

        private async void LoadSuppliers()
        {
            cbSupplier.DataSource = (await _contactRepo.GetAllSuppliersAsync()).ToList();
            cbSupplier.DisplayMember = "SupplierName";
            cbSupplier.ValueMember = "SupplierID";
        }

        private async void BtnRun_Click(object sender, EventArgs e)
        {
            if (cbSupplier.SelectedValue is int id)
            {
                var data = await _reportRepo.GetSupplierStatementAsync(id, dtFrom.Value, dtTo.Value);
                dgvReport.DataSource = data.ToList();

                decimal balance = data.Sum(x => (decimal)x.Debit - (decimal)x.Credit);
                lblClosing.Text = (LanguageHelper.TranslationService.CurrentLanguage == Language.Arabic ? "الرصيد النهائي: " : "Final Balance: ") + balance.ToString("N2");
            }
        }

        private void InitializeComponent() { }
    }
}
