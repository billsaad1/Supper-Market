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
            this.Text = "Purchase Invoices / فواتير المشتريات";
            this.Size = new Size(1100, 700);

            bool isArabic = LanguageHelper.TranslationService.CurrentLanguage == Supermarket.BLL.Services.Language.Arabic;
            Panel top = new Panel { Dock = DockStyle.Top, Height = 60, BackColor = Color.FromArgb(45, 45, 48) };

            Button btnNew = new Button { Text = isArabic ? "+ فاتورة جديدة" : "+ NEW INVOICE", Dock = DockStyle.Left, Width = 200, BackColor = Color.FromArgb(40, 167, 69), ForeColor = Color.White, Font = new Font("Segoe UI", 11, FontStyle.Bold), FlatStyle = FlatStyle.Flat };
            btnNew.Click += (s, e) => {
                using (var form = new PurchaseForm())
                {
                    if (form.ShowDialog() == DialogResult.OK) LoadData();
                }
            };

            TextBox txtSearch = new TextBox {
                Width = 250,
                Height = 35,
                Location = new Point(220, 15),
                Font = new Font("Segoe UI", 12),
                PlaceholderText = isArabic ? "بحث برقم الفاتورة..." : "Search Inv #..."
            };
            txtSearch.TextChanged += async (s, e) => {
                var data = await _repo.GetPurchaseInvoicesAsync();
                if (!string.IsNullOrEmpty(txtSearch.Text))
                {
                    string q = txtSearch.Text.ToLower();
                    data = data.Where(p =>
                        p.InvoiceNumber.ToLower().Contains(q) ||
                        p.SupplierName.ToLower().Contains(q)
                    );
                }
                dgv.DataSource = data.ToList();
            };

            top.Controls.Add(txtSearch);
            top.Controls.Add(btnNew);

            dgv = new DataGridView {
                Dock = DockStyle.Fill,
                AllowUserToAddRows = false,
                AutoGenerateColumns = false,
                SelectionMode = DataGridViewSelectionMode.FullRowSelect,
                BackgroundColor = Color.White,
                Font = new Font("Segoe UI", 10)
            };
            dgv.Columns.Add(new DataGridViewTextBoxColumn { Name = "ID", DataPropertyName = "PurchaseID", HeaderText = "ID", Width = 60 });
            dgv.Columns.Add(new DataGridViewTextBoxColumn { Name = "InvNum", DataPropertyName = "InvoiceNumber", HeaderText = "Invoice #", Width = 150 });
            dgv.Columns.Add(new DataGridViewTextBoxColumn { Name = "Supplier", DataPropertyName = "SupplierName", HeaderText = "Supplier", Width = 200 });
            dgv.Columns.Add(new DataGridViewTextBoxColumn { Name = "Store", DataPropertyName = "StoreName", HeaderText = "Store", Width = 150 });
            dgv.Columns.Add(new DataGridViewTextBoxColumn { Name = "Date", DataPropertyName = "InvoiceDate", HeaderText = "Date", Width = 150 });
            dgv.Columns.Add(new DataGridViewTextBoxColumn { Name = "Total", DataPropertyName = "TotalAmount", HeaderText = "Total", Width = 120 });

            Panel pnlActions = new Panel { Dock = DockStyle.Bottom, Height = 60, BackColor = Color.WhiteSmoke };

            Button btnPreview = CreateActionButton("Preview / معاينة", Color.FromArgb(0, 122, 204), 150);
            btnPreview.Click += async (s, e) => {
                if (dgv.SelectedRows.Count > 0) {
                    int id = (int)dgv.SelectedRows[0].Cells["ID"].Value;
                    await PreviewInvoice(id);
                }
            };

            Button btnEdit = CreateActionButton("Edit / تعديل", Color.FromArgb(255, 193, 7), 150);
            btnEdit.Click += async (s, e) => {
                if (dgv.SelectedRows.Count > 0) {
                    int id = (int)dgv.SelectedRows[0].Cells["ID"].Value;
                    using (var form = new PurchaseForm(id, false)) {
                        if (form.ShowDialog() == DialogResult.OK) LoadData();
                    }
                }
            };

            Button btnDelete = CreateActionButton("Delete / حذف", Color.FromArgb(220, 53, 69), 150);
            btnDelete.Click += async (s, e) => {
                if (dgv.SelectedRows.Count > 0) {
                    if (MessageBox.Show("Are you sure you want to delete this invoice and reverse stock?", "Delete", MessageBoxButtons.YesNo) == DialogResult.Yes) {
                        int id = (int)dgv.SelectedRows[0].Cells["ID"].Value;
                        await _repo.DeletePurchaseInvoiceAsync(id);
                        LoadData();
                    }
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
            Button btn = new Button { Text = text, Dock = DockStyle.Right, Width = width, BackColor = color, ForeColor = Color.White, FlatStyle = FlatStyle.Flat, Font = new Font("Segoe UI", 10, FontStyle.Bold), Margin = new Padding(5) };
            btn.FlatAppearance.BorderSize = 0;
            return btn;
        }

        private async void LoadData()
        {
            var data = await _repo.GetPurchaseInvoicesAsync();
            dgv.DataSource = data.ToList();
        }

        private async System.Threading.Tasks.Task PreviewInvoice(int id)
        {
            using (var form = new PurchaseForm(id, true))
            {
                form.ShowDialog();
            }
        }

        private void InitializeComponent() { }
    }
}
