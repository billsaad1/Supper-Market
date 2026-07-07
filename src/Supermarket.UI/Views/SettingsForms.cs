using Supermarket.UI.Helpers;
using System;
using System.Drawing;
using System.Windows.Forms;
using System.Collections.Generic;
using Supermarket.DAL;
using Supermarket.UI.Views.SettingsEditForms;

namespace Supermarket.UI.Views
{
    using System.Linq;
    using Supermarket.Models.Entities;

    public partial class CategoriesForm : Form
    {
        private DataGridView dgv;
        private MasterDataRepository _repo;

        public CategoriesForm()
        {
            InitializeComponent();
            _repo = new MasterDataRepository(AppSettings.ConnectionString);
            SetupUI();
            LoadData();
        }

        private void SetupUI()
        {
            this.Text = "Categories / المجموعات";
            this.Size = new Size(700, 500);
            this.BackColor = UITheme.ContentBg;

            dgv = new DataGridView { Dock = DockStyle.Fill };
            UITheme.ApplyModernStyle(dgv);

            Panel pnlButtons = new Panel { Dock = DockStyle.Top, Height = 60, BackColor = Color.White, Padding = new Padding(10) };
            Button btnAdd = new Button { Text = "Add / إضافة", Width = 120, Dock = DockStyle.Left, BackColor = UITheme.SuccessColor, ForeColor = Color.White };
            btnAdd.Click += (s, e) => {
                using (var form = new CategoryEditForm()) {
                    if (form.ShowDialog() == DialogResult.OK) LoadData();
                }
            };

            Button btnEdit = new Button { Text = "Edit / تعديل", Width = 120, Dock = DockStyle.Left, Margin = new Padding(10, 0, 0, 0), BackColor = UITheme.WarningColor, ForeColor = Color.White };
            btnEdit.Click += (s, e) => {
                if (dgv.SelectedRows.Count > 0) {
                    var cat = dgv.SelectedRows[0].DataBoundItem as Category;
                    using (var form = new CategoryEditForm(cat)) {
                        if (form.ShowDialog() == DialogResult.OK) LoadData();
                    }
                }
            };

            UITheme.ApplyModernStyle(btnAdd);
            UITheme.ApplyModernStyle(btnEdit);
            pnlButtons.Controls.Add(btnEdit);
            pnlButtons.Controls.Add(btnAdd);

            this.Controls.Add(dgv);
            this.Controls.Add(pnlButtons);
            LanguageHelper.ApplyLanguage(this);
        }

        private async void LoadData()
        {
            var data = await _repo.GetAllCategoriesAsync();
            dgv.DataSource = data.ToList();
        }

        private void InitializeComponent() { }
    }

    public partial class StoresForm : Form
    {
        private DataGridView dgv;
        private MasterDataRepository _repo;

        public StoresForm()
        {
            InitializeComponent();
            _repo = new MasterDataRepository(AppSettings.ConnectionString);
            SetupUI();
            LoadData();
        }

        private void SetupUI()
        {
            this.Text = "Stores / المخازن";
            this.Size = new Size(700, 500);
            this.BackColor = UITheme.ContentBg;

            dgv = new DataGridView { Dock = DockStyle.Fill };
            UITheme.ApplyModernStyle(dgv);

            Panel pnlButtons = new Panel { Dock = DockStyle.Top, Height = 60, BackColor = Color.White, Padding = new Padding(10) };
            Button btnAdd = new Button { Text = "Add / إضافة", Width = 120, Dock = DockStyle.Left, BackColor = UITheme.PrimaryColor, ForeColor = Color.White };
            btnAdd.Click += (s, e) => {
                using (var form = new StoreEditForm()) {
                    if (form.ShowDialog() == DialogResult.OK) LoadData();
                }
            };

            Button btnEdit = new Button { Text = "Edit / تعديل", Width = 120, Dock = DockStyle.Left, BackColor = UITheme.WarningColor, ForeColor = Color.White };
            btnEdit.Click += (s, e) => {
                if (dgv.SelectedRows.Count > 0) {
                    var store = dgv.SelectedRows[0].DataBoundItem as Store;
                    using (var form = new StoreEditForm(store)) {
                        if (form.ShowDialog() == DialogResult.OK) LoadData();
                    }
                }
            };

            UITheme.ApplyModernStyle(btnAdd);
            UITheme.ApplyModernStyle(btnEdit);
            pnlButtons.Controls.Add(btnEdit);
            pnlButtons.Controls.Add(btnAdd);

            this.Controls.Add(dgv);
            this.Controls.Add(pnlButtons);
            LanguageHelper.ApplyLanguage(this);
        }

        private async void LoadData()
        {
            var data = await _repo.GetAllStoresAsync();
            dgv.DataSource = data.ToList();
        }

        private void InitializeComponent() { }
    }
}
