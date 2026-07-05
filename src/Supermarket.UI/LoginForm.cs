using System;
using System.Drawing;
using System.Windows.Forms;
using Supermarket.BLL;

namespace Supermarket.UI
{
    public partial class LoginForm : Form
    {
        private readonly AuthService _authService;
        private TextBox txtUsername;
        private TextBox txtPassword;
        private Button btnLogin;
        private Button btnSettings;
        private Label lblUsername;
        private Label lblPassword;
        private Label lblTitle;
        private Panel panelHeader;

        public LoginForm()
        {
            string connString = AppSettings.ConnectionString;
            _authService = new AuthService(connString);
            SetupUI();
        }

        private void SetupUI()
        {
            this.Text = "Supermarket System - Login";
            this.Size = new Size(450, 350);
            this.StartPosition = FormStartPosition.CenterScreen;
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.BackColor = Color.FromArgb(240, 240, 240);
            this.Font = new Font("Segoe UI", 10);

            panelHeader = new Panel
            {
                Dock = DockStyle.Top,
                Height = 60,
                BackColor = Color.FromArgb(45, 52, 71)
            };

            lblTitle = new Label
            {
                Text = "Supermarket System / نظام السوبر ماركت",
                ForeColor = Color.White,
                Font = new Font("Segoe UI", 14, FontStyle.Bold),
                Dock = DockStyle.Fill,
                TextAlign = ContentAlignment.MiddleCenter
            };
            panelHeader.Controls.Add(lblTitle);

            lblUsername = new Label
            {
                Text = "Username / اسم المستخدم",
                Location = new Point(50, 80),
                Size = new Size(350, 25),
                Tag = "lblUsername"
            };
            txtUsername = new TextBox
            {
                Location = new Point(50, 110),
                Width = 350,
                Height = 30
            };

            lblPassword = new Label
            {
                Text = "Password / كلمة المرور",
                Location = new Point(50, 150),
                Size = new Size(350, 25),
                Tag = "lblPassword"
            };
            txtPassword = new TextBox
            {
                Location = new Point(50, 180),
                Width = 350,
                Height = 30,
                PasswordChar = '*'
            };

            btnLogin = new Button
            {
                Text = "Login / دخول",
                Location = new Point(50, 230),
                Width = 350,
                Height = 45,
                BackColor = Color.FromArgb(0, 122, 204),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 12, FontStyle.Bold),
                Tag = "btnLogin"
            };
            btnLogin.FlatAppearance.BorderSize = 0;
            btnLogin.Click += BtnLogin_Click;

            btnSettings = new Button
            {
                Text = "⚙",
                Location = new Point(410, 310),
                Size = new Size(30, 30),
                FlatStyle = FlatStyle.Flat,
                ForeColor = Color.Gray
            };
            btnSettings.FlatAppearance.BorderSize = 0;
            btnSettings.Click += (s, e) => {
                using (var dbSettings = new DatabaseSettingsForm())
                {
                    if (dbSettings.ShowDialog() == DialogResult.OK)
                    {
                        Application.Restart();
                    }
                }
            };

            this.Controls.AddRange(new Control[] { panelHeader, lblUsername, txtUsername, lblPassword, txtPassword, btnLogin, btnSettings });

            // Apply localization
            LanguageHelper.ApplyLanguage(this);
        }

        private void InitializeComponent() { }

        private async void BtnLogin_Click(object sender, EventArgs e)
        {
            try
            {
                btnLogin.Enabled = false;
                var user = await _authService.LoginAsync(txtUsername.Text, txtPassword.Text);
                if (user != null)
                {
                    this.DialogResult = DialogResult.OK;
                    this.Tag = user.Role;
                    this.Close();
                }
                else
                {
                    MessageBox.Show("Invalid username or password / اسم المستخدم أو كلمة المرور غير صحيحة", "Login Failed", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Connection Error / خطأ في الاتصال:\n{ex.Message}\n\nPlease check database settings.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
            finally
            {
                btnLogin.Enabled = true;
            }
        }
    }
}
