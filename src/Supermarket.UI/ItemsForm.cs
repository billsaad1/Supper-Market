using System;
using System.Drawing;
using System.Windows.Forms;
using Supermarket.Models.Entities;
using Supermarket.DAL;
using System.Linq;

namespace Supermarket.UI
{
    public partial class ItemsForm : Form
    {
        private DataGridView dgvItems;
        private Button btnAdd;
        private SettingsRepository _repo;

        public ItemsForm()
        {
            InitializeComponent();
            string connString = AppSettings.ConnectionString;
            _repo = new SettingsRepository(connString);
            SetupUI();
            LoadData();
        }

        private void SetupUI()
        {
            this.Text = "Manage Items / إدارة الأصناف";
            this.Size = new Size(800, 600);

            dgvItems = new DataGridView { Dock = DockStyle.Fill, AutoGenerateColumns = true, SelectionMode = DataGridViewSelectionMode.FullRowSelect, ReadOnly = true };
            btnAdd = new Button { Text = "Add Item / إضافة صنف", Dock = DockStyle.Bottom, Height = 40 };
            btnAdd.Click += (s, e) => {
                var details = new AdvancedItemForm();
                if (details.ShowDialog() == DialogResult.OK) LoadData();
            };

            this.Controls.Add(dgvItems);
            this.Controls.Add(btnAdd);

            LanguageHelper.ApplyLanguage(this);
        }

        private async void LoadData()
        {
            var items = await _repo.GetAllItemsAsync();
            dgvItems.DataSource = items.ToList();
        }

        private void InitializeComponent() { }
    }
}
