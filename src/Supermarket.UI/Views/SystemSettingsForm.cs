using Supermarket.UI.Helpers;
using System;
using System.Drawing;
using System.Windows.Forms;

namespace Supermarket.UI.Views
{
    public partial class SystemSettingsForm : Form
    {
        public SystemSettingsForm()
        {
            InitializeComponent();
            SetupUI();
        }

        private void SetupUI()
        {
            this.Text = "System Management / إدارة النظام";
            this.Size = new Size(1100, 750);

            TabControl tabs = new TabControl { Dock = DockStyle.Fill, Font = new Font("Segoe UI", 10) };

            // Tab 1: Stores
            TabPage tabStores = new TabPage("Stores / المخازن");
            tabStores.Controls.Add(new StoresForm { TopLevel = false, Dock = DockStyle.Fill, FormBorderStyle = FormBorderStyle.None }.ShowWithReturn());

            // Tab 2: Chart of Accounts
            TabPage tabAccounts = new TabPage("Accounts / الحسابات");
            tabAccounts.Controls.Add(new AccountsForm { TopLevel = false, Dock = DockStyle.Fill, FormBorderStyle = FormBorderStyle.None }.ShowWithReturn());

            // Tab 3: Categories
            TabPage tabCategories = new TabPage("Categories / المجموعات");
            tabCategories.Controls.Add(new CategoriesForm { TopLevel = false, Dock = DockStyle.Fill, FormBorderStyle = FormBorderStyle.None }.ShowWithReturn());

            // Tab 4: Items
            TabPage tabItems = new TabPage("Items / الأصناف");
            tabItems.Controls.Add(new ItemsForm { TopLevel = false, Dock = DockStyle.Fill, FormBorderStyle = FormBorderStyle.None }.ShowWithReturn());

            tabs.TabPages.AddRange(new TabPage[] { tabStores, tabAccounts, tabCategories, tabItems });
            this.Controls.Add(tabs);

            LanguageHelper.ApplyLanguage(this);
        }

        private void InitializeComponent() { }
    }

    public static class FormExtensions
    {
        public static Control ShowWithReturn(this Form frm)
        {
            frm.Visible = true;
            return frm;
        }
    }
}
