using Supermarket.UI.Helpers;
using System;
using System.Drawing;
using System.Windows.Forms;
using System.Collections.Generic;
using Supermarket.BLL.Services;
using Supermarket.DAL;
using Dapper;

namespace Supermarket.UI.Views
{
    public partial class SalesReturnForm : Form
    {
        private BusinessFlowService _flowService;
        private ReturnsRepository _returnRepo;
        private DataGridView dgvItems;
        private TextBox txtInvoiceNum;
        private int _currentSalesId = 0;

        public SalesReturnForm()
        {
            InitializeComponent();
            string conn = AppSettings.ConnectionString;
            _flowService = new BusinessFlowService(conn);
            _returnRepo = new ReturnsRepository(conn);
            SetupUI();
        }

        private void SetupUI()
        {
            bool isArabic = LanguageHelper.TranslationService.CurrentLanguage == Supermarket.BLL.Services.Language.Arabic;
            this.Text = isArabic ? "مرتجع مبيعات" : "Sales Returns";
            this.Size = new Size(1100, 750);
            this.BackColor = UITheme.ContentBg;
            this.RightToLeft = isArabic ? RightToLeft.Yes : RightToLeft.No;

            Panel top = new Panel { Dock = DockStyle.Top, Height = 100, BackColor = Color.White, Padding = new Padding(20), BorderStyle = BorderStyle.FixedSingle };
            Label lblInfo = new Label { Text = isArabic ? "رقم الفاتورة الأصلية:" : "Original Invoice #:", Location = new Point(20, 20), AutoSize = true, Font = UITheme.MainFont };
            txtInvoiceNum = new TextBox { Location = new Point(20, 45), Width = 300, Font = new Font("Segoe UI", 12) };

            Button btnSearch = new Button {
                Text = isArabic ? "بحث وتحميل" : "Search & Load",
                Location = new Point(330, 43),
                Width = 150, Height = 32,
                BackColor = UITheme.PrimaryColor,
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 9, FontStyle.Bold)
            };
            btnSearch.FlatAppearance.BorderSize = 0;
            btnSearch.Click += async (s, e) => {
                dgvItems.Rows.Clear();
                _currentSalesId = 0;
                string connStr = AppSettings.ConnectionString;
                using (var db = new Microsoft.Data.SqlClient.SqlConnection(connStr))
                {
                    var invoice = await db.QueryFirstOrDefaultAsync<dynamic>(
                        "SELECT SalesID FROM SalesInvoices WHERE InvoiceNumber = @inv", new { inv = txtInvoiceNum.Text });

                    if (invoice == null) {
                        MessageBox.Show(isArabic ? "الفاتورة غير موجودة" : "Invoice not found");
                        return;
                    }
                    _currentSalesId = invoice.SalesID;

                    var items = await db.QueryAsync<dynamic>(
                        "SELECT i.ItemID, i.ItemName, si.Quantity, si.UnitPrice FROM SalesInvoiceItems si JOIN Items i ON si.ItemID = i.ItemID WHERE si.SalesID = @SID",
                        new { SID = _currentSalesId });

                    foreach (var item in items)
                    {
                        dgvItems.Rows.Add(item.ItemID, item.ItemName, item.Quantity, 0, item.UnitPrice, 0);
                    }
                }
            };

            top.Controls.AddRange(new Control[] { lblInfo, txtInvoiceNum, btnSearch });

            dgvItems = new DataGridView { Dock = DockStyle.Fill, AllowUserToAddRows = false, AutoGenerateColumns = false };
            UITheme.ApplyModernStyle(dgvItems);
            dgvItems.Columns.Add(new DataGridViewTextBoxColumn { Name = "ItemID", Visible = false });
            dgvItems.Columns.Add(new DataGridViewTextBoxColumn { Name = "Name", HeaderText = isArabic ? "الصنف" : "Item", AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill, ReadOnly = true });
            dgvItems.Columns.Add(new DataGridViewTextBoxColumn { Name = "Qty", HeaderText = isArabic ? "الكمية المباعة" : "Sold Qty", Width = 120, ReadOnly = true });
            dgvItems.Columns.Add(new DataGridViewTextBoxColumn { Name = "RetQty", HeaderText = isArabic ? "الكمية المرتجعة" : "Return Qty", Width = 120 });
            dgvItems.Columns.Add(new DataGridViewTextBoxColumn { Name = "Price", HeaderText = isArabic ? "السعر" : "Price", Width = 120, ReadOnly = true });
            dgvItems.Columns.Add(new DataGridViewTextBoxColumn { Name = "Total", HeaderText = isArabic ? "الإجمالي" : "Total", Width = 150, ReadOnly = true });

            dgvItems.CellValueChanged += (s, e) => {
                if (e.ColumnIndex == dgvItems.Columns["RetQty"].Index && e.RowIndex >= 0) {
                    var row = dgvItems.Rows[e.RowIndex];
                    if (decimal.TryParse(row.Cells["RetQty"].Value?.ToString(), out decimal qty)) {
                        decimal soldQty = Convert.ToDecimal(row.Cells["Qty"].Value);
                        if (qty > soldQty) {
                            MessageBox.Show(isArabic ? "الكمية المرتجعة لا يمكن أن تتجاوز الكمية المباعة" : "Return Qty cannot exceed Sold Qty");
                            row.Cells["RetQty"].Value = 0;
                            qty = 0;
                        }
                        decimal price = Convert.ToDecimal(row.Cells["Price"].Value);
                        row.Cells["Total"].Value = qty * price;
                    }
                }
            };

            Panel pnlFooter = new Panel { Dock = DockStyle.Bottom, Height = 80, BackColor = Color.White, BorderStyle = BorderStyle.FixedSingle, Padding = new Padding(10) };
            Button btnSave = new Button {
                Text = isArabic ? "إتمام عملية المرتجع" : "PROCESS RETURN",
                Dock = DockStyle.Fill,
                BackColor = Color.FromArgb(220, 53, 69),
                ForeColor = Color.White,
                Font = new Font("Segoe UI", 14, FontStyle.Bold),
                FlatStyle = FlatStyle.Flat
            };
            btnSave.FlatAppearance.BorderSize = 0;
            btnSave.Click += async (s, e) => {
                if (_currentSalesId == 0) {
                    MessageBox.Show(isArabic ? "يرجى البحث عن فاتورة أولاً" : "Please search for an invoice first");
                    return;
                }

                List<dynamic> itemsToReturn = new List<dynamic>();
                foreach (DataGridViewRow row in dgvItems.Rows) {
                    decimal retQty = Convert.ToDecimal(row.Cells["RetQty"].Value ?? 0);
                    if (retQty > 0) {
                        itemsToReturn.Add(new { ItemID = row.Cells["ItemID"].Value, Qty = retQty });
                    }
                }

                if (itemsToReturn.Count > 0) {
                    await _returnRepo.ProcessSalesReturnAsync(_currentSalesId, itemsToReturn, 1);
                    MessageBox.Show(isArabic ? "تم إتمام المرتجع بنجاح" : "Return Processed Successfully!");
                    this.Close();
                }
            };
            pnlFooter.Controls.Add(btnSave);

            this.Controls.Add(dgvItems);
            this.Controls.Add(top);
            this.Controls.Add(pnlFooter);

            LanguageHelper.ApplyLanguage(this);
        }

        private void InitializeComponent() { }
    }
}
