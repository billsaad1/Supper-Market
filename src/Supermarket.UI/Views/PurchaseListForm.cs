using Supermarket.UI.Helpers;
using System;
using System.Drawing;
using System.Windows.Forms;
using System.Collections.Generic;
using System.Linq;
using Supermarket.DAL;
using Supermarket.Models.Entities;

namespace Supermarket.UI.Views
{
    public partial class PurchaseListForm : Form
    {
        private DataGridView dgv;
        private IntegratedPurchaseRepository _repo;

        public PurchaseListForm()
        {
            InitializeComponent();
            _repo = new IntegratedPurchaseRepository(AppSettings.ConnectionString);
            SetupUI();
            LoadData();
        }

        private void SetupUI()
        {
            bool isArabic = LanguageHelper.TranslationService.CurrentLanguage == Supermarket.BLL.Services.Language.Arabic;
            this.Text = isArabic ? "فواتير المشتريات" : "Purchase Invoices";
            this.Size = new Size(1150, 700);
            this.BackColor = UITheme.ContentBg;
            this.RightToLeft = isArabic ? RightToLeft.Yes : RightToLeft.No;

            Panel top = new Panel { Dock = DockStyle.Top, Height = 70, BackColor = Color.White, Padding = new Padding(15), BorderStyle = BorderStyle.FixedSingle };

            // Search Box (Left-aligned in LTR, Right-aligned in RTL)
            TextBox txtSearch = new TextBox {
                Width = 350,
                Font = UITheme.MainFont,
                PlaceholderText = isArabic ? "بحث برقم الفاتورة أو المورد..." : "Search Inv # or Supplier...",
                Dock = DockStyle.Left
            };
            txtSearch.TextChanged += async (s, e) => {
                var data = await _repo.GetPurchaseInvoicesAsync();
                if (!string.IsNullOrEmpty(txtSearch.Text)) {
                    string q = txtSearch.Text.ToLower();
                    data = data.Where(p => (p.InvoiceNumber ?? "").ToLower().Contains(q) || (p.SupplierName ?? "").ToLower().Contains(q));
                }
                dgv.DataSource = data.ToList();
            };

            // New Invoice Button (Opposite side of search)
            Button btnNew = new Button {
                Text = isArabic ? "+ فاتورة جديدة" : "+ NEW INVOICE",
                Dock = DockStyle.Right,
                Width = 200,
                BackColor = Color.FromArgb(40, 167, 69),
                ForeColor = Color.White,
                Font = new Font("Segoe UI", 10, FontStyle.Bold),
                FlatStyle = FlatStyle.Flat
            };
            btnNew.FlatAppearance.BorderSize = 0;
            btnNew.Click += (s, e) => {
                using (var form = new PurchaseForm()) {
                    if (form.ShowDialog() == DialogResult.OK) LoadData();
                }
            };

            top.Controls.Add(txtSearch);
            top.Controls.Add(btnNew);

            dgv = new DataGridView { Dock = DockStyle.Fill };
            UITheme.ApplyModernStyle(dgv);
            dgv.AutoGenerateColumns = false;

            dgv.Columns.Add(new DataGridViewTextBoxColumn { Name = "ID", DataPropertyName = "PurchaseID", HeaderText = "ID", Width = 60 });
            dgv.Columns.Add(new DataGridViewTextBoxColumn { Name = "InvNum", DataPropertyName = "InvoiceNumber", HeaderText = isArabic ? "رقم الفاتورة" : "Invoice #", Width = 180 });
            dgv.Columns.Add(new DataGridViewTextBoxColumn { Name = "Supplier", DataPropertyName = "SupplierName", HeaderText = isArabic ? "المورد" : "Supplier", AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill });
            dgv.Columns.Add(new DataGridViewTextBoxColumn { Name = "Store", DataPropertyName = "StoreName", HeaderText = isArabic ? "المخزن" : "Store", Width = 150 });
            dgv.Columns.Add(new DataGridViewTextBoxColumn { Name = "Date", DataPropertyName = "InvoiceDate", HeaderText = isArabic ? "التاريخ" : "Date", Width = 160 });
            dgv.Columns.Add(new DataGridViewTextBoxColumn { Name = "Total", DataPropertyName = "NetAmount", HeaderText = isArabic ? "الإجمالي" : "Total", Width = 120 });

            Panel pnlActions = new Panel { Dock = DockStyle.Bottom, Height = 70, BackColor = Color.White, BorderStyle = BorderStyle.FixedSingle, Padding = new Padding(10) };

            Button btnDelete = CreateActionButton(isArabic ? "حذف" : "Delete", Color.FromArgb(220, 53, 69), 120);
            btnDelete.Click += async (s, e) => {
                if (dgv.CurrentRow != null) {
                    if (MessageBox.Show(isArabic ? "هل أنت متأكد من الحذف وعكس العمليات؟" : "Confirm delete and reversal?", "Delete", MessageBoxButtons.YesNo) == DialogResult.Yes) {
                        await _repo.DeletePurchaseInvoiceAsync((int)dgv.CurrentRow.Cells["ID"].Value);
                        LoadData();
                    }
                }
            };

            Button btnEdit = CreateActionButton(isArabic ? "تعديل" : "Edit", Color.FromArgb(255, 193, 7), 120);
            btnEdit.Click += (s, e) => {
                if (dgv.CurrentRow != null) {
                    using (var form = new PurchaseForm((int)dgv.CurrentRow.Cells["ID"].Value, false)) {
                        if (form.ShowDialog() == DialogResult.OK) LoadData();
                    }
                }
            };

            Button btnPreview = CreateActionButton(isArabic ? "معاينة" : "Preview", Color.FromArgb(23, 162, 184), 120);
            btnPreview.Click += (s, e) => {
                if (dgv.CurrentRow != null) {
                    using (var form = new PurchaseForm((int)dgv.CurrentRow.Cells["ID"].Value, true)) form.ShowDialog();
                }
            };

            pnlActions.Controls.AddRange(new Control[] { btnDelete, btnEdit, btnPreview });

            this.Controls.Add(dgv);
            this.Controls.Add(top);
            this.Controls.Add(pnlActions);

            LanguageHelper.ApplyLanguage(this);
        }

        private Button CreateActionButton(string text, Color color, int width)
        {
            Button btn = new Button { Text = text, Dock = DockStyle.Right, Width = width, BackColor = color, ForeColor = Color.White, FlatStyle = FlatStyle.Flat, Font = new Font("Segoe UI", 9, FontStyle.Bold), Margin = new Padding(5) };
            btn.FlatAppearance.BorderSize = 0;
            return btn;
        }

        private async void LoadData() {
            var list = (await _repo.GetPurchaseInvoicesAsync()).ToList();
            dgv.DataSource = list;
        }
        private void InitializeComponent() { }
    }
}
