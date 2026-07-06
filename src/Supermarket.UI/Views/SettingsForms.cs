using Supermarket.UI.Helpers;
using System;
using System.Drawing;
using System.Windows.Forms;
using System.Collections.Generic;
using Supermarket.DAL;
using Supermarket.UI.Views.SettingsEditForms;

namespace Supermarket.UI.Views
{
    public partial class CategoriesForm : Form
    {
        private DataGridView dgv;
        private SettingsRepository _repo;

        public CategoriesForm()
        {
            InitializeComponent();
            _repo = new SettingsRepository(AppSettings.ConnectionString);
            SetupUI();
            LoadData();
        }

        private void SetupUI()
        {
            this.Text = "Categories / المجموعات";
            this.Size = new Size(600, 500);

            dgv = new DataGridView { Dock = DockStyle.Fill, AutoGenerateColumns = true, ReadOnly = true };

            Button btnAdd = new Button { Text = "Add Category / إضافة مجموعة", Dock = DockStyle.Bottom, Height = 40, BackColor = Color.Teal, ForeColor = Color.White };
            btnAdd.Click += (s, e) => {
                using (var form = new CategoryEditForm())
                {
                    if (form.ShowDialog() == DialogResult.OK) LoadData();
                }
            };

            this.Controls.Add(dgv);
            this.Controls.Add(btnAdd);
            LanguageHelper.ApplyLanguage(this);
        }

        private async void LoadData()
        {
            dgv.DataSource = (await _repo.GetAllCategoriesAsync()).ToList();
        }

        private void InitializeComponent() { }
    }

    public partial class StoresForm : Form
    {
        private DataGridView dgv;
        private SettingsRepository _repo;

        public StoresForm()
        {
            InitializeComponent();
            _repo = new SettingsRepository(AppSettings.ConnectionString);
            SetupUI();
            LoadData();
        }

        private void SetupUI()
        {
            this.Text = "Stores / المخازن";
            this.Size = new Size(600, 500);

            dgv = new DataGridView { Dock = DockStyle.Fill, AutoGenerateColumns = true, ReadOnly = true };

            Button btnAdd = new Button { Text = "Add Store / إضافة مخزن", Dock = DockStyle.Bottom, Height = 40, BackColor = Color.Navy, ForeColor = Color.White };
            btnAdd.Click += (s, e) => {
                using (var form = new StoreEditForm())
                {
                    if (form.ShowDialog() == DialogResult.OK) LoadData();
                }
            };

            this.Controls.Add(dgv);
            this.Controls.Add(btnAdd);
            LanguageHelper.ApplyLanguage(this);
        }

        private async void LoadData()
        {
            dgv.DataSource = (await _repo.GetAllStoresAsync()).ToList();
        }

        private void InitializeComponent() { }
    }
}
