using Supermarket.UI.Helpers;
using System;
using System.Drawing;
using System.Windows.Forms;
using Supermarket.DAL;

namespace Supermarket.UI.Views
{
    public partial class WasteForm : Form
    {
        private StockOperationRepository _stockRepo;

        public WasteForm()
        {
            InitializeComponent();
            _stockRepo = new StockOperationRepository(AppSettings.ConnectionString);
            SetupUI();
        }

        private void SetupUI()
        {
            this.Text = "Waste Management / إدارة التوالف";
            this.Size = new Size(600, 500);

            Panel pnl = new Panel { Dock = DockStyle.Top, Height = 160, Padding = new Padding(10) };
            pnl.Controls.Add(new Label { Text = "Item Barcode / باركود الصنف", Location = new Point(20, 20), AutoSize = true });
            TextBox txtBarcode = new TextBox { Location = new Point(180, 18), Width = 200 };

            pnl.Controls.Add(new Label { Text = "Waste Qty / كمية التالف", Location = new Point(20, 60), AutoSize = true });
            TextBox txtQty = new TextBox { Location = new Point(180, 58), Width = 100 };

            pnl.Controls.Add(new Label { Text = "Reason / السبب", Location = new Point(20, 100), AutoSize = true });
            ComboBox cbReason = new ComboBox { Location = new Point(180, 98), Width = 200 };
            cbReason.Items.AddRange(new string[] { "Expired / منتهي الصلاحية", "Damaged / تالف", "Lost / مفقود" });
            cbReason.SelectedIndex = 0;
            pnl.Controls.Add(cbReason);

            Button btnSave = new Button { Text = "RECORD WASTE / تسجيل تالف", Location = new Point(400, 95), Width = 180, Height = 40, BackColor = Color.Red, ForeColor = Color.White };
            btnSave.Click += async (s, e) => {
                await _stockRepo.SaveAdjustmentAsync(1, 1, -decimal.Parse(txtQty.Text), "Waste: " + cbReason.Text, 1);
                MessageBox.Show("Waste Recorded & Stock Updated / تم تسجيل التالف وتحديث المخزن والقيود");
            };

            pnl.Controls.Add(txtBarcode);
            pnl.Controls.Add(txtQty);
            pnl.Controls.Add(btnSave);

            this.Controls.Add(pnl);
            this.Controls.Add(new DataGridView { Dock = DockStyle.Fill });

            LanguageHelper.ApplyLanguage(this);
        }

        private void InitializeComponent() { }
    }
}
