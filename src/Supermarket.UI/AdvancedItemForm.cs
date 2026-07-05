using System;
using System.Drawing;
using System.Windows.Forms;
using Supermarket.Models.Entities;
using Supermarket.DAL;
using Dapper;

namespace Supermarket.UI
{
    public partial class AdvancedItemForm : Form
    {
        private MasterDataRepository _repo;
        private Item _item;
        private TextBox txtBarcode, txtName, txtCost, txtSale, txtMin;
        private ComboBox cbCategory;
        private CheckBox chkActive;

        public AdvancedItemForm(Item item = null)
        {
            InitializeComponent();
            _item = item ?? new Item();
            _repo = new MasterDataRepository(AppSettings.ConnectionString);
            SetupUI();
        }

        private void SetupUI()
        {
            this.Text = "Item Details / بيانات الصنف";
            this.Size = new Size(700, 600);
            this.StartPosition = FormStartPosition.CenterParent;

            TabControl tabs = new TabControl { Dock = DockStyle.Fill };

            // Tab 1: Basic Data
            TabPage tabBasic = new TabPage("Basic Data / البيانات الأساسية");
            tabBasic.Controls.Add(new Label { Text = "Barcode / باركود", Location = new Point(20, 20), AutoSize = true });
            txtBarcode = new TextBox { Location = new Point(150, 18), Width = 200, Text = _item.Barcode };
            tabBasic.Controls.Add(txtBarcode);
            tabBasic.Controls.Add(new Label { Text = "Name / الاسم", Location = new Point(20, 60), AutoSize = true });
            txtName = new TextBox { Location = new Point(150, 58), Width = 300, Text = _item.ItemName };
            tabBasic.Controls.Add(txtName);

            tabBasic.Controls.Add(new Label { Text = "Category / المجموعة", Location = new Point(20, 100), AutoSize = true });
            cbCategory = new ComboBox { Location = new Point(150, 98), Width = 200 };
            LoadCategoriesForEdit();
            tabBasic.Controls.Add(cbCategory);

            // Tab 2: Prices
            TabPage tabPrices = new TabPage("Prices / الأسعار والضرائب");
            tabPrices.Controls.Add(new Label { Text = "Cost Price / التكلفة", Location = new Point(20, 20), AutoSize = true });
            txtCost = new TextBox { Location = new Point(150, 18), Width = 150, Text = _item.CostPrice.ToString() };
            tabPrices.Controls.Add(txtCost);
            tabPrices.Controls.Add(new Label { Text = "Sale Price / البيع", Location = new Point(20, 60), AutoSize = true });
            txtSale = new TextBox { Location = new Point(150, 58), Width = 150, Text = _item.SalePrice.ToString() };
            tabPrices.Controls.Add(txtSale);

            // Tab 3: Stock
            TabPage tabStock = new TabPage("Stock / المخزون");
            tabStock.Controls.Add(new Label { Text = "Min Level / الحد الأدنى", Location = new Point(20, 20), AutoSize = true });
            txtMin = new TextBox { Location = new Point(150, 18), Width = 150, Text = _item.MinimumStockLevel.ToString() };
            tabStock.Controls.Add(txtMin);

            // Tab 4: Status
            TabPage tabStatus = new TabPage("Status / الحالة");
            chkActive = new CheckBox { Text = "Active / نشط", Location = new Point(20, 20), Checked = true };
            tabStatus.Controls.Add(chkActive);

            tabs.TabPages.AddRange(new TabPage[] { tabBasic, tabPrices, tabStock, tabStatus });

            Button btnSave = new Button { Text = "Save Item / حفظ الصنف", Dock = DockStyle.Bottom, Height = 50, BackColor = Color.Green, ForeColor = Color.White, Font = new Font("Arial", 12, FontStyle.Bold) };
            btnSave.Click += async (s, e) => {
                _item.Barcode = txtBarcode.Text;
                _item.ItemName = txtName.Text;
                _item.CostPrice = decimal.Parse(txtCost.Text);
                _item.SalePrice = decimal.Parse(txtSale.Text);
                _item.MinimumStockLevel = decimal.Parse(txtMin.Text);
                await _repo.UpsertItemAsync(_item);
                MessageBox.Show("Item Saved! / تم حفظ الصنف بنجاح");
                this.DialogResult = DialogResult.OK;
            };

            this.Controls.Add(tabs);
            this.Controls.Add(btnSave);

            LanguageHelper.ApplyLanguage(this);
        }

        private async void LoadCategoriesForEdit()
        {
            string conn = AppSettings.ConnectionString;
            using (var db = new Microsoft.Data.SqlClient.SqlConnection(conn))
            {
                var cats = await db.QueryAsync<dynamic>("SELECT CategoryID, CategoryName FROM Categories");
                foreach (var cat in cats)
                {
                    cbCategory.Items.Add(new { ID = (int)cat.CategoryID, Name = (string)cat.CategoryName });
                }
                cbCategory.DisplayMember = "Name";
                cbCategory.ValueMember = "ID";
            }
        }

        private void InitializeComponent() { }
    }
}
