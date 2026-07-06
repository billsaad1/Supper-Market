using System;
using System.IO;

namespace Supermarket.UI
{
    public static class AppSettings
    {
        private static string _configPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "config.txt");
        public static string ConnectionString { get; set; } = "Server=(localdb)\\MSSQLLocalDB;Database=SupermarketDB;Trusted_Connection=True;TrustServerCertificate=True;";

        static AppSettings()
        {
            LoadSettings();
        }

        public static void LoadSettings()
        {
            if (File.Exists(_configPath))
            {
                try
                {
                    string savedConn = File.ReadAllText(_configPath);
                    if (!string.IsNullOrWhiteSpace(savedConn))
                    {
                        ConnectionString = savedConn;
                    }
                }
                catch { }
            }
        }

        public static void SaveSettings()
        {
            try
            {
                File.WriteAllText(_configPath, ConnectionString);
            }
            catch { }
        }
    }
}
