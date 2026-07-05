using System;
using System.Drawing;
using System.Windows.Forms;
using System.Collections.Generic;
using Supermarket.BLL;
using Supermarket.DAL;
using Dapper;

namespace Supermarket.UI
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
            this.Text = "Sales Returns / مرتجعات المبيعات";
            this.Size = new Size(1000, 700);

            Panel top = new Panel { Dock = DockStyle.Top, Height = 80, BackColor = Color.WhiteSmoke };
            top.Controls.Add(new Label { Text = "Invoice # / رقم الفاتورة", Location = new Point(20, 30), AutoSize = true });
            txtInvoiceNum = new TextBox { Location = new Point(180, 28), Width = 200, Font = new Font("Arial", 12) };
            Button btnSearch = new Button { Text = "Search / بحث", Location = new Point(400, 25), Width = 120, Height = 35, BackColor = Color.Teal, ForeColor = Color.White };
            btnSearch.Click += async (s, e) => {
                dgvItems.Rows.Clear();
                _currentSalesId = 0;
                string connStr = AppSettings.ConnectionString;
                using (var db = new Microsoft.Data.SqlClient.SqlConnection(connStr))
                {
                    var invoice = await db.QueryFirstOrDefaultAsync<dynamic>(
                        "SELECT SalesID FROM SalesInvoices WHERE InvoiceNumber = @inv", new { inv = txtInvoiceNum.Text });

                    if (invoice == null) {
                        MessageBox.Show("Invoice not found / الفاتورة غير موجودة");
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
                MessageBox.Show("Invoice Found and Items Loaded / تم العثور على الفاتورة وتحميل الأصناف");
            };

            top.Controls.AddRange(new Control[] { txtInvoiceNum, btnSearch });

            dgvItems = new DataGridView { Dock = DockStyle.Fill, AllowUserToAddRows = false, AutoGenerateColumns = false };
            dgvItems.Columns.Add(new DataGridViewTextBoxColumn { Name = "ItemID", Visible = false });
            dgvItems.Columns.Add(new DataGridViewTextBoxColumn { Name = "Name", HeaderText = "Item", ReadOnly = true });
            dgvItems.Columns.Add(new DataGridViewTextBoxColumn { Name = "Qty", HeaderText = "Original Qty", ReadOnly = true });
            dgvItems.Columns.Add(new DataGridViewTextBoxColumn { Name = "RetQty", HeaderText = "Return Qty" });
            dgvItems.Columns.Add(new DataGridViewTextBoxColumn { Name = "Price", HeaderText = "Price", ReadOnly = true });
            dgvItems.Columns.Add(new DataGridViewTextBoxColumn { Name = "Total", HeaderText = "Refund Total", ReadOnly = true });

            dgvItems.CellValueChanged += (s, e) => {
                if (e.ColumnIndex == dgvItems.Columns["RetQty"].Index) {
                    var row = dgvItems.Rows[e.RowIndex];
                    decimal qty = Convert.ToDecimal(row.Cells["RetQty"].Value);
                    decimal price = Convert.ToDecimal(row.Cells["Price"].Value);
                    row.Cells["Total"].Value = qty * price;
                }
            };

            Button btnSave = new Button { Text = "PROCESS RETURN / إتمام المرتجع", Dock = DockStyle.Bottom, Height = 60, BackColor = Color.Maroon, ForeColor = Color.White, Font = new Font("Arial", 16, FontStyle.Bold) };
            btnSave.Click += async (s, e) => {
                if (_currentSalesId == 0) {
                    MessageBox.Show("Please search for a valid invoice first / يرجى البحث عن فاتورة صحيحة أولاً");
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
                    MessageBox.Show("Return Processed Successfully! / تم إتمام المرتجع وتحديث المخزن والقيود");
                    this.Close();
                }
            };

            this.Controls.Add(dgvItems);
            this.Controls.Add(top);
            this.Controls.Add(btnSave);

            LanguageHelper.ApplyLanguage(this);
        }

        private void InitializeComponent() { }
    }
}
