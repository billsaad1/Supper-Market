using System;
using System.Drawing;
using System.Windows.Forms;
using Supermarket.BLL.Services;
using Supermarket.UI.Helpers;
using Supermarket.UI.ViewModels;

namespace Supermarket.UI.Views
{
    public partial class LoginForm : Form
    {
        private readonly LoginViewModel _viewModel;
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
            var authService = new AuthService(AppSettings.ConnectionString);
            _viewModel = new LoginViewModel(authService);
            SetupUI();
        }

        private void SetupUI()
        {
            this.Text = "Supermarket System - Login";
            this.Size = new Size(450, 400);
            this.StartPosition = FormStartPosition.CenterScreen;
            this.FormBorderStyle = FormBorderStyle.None;
            this.BackColor = Color.White;

            panelHeader = new Panel
            {
                Dock = DockStyle.Top,
                Height = 100,
                BackColor = UITheme.PrimaryColor
            };

            lblTitle = new Label
            {
                Text = "BCREATIVE\nSupermarket POS",
                ForeColor = Color.White,
                Font = new Font("Segoe UI", 16, FontStyle.Bold),
                Dock = DockStyle.Fill,
                TextAlign = ContentAlignment.MiddleCenter
            };
            panelHeader.Controls.Add(lblTitle);

            lblUsername = new Label { Text = "Username / اسم المستخدم", Location = new Point(50, 120), Size = new Size(350, 25), Font = UITheme.MainFont };
            txtUsername = new TextBox { Location = new Point(50, 145), Width = 350, Font = new Font("Segoe UI", 12) };
            txtUsername.TextChanged += (s, e) => _viewModel.Username = txtUsername.Text;

            lblPassword = new Label { Text = "Password / كلمة المرور", Location = new Point(50, 185), Size = new Size(350, 25), Font = UITheme.MainFont };
            txtPassword = new TextBox { Location = new Point(50, 210), Width = 350, Font = new Font("Segoe UI", 12), PasswordChar = '*' };
            txtPassword.TextChanged += (s, e) => _viewModel.Password = txtPassword.Text;

            btnLogin = new Button
            {
                Text = "LOGIN / دخول",
                Location = new Point(50, 270),
                Width = 350,
                Height = 50,
                BackColor = UITheme.PrimaryColor,
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 12, FontStyle.Bold)
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
                var user = await _viewModel.LoginAsync();
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
