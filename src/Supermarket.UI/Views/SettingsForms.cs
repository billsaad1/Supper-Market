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
            this.Size = new Size(600, 500);

            dgv = new DataGridView { Dock = DockStyle.Fill, AutoGenerateColumns = true, ReadOnly = true, SelectionMode = DataGridViewSelectionMode.FullRowSelect };

            Panel pnlButtons = new Panel { Dock = DockStyle.Bottom, Height = 50 };
            Button btnAdd = new Button { Text = "Add / إضافة", Width = 100, Height = 40, Location = new Point(10, 5), BackColor = Color.Teal, ForeColor = Color.White };
            btnAdd.Click += (s, e) => {
                using (var form = new CategoryEditForm())
                {
                    if (form.ShowDialog() == DialogResult.OK) LoadData();
                }
            };

            Button btnEdit = new Button { Text = "Edit / تعديل", Width = 100, Height = 40, Location = new Point(120, 5), BackColor = Color.Orange, ForeColor = Color.White };
            btnEdit.Click += (s, e) => {
                if (dgv.SelectedRows.Count > 0)
                {
                    var cat = dgv.SelectedRows[0].DataBoundItem as Category;
                    using (var form = new CategoryEditForm(cat))
                    {
                        if (form.ShowDialog() == DialogResult.OK) LoadData();
                    }
                }
            };

            pnlButtons.Controls.Add(btnAdd);
            pnlButtons.Controls.Add(btnEdit);

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
            this.Size = new Size(600, 500);

            dgv = new DataGridView { Dock = DockStyle.Fill, AutoGenerateColumns = true, ReadOnly = true, SelectionMode = DataGridViewSelectionMode.FullRowSelect };

            Panel pnlButtons = new Panel { Dock = DockStyle.Bottom, Height = 50 };
            Button btnAdd = new Button { Text = "Add / إضافة", Width = 100, Height = 40, Location = new Point(10, 5), BackColor = Color.Navy, ForeColor = Color.White };
            btnAdd.Click += (s, e) => {
                using (var form = new StoreEditForm())
                {
                    if (form.ShowDialog() == DialogResult.OK) LoadData();
                }
            };

            Button btnEdit = new Button { Text = "Edit / تعديل", Width = 100, Height = 40, Location = new Point(120, 5), BackColor = Color.Orange, ForeColor = Color.White };
            btnEdit.Click += (s, e) => {
                if (dgv.SelectedRows.Count > 0)
                {
                    var store = dgv.SelectedRows[0].DataBoundItem as Store;
                    using (var form = new StoreEditForm(store))
                    {
                        if (form.ShowDialog() == DialogResult.OK) LoadData();
                    }
                }
            };

            pnlButtons.Controls.Add(btnAdd);
            pnlButtons.Controls.Add(btnEdit);

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
