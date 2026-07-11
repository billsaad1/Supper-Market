using Supermarket.UI.Helpers;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;
using Supermarket.DAL;
using Supermarket.Models.Entities;
using Supermarket.BLL.Services;
using System.Linq;
using Dapper;

namespace Supermarket.UI.Views
{
    public partial class PurchaseReturnForm : Form
    {
        private DataGridView dgvItems;
        private TextBox txtInvoiceSearch;
        private Label lblTotal;
        private IntegratedPurchaseRepository _purchaseRepo;
        private int? _originalInvoiceId;
        private int _supplierId;
        private int _storeId;

        public PurchaseReturnForm()
        {
            InitializeComponent();
            string conn = AppSettings.ConnectionString;
            _purchaseRepo = new IntegratedPurchaseRepository(conn);
            SetupUI();
        }

        private void SetupUI()
        {
            bool isArabic = LanguageHelper.TranslationService.CurrentLanguage == Language.Arabic;
            this.Text = isArabic ? "مرتجع مشتريات احترافي" : "Professional Purchase Return";
            this.Size = new Size(1100, 700);
            this.BackColor = UITheme.ContentBg;
            this.StartPosition = FormStartPosition.CenterScreen;
            this.RightToLeft = isArabic ? RightToLeft.Yes : RightToLeft.No;

            Panel pnlHeader = new Panel { Dock = DockStyle.Top, Height = 100, BackColor = Color.White, Padding = new Padding(20) };
            txtInvoiceSearch = new TextBox { Width = 250, PlaceholderText = isArabic ? "رقم فاتورة المشتريات..." : "Purchase Invoice #..." };
            Button btnSearch = new Button {
                Text = isArabic ? "بحث" : "SEARCH",
                Left = 260, Width = 100, Height = 30,
                BackColor = UITheme.PrimaryColor, ForeColor = Color.White, FlatStyle = FlatStyle.Flat
            };
            btnSearch.Click += BtnSearch_Click;
            pnlHeader.Controls.AddRange(new Control[] { txtInvoiceSearch, btnSearch });

            dgvItems = new DataGridView { Dock = DockStyle.Fill, AllowUserToAddRows = false };
            UITheme.ApplyModernStyle(dgvItems);
            dgvItems.Columns.Add(new DataGridViewTextBoxColumn { Name = "ItemID", Visible = false });
            dgvItems.Columns.Add(new DataGridViewTextBoxColumn { Name = "Name", HeaderText = isArabic ? "الصنف" : "Item", AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill, ReadOnly = true });
            dgvItems.Columns.Add(new DataGridViewTextBoxColumn { Name = "OriginalQty", HeaderText = isArabic ? "الكمية المشتراة" : "Orig Qty", Width = 120, ReadOnly = true });
            dgvItems.Columns.Add(new DataGridViewTextBoxColumn { Name = "ReturnQty", HeaderText = isArabic ? "الكمية المرتجعة" : "Ret Qty", Width = 120 });
            dgvItems.Columns.Add(new DataGridViewTextBoxColumn { Name = "Price", HeaderText = isArabic ? "سعر الشراء" : "Purchase Price", Width = 120, ReadOnly = true });
            dgvItems.Columns.Add(new DataGridViewTextBoxColumn { Name = "Total", HeaderText = isArabic ? "الإجمالي" : "Total", Width = 150, ReadOnly = true });

            Panel pnlFooter = new Panel { Dock = DockStyle.Bottom, Height = 80, BackColor = Color.White, Padding = new Padding(15) };
            Button btnSave = new Button {
                Text = isArabic ? "حفظ وإرجاع للمورد" : "PROCESS RETURN",
                Dock = DockStyle.Right, Width = 200,
                BackColor = Color.FromArgb(220, 53, 69), ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat, Font = new Font("Segoe UI", 12, FontStyle.Bold)
            };
            btnSave.Click += BtnSave_Click;
            lblTotal = new Label { Text = "0.00", Font = new Font("Segoe UI", 24, FontStyle.Bold), Dock = DockStyle.Left, TextAlign = ContentAlignment.MiddleLeft, Width = 300 };
            pnlFooter.Controls.AddRange(new Control[] { btnSave, lblTotal });

            this.Controls.Add(dgvItems);
            this.Controls.Add(pnlHeader);
            this.Controls.Add(pnlFooter);

            dgvItems.CellValueChanged += (s, e) => {
                if (e.ColumnIndex == dgvItems.Columns["ReturnQty"].Index && e.RowIndex >= 0) {
                    var row = dgvItems.Rows[e.RowIndex];
                    decimal orig = Convert.ToDecimal(row.Cells["OriginalQty"].Value);
                    if (decimal.TryParse(row.Cells["ReturnQty"].Value?.ToString(), out decimal ret)) {
                        if (ret > orig) {
                            MessageBox.Show(isArabic ? "لا يمكن إرجاع كمية أكبر من المشتراة" : "Cannot return more than purchased");
                            row.Cells["ReturnQty"].Value = 0;
                            ret = 0;
                        }
                        decimal p = Convert.ToDecimal(row.Cells["Price"].Value);
                        row.Cells["Total"].Value = ret * p;
                        UpdateTotal();
                    }
                }
            };
        }

        private async void BtnSearch_Click(object sender, EventArgs e)
        {
            bool isArabic = LanguageHelper.TranslationService.CurrentLanguage == Language.Arabic;
            using (var db = new Microsoft.Data.SqlClient.SqlConnection(AppSettings.ConnectionString))
            {
                var inv = await db.QueryFirstOrDefaultAsync<dynamic>("SELECT * FROM PurchaseInvoices WHERE InvoiceNumber = @num", new { num = txtInvoiceSearch.Text });
                if (inv == null) {
                    MessageBox.Show(isArabic ? "الفاتورة غير موجودة" : "Invoice not found");
                    return;
                }
                _originalInvoiceId = inv.PurchaseID;
                _supplierId = inv.SupplierID;
                _storeId = inv.StoreID;

                var items = await _purchaseRepo.GetPurchaseInvoiceItemsAsync(_originalInvoiceId.Value);
                dgvItems.Rows.Clear();
                foreach (var i in items) dgvItems.Rows.Add(i.ItemID, i.ItemName, i.Quantity, 0, i.UnitPrice, 0);
            }
        }

        private void UpdateTotal()
        {
            decimal total = dgvItems.Rows.Cast<DataGridViewRow>().Sum(r => Convert.ToDecimal(r.Cells["Total"].Value ?? 0));
            lblTotal.Text = total.ToString("N2");
        }

        private async void BtnSave_Click(object sender, EventArgs e)
        {
            if (_originalInvoiceId == null) return;
            var retItems = dgvItems.Rows.Cast<DataGridViewRow>()
                .Where(r => Convert.ToDecimal(r.Cells["ReturnQty"].Value ?? 0) > 0)
                .Select(r => new PurchaseReturnItem {
                    ItemID = (int)r.Cells["ItemID"].Value,
                    Quantity = Convert.ToDecimal(r.Cells["ReturnQty"].Value),
                    UnitPrice = Convert.ToDecimal(r.Cells["Price"].Value),
                    TotalAmount = Convert.ToDecimal(r.Cells["Total"].Value),
                    TaxAmount = Convert.ToDecimal(r.Cells["Total"].Value) * 0.15m
                }).ToList();

            if (retItems.Any()) {
                var ret = new PurchaseReturn {
                    ReturnNumber = "PUR-RET-" + DateTime.Now.Ticks,
                    OriginalPurchaseID = _originalInvoiceId,
                    SupplierID = _supplierId,
                    StoreID = _storeId,
                    TotalAmount = retItems.Sum(i => i.TotalAmount),
                    TaxAmount = retItems.Sum(i => i.TaxAmount),
                    NetAmount = retItems.Sum(i => i.TotalAmount + i.TaxAmount),
                    CreatedBy = 1
                };
                await _purchaseRepo.SavePurchaseReturnAsync(ret, retItems);
                MessageBox.Show(LanguageHelper.TranslationService.CurrentLanguage == Language.Arabic ? "تم حفظ المرتجع وتعديل المخزون والحسابات" : "Return processed successfully");
                this.Close();
            }
        }

        private void InitializeComponent() { }
    }
}
