using Supermarket.UI.Helpers;
using System;
using System.Drawing;
using System.Windows.Forms;
using Dapper;
using System.Linq;

namespace Supermarket.UI.Views
{
    public partial class UsersForm : Form
    {
        public UsersForm()
        {
            InitializeComponent();
            SetupUI();
        }

        private DataGridView dgv;

        private void SetupUI()
        {
            this.Text = "Manage Users / إدارة المستخدمين";
            this.Size = new Size(700, 500);

            dgv = new DataGridView { Dock = DockStyle.Fill, AutoGenerateColumns = true, ReadOnly = true };

            Button btnAdd = new Button { Text = "Add User / إضافة مستخدم", Dock = DockStyle.Bottom, Height = 40, BackColor = Color.DarkBlue, ForeColor = Color.White };
            btnAdd.Click += (s, e) => {
                MessageBox.Show("Add User functionality / إضافة مستخدم");
            };

            this.Controls.Add(dgv);
            this.Controls.Add(btnAdd);

            LoadUsers();
            LanguageHelper.ApplyLanguage(this);
        }

        private async void LoadUsers()
        {
            try
            {
                using (var db = new Microsoft.Data.SqlClient.SqlConnection(AppSettings.ConnectionString))
                {
                    var data = await db.QueryAsync<dynamic>("SELECT UserID, Username, FullName, Role, IsActive FROM Users");
                    dgv.DataSource = data.ToList();
                }
            }
            catch { }
        }

        private void InitializeComponent() { }
    }
}
