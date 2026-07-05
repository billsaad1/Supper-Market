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

            // Load settings first
            AppSettings.LoadSettings();

            // Initialize Localization
            LanguageHelper.TranslationService = new TranslationService(AppSettings.ConnectionString);
            try
            {
                // Try to load translations, but don't crash if DB is not reachable yet
                LanguageHelper.TranslationService.LoadResourcesAsync().GetAwaiter().GetResult();
            }
            catch
            {
                // Fallback will happen naturally in TranslationService if _resources is null
            }

            bool retryLogin = true;
            while (retryLogin)
            {
                using (LoginForm login = new LoginForm())
                {
                    var result = login.ShowDialog();
                    if (result == DialogResult.OK)
                    {
                        string role = login.Tag?.ToString() ?? "Admin";
                        Application.Run(new MainForm(role));
                        retryLogin = false;
                    }
                    else
                    {
                        retryLogin = false; // User cancelled
                    }
                }
            }
        }
    }
}
