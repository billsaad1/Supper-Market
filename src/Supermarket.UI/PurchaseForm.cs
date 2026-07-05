using System;
using System.Drawing;
using System.Windows.Forms;
using System.Collections.Generic;
using Supermarket.DAL;

namespace Supermarket.UI
{
    public partial class PurchaseForm : Form
    {
        private DataGridView dgvItems;
        private TextBox txtBarcode;
        private Label lblTotal;
        private ComboBox cbSupplier;

        public PurchaseForm()
        {
            InitializeComponent();
            SetupUI();
        }

        private void SetupUI()
        {
            this.Text = "Purchase Invoice / فاتورة مشتريات";
            this.Size = new Size(1000, 700);

            Panel topPanel = new Panel { Dock = DockStyle.Top, Height = 100 };

            topPanel.Controls.Add(new Label { Text = "Supplier / المورد", Location = new Point(20, 15), AutoSize = true });
            cbSupplier = new ComboBox { Location = new Point(150, 12), Width = 250 };
            topPanel.Controls.Add(cbSupplier);

            txtBarcode = new TextBox { Location = new Point(20, 50), Width = 400, Font = new Font("Arial", 14), PlaceholderText = "Scan Barcode / امسح الباركود" };
            topPanel.Controls.Add(txtBarcode);

            dgvItems = new DataGridView {
                Dock = DockStyle.Fill,
                AutoGenerateColumns = false,
                Font = new Font("Arial", 11)
            };
            dgvItems.Columns.Add("ItemName", "Item / الصنف");
            dgvItems.Columns.Add("Qty", "Qty / الكمية");
            dgvItems.Columns.Add("Price", "Price / السعر");
            dgvItems.Columns.Add("Tax", "Tax / الضريبة");
            dgvItems.Columns.Add("Total", "Total / الإجمالي");

            Panel bottomPanel = new Panel { Dock = DockStyle.Bottom, Height = 100, BackColor = Color.FromArgb(45, 45, 48) };
            lblTotal = new Label { Text = "Total: 0.00", ForeColor = Color.Yellow, Font = new Font("Arial", 20, FontStyle.Bold), Location = new Point(20, 30), AutoSize = true };
            Button btnSave = new Button { Text = "Confirm Invoice / تأكيد الفاتورة", Dock = DockStyle.Right, Width = 250, BackColor = Color.Blue, ForeColor = Color.White, Font = new Font("Arial", 16, FontStyle.Bold), FlatStyle = FlatStyle.Flat };

            bottomPanel.Controls.Add(lblTotal);
            bottomPanel.Controls.Add(btnSave);

            this.Controls.Add(dgvItems);
            this.Controls.Add(topPanel);
            this.Controls.Add(bottomPanel);

            LanguageHelper.ApplyLanguage(this);
        }

        private void InitializeComponent() { }
    }
}
