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
        private Label lblUsername;
        private Label lblPassword;

        public LoginForm()
        {
            string connString = AppSettings.ConnectionString;
            _authService = new AuthService(connString);
            SetupUI();
        }

        private void SetupUI()
        {
            this.Text = "Login / تسجيل الدخول";
            this.Size = new Size(400, 250);
            this.StartPosition = FormStartPosition.CenterScreen;
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;

            lblUsername = new Label { Text = "Username / اسم المستخدم", Location = new Point(30, 30), Size = new Size(340, 20), Tag = "lblUsername" };
            txtUsername = new TextBox { Location = new Point(30, 55), Width = 320 };

            lblPassword = new Label { Text = "Password / كلمة المرور", Location = new Point(30, 90), Size = new Size(340, 20), Tag = "lblPassword" };
            txtPassword = new TextBox { Location = new Point(30, 115), Width = 320, PasswordChar = '*' };

            btnLogin = new Button { Text = "Login / دخول", Location = new Point(30, 160), Width = 320, Height = 40, Tag = "btnLogin" };
            btnLogin.Click += BtnLogin_Click;

            this.Controls.AddRange(new Control[] { lblUsername, txtUsername, lblPassword, txtPassword, btnLogin });

            // Apply localization
            LanguageHelper.ApplyLanguage(this);
        }

        private void InitializeComponent() { }

        private async void BtnLogin_Click(object sender, EventArgs e)
        {
            btnLogin.Enabled = false;
            var user = await _authService.LoginAsync(txtUsername.Text, txtPassword.Text);
            if (user != null)
            {
                MessageBox.Show($"Welcome / أهلاً بك {user.FullName}");
                this.DialogResult = DialogResult.OK;
                this.Tag = user.Role;
                this.Close();
            }
            else
            {
                MessageBox.Show("Invalid username or password / اسم المستخدم أو كلمة المرور غير صحيحة");
            }
            btnLogin.Enabled = true;
        }
    }
}
