using System;
using System.Drawing;
using System.Windows.Forms;

namespace Supermarket.UI
{
    public partial class UsersForm : Form
    {
        public UsersForm()
        {
            InitializeComponent();
            SetupUI();
        }

        private void SetupUI()
        {
            this.Text = "Manage Users / إدارة المستخدمين";
            this.Size = new Size(700, 500);

            DataGridView dgv = new DataGridView { Dock = DockStyle.Fill };
            dgv.Columns.Add("Username", "Username / اسم المستخدم");
            dgv.Columns.Add("FullName", "Full Name / الاسم الكامل");
            dgv.Columns.Add("Role", "Role / الصلاحية");

            Button btnAdd = new Button { Text = "Add User / إضافة مستخدم", Dock = DockStyle.Bottom, Height = 40, BackColor = Color.DarkBlue, ForeColor = Color.White };

            this.Controls.Add(dgv);
            this.Controls.Add(btnAdd);

            LanguageHelper.ApplyLanguage(this);
        }

        private void InitializeComponent() { }
    }
}
