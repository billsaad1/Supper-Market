using Supermarket.UI.Helpers;
using System;
using System.Drawing;
using System.Windows.Forms;
using System.IO;

namespace Supermarket.UI.Views
{
    public partial class DatabaseSettingsForm : Form
    {
        private TextBox txtServer;
        private TextBox txtDatabase;
        private CheckBox chkWindowsAuth;
        private TextBox txtUser;
        private TextBox txtPassword;
        private Button btnSave;
        private Button btnTest;

        public DatabaseSettingsForm()
        {
            SetupUI();
            LoadCurrentSettings();
        }

        private void SetupUI()
        {
            this.Text = "Database Settings / إعدادات قاعدة البيانات";
            this.Size = new Size(400, 400);
            this.StartPosition = FormStartPosition.CenterParent;
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;

            int labelX = 20, inputX = 150, width = 200;

            var lblServer = new Label { Text = "Server:", Location = new Point(labelX, 30) };
            txtServer = new TextBox { Location = new Point(inputX, 27), Width = width };

            var lblDatabase = new Label { Text = "Database:", Location = new Point(labelX, 70) };
            txtDatabase = new TextBox { Location = new Point(inputX, 67), Width = width };

            chkWindowsAuth = new CheckBox { Text = "Windows Authentication", Location = new Point(inputX, 110), Width = width, Checked = true };
            chkWindowsAuth.CheckedChanged += (s, e) => {
                txtUser.Enabled = !chkWindowsAuth.Checked;
                txtPassword.Enabled = !chkWindowsAuth.Checked;
            };

            var lblUser = new Label { Text = "User ID:", Location = new Point(labelX, 150) };
            txtUser = new TextBox { Location = new Point(inputX, 147), Width = width, Enabled = false };

            var lblPassword = new Label { Text = "Password:", Location = new Point(labelX, 190) };
            txtPassword = new TextBox { Location = new Point(inputX, 187), Width = width, PasswordChar = '*', Enabled = false };

            btnTest = new Button { Text = "Test Connection", Location = new Point(inputX, 240), Width = width };
            btnTest.Click += BtnTest_Click;

            btnSave = new Button { Text = "Save & Restart", Location = new Point(inputX, 280), Width = width, Height = 40, BackColor = Color.LightGreen };
            btnSave.Click += BtnSave_Click;

            this.Controls.AddRange(new Control[] {
                lblServer, txtServer, lblDatabase, txtDatabase, chkWindowsAuth,
                lblUser, txtUser, lblPassword, txtPassword, btnTest, btnSave
            });
        }

        private void LoadCurrentSettings()
        {
            try
            {
                // Simple parsing of connection string for the UI
                string conn = AppSettings.ConnectionString;
                if (conn.Contains("Server=")) txtServer.Text = GetPart(conn, "Server");
                if (conn.Contains("Database=")) txtDatabase.Text = GetPart(conn, "Database");
                if (conn.Contains("Trusted_Connection=True")) chkWindowsAuth.Checked = true;
                else
                {
                    chkWindowsAuth.Checked = false;
                    txtUser.Text = GetPart(conn, "User Id");
                    txtPassword.Text = GetPart(conn, "Password");
                }
            }
            catch { }
        }

        private string GetPart(string conn, string key)
        {
            try
            {
                int start = conn.IndexOf(key + "=") + key.Length + 1;
                int end = conn.IndexOf(";", start);
                if (end == -1) end = conn.Length;
                return conn.Substring(start, end - start);
            }
            catch { return ""; }
        }

        private void BtnTest_Click(object sender, EventArgs e)
        {
            string conn = BuildConnectionString();
            try
            {
                using (var connection = new Microsoft.Data.SqlClient.SqlConnection(conn))
                {
                    connection.Open();
                    MessageBox.Show("Connection Successful! / تم الاتصال بنجاح", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Connection Failed / فشل الاتصال:\n{ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private string BuildConnectionString()
        {
            if (chkWindowsAuth.Checked)
                return $"Server={txtServer.Text};Database={txtDatabase.Text};Trusted_Connection=True;TrustServerCertificate=True;";
            else
                return $"Server={txtServer.Text};Database={txtDatabase.Text};User Id={txtUser.Text};Password={txtPassword.Text};TrustServerCertificate=True;";
        }

        private void BtnSave_Click(object sender, EventArgs e)
        {
            AppSettings.ConnectionString = BuildConnectionString();
            AppSettings.SaveSettings();
            this.DialogResult = DialogResult.OK;
            this.Close();
        }
    }
}
