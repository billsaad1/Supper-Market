using Supermarket.UI.Helpers;
using System;
using System.Drawing;
using System.Windows.Forms;

namespace Supermarket.UI.Views.SettingsEditForms
{
    using Supermarket.DAL;
    using Supermarket.Models.Entities;

    public partial class CategoryEditForm : Form
    {
        private MasterDataRepository _repo;
        private Category _category;
        private TextBox txtName, txtDesc;

        public CategoryEditForm(Category category = null)
        {
            InitializeComponent();
            _category = category ?? new Category();
            _repo = new MasterDataRepository(AppSettings.ConnectionString);
            SetupUI();
        }

        private void SetupUI()
        {
            this.Text = "Category / المجموعة";
            this.Size = new Size(400, 250);
            this.StartPosition = FormStartPosition.CenterParent;

            this.Controls.Add(new Label { Text = "Name / الاسم", Location = new Point(20, 30), AutoSize = true });
            txtName = new TextBox { Location = new Point(150, 28), Width = 200, Text = _category.CategoryName };
            this.Controls.Add(txtName);

            this.Controls.Add(new Label { Text = "Description / الوصف", Location = new Point(20, 70), AutoSize = true });
            txtDesc = new TextBox { Location = new Point(150, 68), Width = 200, Multiline = true, Height = 60, Text = _category.Description };
            this.Controls.Add(txtDesc);

            Button btnSave = new Button { Text = "Save / حفظ", Location = new Point(20, 150), Width = 330, Height = 40, BackColor = Color.Green, ForeColor = Color.White };
            btnSave.Click += async (s, e) => {
                _category.CategoryName = txtName.Text;
                _category.Description = txtDesc.Text;
                await _repo.UpsertCategoryAsync(_category);
                this.DialogResult = DialogResult.OK;
            };
            this.Controls.Add(btnSave);

            LanguageHelper.ApplyLanguage(this);
        }

        private void InitializeComponent() { }
    }

    public partial class StoreEditForm : Form
    {
        private MasterDataRepository _repo;
        private Store _store;
        private TextBox txtName, txtLoc;
        private CheckBox chkMain;

        public StoreEditForm(Store store = null)
        {
            InitializeComponent();
            _store = store ?? new Store();
            _repo = new MasterDataRepository(AppSettings.ConnectionString);
            SetupUI();
        }

        private void SetupUI()
        {
            this.Text = "Store / المخزن";
            this.Size = new Size(400, 300);
            this.StartPosition = FormStartPosition.CenterParent;

            this.Controls.Add(new Label { Text = "Store Name / الاسم", Location = new Point(20, 30), AutoSize = true });
            txtName = new TextBox { Location = new Point(150, 28), Width = 200, Text = _store.StoreName };
            this.Controls.Add(txtName);

            this.Controls.Add(new Label { Text = "Location / الموقع", Location = new Point(20, 70), AutoSize = true });
            txtLoc = new TextBox { Location = new Point(150, 68), Width = 200, Text = _store.Location };
            this.Controls.Add(txtLoc);

            chkMain = new CheckBox { Text = "Main Store / مخزن رئيسي", Location = new Point(150, 110), AutoSize = true, Checked = _store.IsMainStore };
            this.Controls.Add(chkMain);

            Button btnSave = new Button { Text = "Save / حفظ", Location = new Point(20, 180), Width = 330, Height = 40, BackColor = Color.Green, ForeColor = Color.White };
            btnSave.Click += async (s, e) => {
                _store.StoreName = txtName.Text;
                _store.Location = txtLoc.Text;
                _store.IsMainStore = chkMain.Checked;
                await _repo.UpsertStoreAsync(_store);
                this.DialogResult = DialogResult.OK;
            };
            this.Controls.Add(btnSave);

            LanguageHelper.ApplyLanguage(this);
        }

        private void InitializeComponent() { }
    }
}
