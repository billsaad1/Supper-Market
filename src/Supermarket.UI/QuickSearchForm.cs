using System;
using System.Drawing;
using System.Windows.Forms;

namespace Supermarket.UI
{
    public partial class QuickSearchForm : Form
    {
        public string SelectedBarcode { get; private set; }

        public QuickSearchForm()
        {
            InitializeComponent();
            SetupUI();
        }

        private void SetupUI()
        {
            this.Text = "Quick Search / بحث سريع";
            this.Size = new Size(600, 400);
            this.StartPosition = FormStartPosition.CenterParent;

            TextBox txtSearch = new TextBox { Dock = DockStyle.Top, Font = new Font("Arial", 14), PlaceholderText = "Search by name or code / ابحث بالاسم أو الكود" };
            DataGridView dgv = new DataGridView { Dock = DockStyle.Fill, SelectionMode = DataGridViewSelectionMode.FullRowSelect, ReadOnly = true };
            dgv.Columns.Add("Code", "Code / الكود");
            dgv.Columns.Add("Name", "Name / الاسم");
            dgv.Columns.Add("Price", "Price / السعر");

            dgv.DoubleClick += (s, e) => {
                if (dgv.SelectedRows.Count > 0) {
                    SelectedBarcode = dgv.SelectedRows[0].Cells[0].Value.ToString();
                    this.DialogResult = DialogResult.OK;
                    this.Close();
                }
            };

            this.Controls.Add(dgv);
            this.Controls.Add(txtSearch);

            LanguageHelper.ApplyLanguage(this);
        }

        private void InitializeComponent() { }
    }
}
