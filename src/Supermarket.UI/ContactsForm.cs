using System;
using System.Drawing;
using System.Windows.Forms;
using System.Collections.Generic;
using Supermarket.DAL;

namespace Supermarket.UI
{
    public partial class ContactsForm : Form
    {
        private ContactRepository _contactRepo;

        public ContactsForm()
        {
            InitializeComponent();
            _contactRepo = new ContactRepository(AppSettings.ConnectionString);
            SetupUI();
        }

        private void SetupUI()
        {
            this.Text = "Manage Contacts / إدارة الموردين والعملاء";
            this.Size = new Size(900, 600);

            TabControl tabs = new TabControl { Dock = DockStyle.Fill };

            tabs.TabPages.Add(CreateContactTab("Suppliers / الموردون", "Supplier"));
            tabs.TabPages.Add(CreateContactTab("Customers / العملاء", "Customer"));

            this.Controls.Add(tabs);
            LanguageHelper.ApplyLanguage(this);
        }

        private TabPage CreateContactTab(string title, string type)
        {
            TabPage tp = new TabPage(title);
            Panel pnl = new Panel { Dock = DockStyle.Fill, Padding = new Padding(10) };

            Panel controls = new Panel { Dock = DockStyle.Top, Height = 60 };
            Button btnAdd = new Button { Text = "Add / إضافة", Location = new Point(10, 10), Width = 100, Height = 35, BackColor = Color.Teal, ForeColor = Color.White };
            controls.Controls.Add(btnAdd);

            DataGridView dgv = new DataGridView { Dock = DockStyle.Fill, BackgroundColor = Color.White };

            pnl.Controls.Add(dgv);
            pnl.Controls.Add(controls);
            tp.Controls.Add(pnl);

            return tp;
        }

        private void InitializeComponent() { }
    }
}
