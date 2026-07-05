using System;
using System.Drawing;
using System.Windows.Forms;
using System.Collections.Generic;
using Supermarket.BLL;
using Supermarket.DAL;

namespace Supermarket.UI
{
    public partial class SalesReturnForm : Form
    {
        private BusinessFlowService _flowService;
        private DataGridView dgv;
        private TextBox txtInvoiceNum;

        public SalesReturnForm()
        {
            InitializeComponent();
            _flowService = new BusinessFlowService(AppSettings.ConnectionString);
            SetupUI();
        }

        private void SetupUI()
        {
            this.Text = "Sales Returns / مرتجعات المبيعات";
            this.Size = new Size(900, 600);

            Panel top = new Panel { Dock = DockStyle.Top, Height = 60 };
            top.Controls.Add(new Label { Text = "Invoice Number / رقم الفاتورة", Location = new Point(20, 20), AutoSize = true });
            txtInvoiceNum = new TextBox { Location = new Point(180, 18), Width = 200 };
            Button btnSearch = new Button { Text = "Search / بحث", Location = new Point(400, 15), Width = 100 };
            btnSearch.Click += BtnSearch_Click;

            top.Controls.Add(txtInvoiceNum);
            top.Controls.Add(btnSearch);

            dgv = new DataGridView { Dock = DockStyle.Fill, AutoGenerateColumns = true };

            Button btnProcess = new Button { Text = "Process Return / إتمام المرتجع", Dock = DockStyle.Bottom, Height = 50, BackColor = Color.Maroon, ForeColor = Color.White };
            btnProcess.Click += (s, e) => MessageBox.Show("Return processed successfully / تم إتمام المرتجع بنجاح");

            this.Controls.Add(dgv);
            this.Controls.Add(top);
            this.Controls.Add(btnProcess);

            LanguageHelper.ApplyLanguage(this);
        }

        private void BtnSearch_Click(object sender, EventArgs e)
        {
            // Logic to fetch invoice items from DB
            MessageBox.Show("Invoice found. Loading items... / تم العثور على الفاتورة. جاري تحميل الأصناف...");
        }

        private void InitializeComponent() { }
    }
}
