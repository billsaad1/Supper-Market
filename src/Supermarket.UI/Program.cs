using Supermarket.UI.Views;
using Supermarket.UI.Helpers;
using System;
using System.Threading.Tasks;
using System.Windows.Forms;
using Supermarket.BLL.Services;

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

            // Auto-Initialize Database
            try
            {
                string scriptPath = System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "database_schema.sql");
                if (!System.IO.File.Exists(scriptPath))
                {
                    // For development environment, check relative paths
                    scriptPath = System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "..", "..", "..", "..", "..", "database_schema.sql");
                }

                if (System.IO.File.Exists(scriptPath))
                {
                    string script = System.IO.File.ReadAllText(scriptPath);
                    Supermarket.DAL.DatabaseInitializer.InitializeDatabaseAsync(AppSettings.ConnectionString, script).GetAwaiter().GetResult();
                }
            }
            catch (Exception ex)
            {
                // If auto-init fails, we'll let the user handle it via settings or it will fail at login
                Console.WriteLine("Database Auto-Init Failed: " + ex.Message);
            }

            // Initialize Localization
            LanguageHelper.TranslationService = new TranslationService(AppSettings.ConnectionString);
            try
            {
                // Try to load translations, but don't crash if DB is not reachable yet
                LanguageHelper.TranslationService.LoadResourcesAsync().GetAwaiter().GetResult();

                // Default is Arabic as per BLL/Services/TranslationService.cs
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
