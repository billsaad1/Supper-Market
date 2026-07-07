using Supermarket.UI.Helpers;
using System;
using System.Drawing;
using System.Windows.Forms;
using Supermarket.Models.Entities;
using Supermarket.DAL;
using System.Linq;

namespace Supermarket.UI.Views
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
            this.Size = new Size(1000, 700);
            this.BackColor = UITheme.ContentBg;
            bool isArabic = LanguageHelper.TranslationService.CurrentLanguage == Supermarket.BLL.Services.Language.Arabic;

            Panel pnlToolbar = new Panel { Dock = DockStyle.Top, Height = 60, BackColor = Color.White, Padding = new Padding(10) };
            btnAdd = new Button {
                Text = isArabic ? "إضافة صنف جديد" : "Add New Item",
                Dock = isArabic ? DockStyle.Left : DockStyle.Right,
                Width = 150,
                BackColor = UITheme.PrimaryColor,
                ForeColor = Color.White
            };
            UITheme.ApplyModernStyle(btnAdd);
            btnAdd.Click += (s, e) => {
                var details = new AdvancedItemForm();
                if (details.ShowDialog() == DialogResult.OK) LoadData();
            };

            TextBox txtSearch = new TextBox {
                Width = 250,
                Font = UITheme.MainFont,
                PlaceholderText = isArabic ? "بحث..." : "Search...",
                Location = new Point(10, 15)
            };
            pnlToolbar.Controls.Add(txtSearch);
            pnlToolbar.Controls.Add(btnAdd);

            dgvItems = new DataGridView { Dock = DockStyle.Fill, AutoGenerateColumns = true };
            UITheme.ApplyModernStyle(dgvItems);

            this.Controls.Add(dgvItems);
            this.Controls.Add(pnlToolbar);

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
