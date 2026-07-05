using Supermarket.UI.Helpers;
using System;
using System.Drawing;
using System.Windows.Forms;
using System.Collections.Generic;
using Supermarket.DAL;

namespace Supermarket.UI.Views
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
}
