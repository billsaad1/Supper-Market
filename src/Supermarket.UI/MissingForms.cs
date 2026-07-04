using System;
using System.Drawing;
using System.Windows.Forms;
using System.Collections.Generic;
using Supermarket.DAL;

namespace Supermarket.UI
{
    public partial class CategoriesForm : Form
    {
        public CategoriesForm() { InitializeComponent(); SetupUI(); }
        private void SetupUI() { this.Text = "Categories"; this.Size = new Size(600, 400); this.Controls.Add(new DataGridView { Dock = DockStyle.Fill }); LanguageHelper.ApplyLanguage(this); }
        private void InitializeComponent() { }
    }

    public partial class StoresForm : Form
    {
        public StoresForm() { InitializeComponent(); SetupUI(); }
        private void SetupUI() { this.Text = "Stores"; this.Size = new Size(600, 400); this.Controls.Add(new DataGridView { Dock = DockStyle.Fill }); LanguageHelper.ApplyLanguage(this); }
        private void InitializeComponent() { }
    }

    public partial class AccountsForm : Form
    {
        public AccountsForm() { InitializeComponent(); SetupUI(); }
        private void SetupUI() { this.Text = "Accounts"; this.Size = new Size(600, 400); this.Controls.Add(new TreeView { Dock = DockStyle.Fill }); LanguageHelper.ApplyLanguage(this); }
        private void InitializeComponent() { }
    }

    public partial class ContactsForm : Form
    {
        public ContactsForm() { InitializeComponent(); SetupUI(); }
        private void SetupUI() { this.Text = "Contacts"; this.Size = new Size(600, 400); this.Controls.Add(new DataGridView { Dock = DockStyle.Fill }); LanguageHelper.ApplyLanguage(this); }
        private void InitializeComponent() { }
    }

    public partial class ReportsForm : Form
    {
        public ReportsForm() { InitializeComponent(); SetupUI(); }
        private void SetupUI() { this.Text = "Reports"; this.Size = new Size(600, 400); this.Controls.Add(new DataGridView { Dock = DockStyle.Fill }); LanguageHelper.ApplyLanguage(this); }
        private void InitializeComponent() { }
    }

    public partial class PurchaseForm : Form
    {
        public PurchaseForm() { InitializeComponent(); SetupUI(); }
        private void SetupUI() { this.Text = "Purchases"; this.Size = new Size(800, 600); this.Controls.Add(new DataGridView { Dock = DockStyle.Fill }); LanguageHelper.ApplyLanguage(this); }
        private void InitializeComponent() { }
    }
}
