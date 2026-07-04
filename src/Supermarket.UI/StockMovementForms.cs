using System;
using System.Drawing;
using System.Windows.Forms;

namespace Supermarket.UI
{
    public partial class AdjustmentForm : Form
    {
        public AdjustmentForm()
        {
            InitializeComponent();
            SetupUI();
        }

        private void SetupUI()
        {
            this.Text = "Stock Adjustment / تسوية مخزون";
            this.Size = new Size(600, 450);

            DataGridView dgv = new DataGridView { Dock = DockStyle.Fill };
            dgv.Columns.Add("Item", "Item / الصنف");
            dgv.Columns.Add("Current", "Current / الحالي");
            dgv.Columns.Add("New", "New / الجديد");

            Button btnSave = new Button { Text = "Save Adjustment / حفظ التسوية", Dock = DockStyle.Bottom, Height = 40, BackColor = Color.Orange };

            this.Controls.Add(dgv);
            this.Controls.Add(btnSave);

            LanguageHelper.ApplyLanguage(this);
        }

        private void InitializeComponent() { }
    }

    public partial class WasteForm : Form
    {
        public WasteForm()
        {
            InitializeComponent();
            SetupUI();
        }

        private void SetupUI()
        {
            this.Text = "Stock Waste / توالف المخزون";
            this.Size = new Size(600, 450);

            DataGridView dgv = new DataGridView { Dock = DockStyle.Fill };
            dgv.Columns.Add("Item", "Item / الصنف");
            dgv.Columns.Add("Qty", "Qty / الكمية");
            dgv.Columns.Add("Reason", "Reason / السبب");

            Button btnSave = new Button { Text = "Record Waste / تسجيل تالف", Dock = DockStyle.Bottom, Height = 40, BackColor = Color.Red, ForeColor = Color.White };

            this.Controls.Add(dgv);
            this.Controls.Add(btnSave);

            LanguageHelper.ApplyLanguage(this);
        }

        private void InitializeComponent() { }
    }
}
