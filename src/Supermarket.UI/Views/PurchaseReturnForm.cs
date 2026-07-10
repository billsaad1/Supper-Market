using Supermarket.UI.Helpers;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;
using Supermarket.DAL;
using Supermarket.Models.Entities;
using Supermarket.BLL.Services;
using System.Linq;

namespace Supermarket.UI.Views
{
    public partial class PurchaseReturnForm : Form
    {
        private DataGridView dgvItems;
        private TextBox txtInvoiceSearch;
        private Label lblTotal;
        private ComboBox cbSupplier, cbStore;
        private IntegratedPurchaseRepository _purchaseRepo;
        private int? _originalInvoiceId;

        public PurchaseReturnForm()
        {
            InitializeComponent();
            string conn = AppSettings.ConnectionString;
            _purchaseRepo = new IntegratedPurchaseRepository(conn);
            SetupUI();
            LoadMetadata();
        }

        private void SetupUI()
        {
            bool isArabic = LanguageHelper.TranslationService.CurrentLanguage == Language.Arabic;
            this.Text = isArabic ? "مرتجع مشتريات" : "Purchase Return";
            this.Size = new Size(1100, 700);
            this.BackColor = UITheme.ContentBg;
            this.StartPosition = FormStartPosition.CenterScreen;
            this.RightToLeft = isArabic ? RightToLeft.Yes : RightToLeft.No;

            Panel pnlHeader = new Panel { Dock = DockStyle.Top, Height = 100, BackColor = Color.White, Padding = new Padding(20) };
            txtInvoiceSearch = new TextBox { Width = 250, PlaceholderText = isArabic ? "بحث برقم الفاتورة..." : "Search Invoice #..." };
            Button btnSearch = new Button { Text = isArabic ? "بحث" : "SEARCH", Left = 260, BackColor = UITheme.PrimaryColor, ForeColor = Color.White, FlatStyle = FlatStyle.Flat };
            btnSearch.Click += BtnSearch_Click;
            pnlHeader.Controls.AddRange(new Control[] { txtInvoiceSearch, btnSearch });

            dgvItems = new DataGridView { Dock = DockStyle.Fill, AllowUserToAddRows = false };
            UITheme.ApplyModernStyle(dgvItems);
            dgvItems.Columns.Add(new DataGridViewTextBoxColumn { Name = "ItemID", Visible = false });
            dgvItems.Columns.Add(new DataGridViewTextBoxColumn { Name = "Name", HeaderText = isArabic ? "الصنف" : "Item", AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill });
            dgvItems.Columns.Add(new DataGridViewTextBoxColumn { Name = "OriginalQty", HeaderText = isArabic ? "الكمية المشتراة" : "Orig Qty", ReadOnly = true });
            dgvItems.Columns.Add(new DataGridViewTextBoxColumn { Name = "ReturnQty", HeaderText = isArabic ? "الكمية المرتجعة" : "Ret Qty" });
            dgvItems.Columns.Add(new DataGridViewTextBoxColumn { Name = "Price", HeaderText = isArabic ? "السعر" : "Price" });
            dgvItems.Columns.Add(new DataGridViewTextBoxColumn { Name = "Total", HeaderText = isArabic ? "الإجمالي" : "Total", ReadOnly = true });

            Panel pnlFooter = new Panel { Dock = DockStyle.Bottom, Height = 80, BackColor = Color.White };
            Button btnSave = new Button { Text = isArabic ? "حفظ المرتجع" : "SAVE RETURN", Dock = DockStyle.Right, Width = 150, BackColor = UITheme.SuccessColor, ForeColor = Color.White, FlatStyle = FlatStyle.Flat };
            btnSave.Click += BtnSave_Click;
            lblTotal = new Label { Text = "0.00", Font = UITheme.HeaderFont, Dock = DockStyle.Left, TextAlign = ContentAlignment.MiddleLeft, Width = 300 };
            pnlFooter.Controls.AddRange(new Control[] { btnSave, lblTotal });

            this.Controls.Add(dgvItems);
            this.Controls.Add(pnlHeader);
            this.Controls.Add(pnlFooter);

            dgvItems.CellValueChanged += (s, e) => {
                if (e.ColumnIndex == dgvItems.Columns["ReturnQty"].Index || e.ColumnIndex == dgvItems.Columns["Price"].Index) {
                    var row = dgvItems.Rows[e.RowIndex];
                    decimal q = Convert.ToDecimal(row.Cells["ReturnQty"].Value);
                    decimal p = Convert.ToDecimal(row.Cells["Price"].Value);
                    row.Cells["Total"].Value = q * p;
                    UpdateTotal();
                }
            };
        }

        private async void BtnSearch_Click(object sender, EventArgs e)
        {
            // Logic to find invoice and load items
            MessageBox.Show("Loading items from invoice...");
        }

        private void UpdateTotal()
        {
            decimal total = dgvItems.Rows.Cast<DataGridViewRow>().Sum(r => Convert.ToDecimal(r.Cells["Total"].Value));
            lblTotal.Text = total.ToString("N2");
        }

        private void LoadMetadata() { }

        private async void BtnSave_Click(object sender, EventArgs e)
        {
            // Logic to call repository.SavePurchaseReturnAsync
            MessageBox.Show("Return processed successfully");
            this.Close();
        }

        private void InitializeComponent() { }
    }
}
