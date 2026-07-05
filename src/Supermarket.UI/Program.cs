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

            // Initialize Localization (Synchronous wait for essential async startup data)
            LanguageHelper.TranslationService = new TranslationService(AppSettings.ConnectionString);
            try
            {
                LanguageHelper.TranslationService.LoadResourcesAsync().GetAwaiter().GetResult();
            }
            catch { /* Fallback to default if DB is not ready */ }

            using (LoginForm login = new LoginForm())
            {
                if (login.ShowDialog() == DialogResult.OK)
                {
                    string role = login.Tag?.ToString() ?? "Admin";
                    Application.Run(new MainForm(role));
                }
            }
        }
    }
}
