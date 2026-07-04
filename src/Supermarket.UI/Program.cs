using System;
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

            string connString = AppSettings.ConnectionString;

            // Initialize Localization
            LanguageHelper.TranslationService = new TranslationService(connString);

            LoginForm login = new LoginForm();
            if (login.ShowDialog() == DialogResult.OK)
            {
                Application.Run(new MainForm());
            }
        }
    }
}
