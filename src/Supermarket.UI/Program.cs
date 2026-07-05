using System;
using System.Threading.Tasks;
using System.Windows.Forms;
using Supermarket.BLL;

namespace Supermarket.UI
{
    static class Program
    {
        [STAThread]
        static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);

            RunAppAsync();
            Application.Run(); // Keep the message loop running
        }

        static async void RunAppAsync()
        {
            // Initialize Localization
            LanguageHelper.TranslationService = new TranslationService(AppSettings.ConnectionString);
            try {
                await LanguageHelper.TranslationService.LoadResourcesAsync();
            } catch { }

            LoginForm login = new LoginForm();
            if (login.ShowDialog() == DialogResult.OK)
            {
                string role = login.Tag?.ToString() ?? "Admin";
                MainForm main = new MainForm(role);
                main.FormClosed += (s, e) => Application.Exit();
                main.Show();
            }
            else
            {
                Application.Exit();
            }
        }
    }
}
