using System;
using System.Drawing;
using System.Windows.Forms;
using Supermarket.Models.Entities;
using Supermarket.DAL;

namespace Supermarket.UI
{
    public partial class ItemDetailsForm : Form
    {
        private Item _item;
        private SettingsRepository _repo;
        private TextBox txtBarcode, txtName, txtSalePrice, txtCostPrice;
        private ComboBox cbCategory;
        private Button btnSave;

        public ItemDetailsForm(Item item = null)
        {
            InitializeComponent();
            _item = item ?? new Item();
            _repo = new SettingsRepository(AppSettings.ConnectionString);
            SetupUI();
        }

        private void SetupUI()
        {
            this.Text = _item.ItemID == 0 ? "Add Item / إضافة صنف" : "Edit Item / تعديل صنف";
            this.Size = new Size(400, 450);
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.StartPosition = FormStartPosition.CenterParent;

            int labelX = 20, inputX = 150, currentY = 20, spacing = 40;

            this.Controls.Add(new Label { Text = "Barcode / باركود", Location = new Point(labelX, currentY), AutoSize = true });
            txtBarcode = new TextBox { Location = new Point(inputX, currentY), Width = 200, Text = _item.Barcode };
            this.Controls.Add(txtBarcode);
            currentY += spacing;

            this.Controls.Add(new Label { Text = "Name / الاسم", Location = new Point(labelX, currentY), AutoSize = true });
            txtName = new TextBox { Location = new Point(inputX, currentY), Width = 200, Text = _item.ItemName };
            this.Controls.Add(txtName);
            currentY += spacing;

            this.Controls.Add(new Label { Text = "Cost / التكلفة", Location = new Point(labelX, currentY), AutoSize = true });
            txtCostPrice = new TextBox { Location = new Point(inputX, currentY), Width = 200, Text = _item.CostPrice.ToString() };
            this.Controls.Add(txtCostPrice);
            currentY += spacing;

            this.Controls.Add(new Label { Text = "Sale / البيع", Location = new Point(labelX, currentY), AutoSize = true });
            txtSalePrice = new TextBox { Location = new Point(inputX, currentY), Width = 200, Text = _item.SalePrice.ToString() };
            this.Controls.Add(txtSalePrice);
            currentY += spacing;

            btnSave = new Button { Text = "Save / حفظ", Location = new Point(labelX, currentY + 20), Width = 330, Height = 40, BackColor = Color.Green, ForeColor = Color.White };
            btnSave.Click += BtnSave_Click;
            this.Controls.Add(btnSave);

            LanguageHelper.ApplyLanguage(this);
        }

        private async void BtnSave_Click(object sender, EventArgs e)
        {
            _item.Barcode = txtBarcode.Text;
            _item.ItemName = txtName.Text;
            _item.CostPrice = decimal.Parse(txtCostPrice.Text);
            _item.SalePrice = decimal.Parse(txtSalePrice.Text);
            _item.TaxRate = 15; // Default VAT

            await _repo.AddItemAsync(_item);
            this.DialogResult = DialogResult.OK;
            this.Close();
        }

        private void InitializeComponent() { }
    }
}
