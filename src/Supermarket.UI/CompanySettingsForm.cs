using System;
using System.Drawing;
using System.Windows.Forms;
using Supermarket.DAL;

namespace Supermarket.UI
{
    public partial class CompanySettingsForm : Form
    {
        public CompanySettingsForm()
        {
            InitializeComponent();
            SetupUI();
        }

        private void SetupUI()
        {
            this.Text = "Company Settings & Backup / إعدادات المنشأة والنسخ الاحتياطي";
            this.Size = new Size(600, 450);

            TabControl tabs = new TabControl { Dock = DockStyle.Fill };

            TabPage tabGeneral = new TabPage("General / عام");
            tabGeneral.Controls.Add(new Label { Text = "Company Name / اسم المنشأة", Location = new Point(20, 20), AutoSize = true });
            tabGeneral.Controls.Add(new TextBox { Location = new Point(200, 18), Width = 250 });
            tabGeneral.Controls.Add(new Label { Text = "Tax Number / الرقم الضريبي", Location = new Point(20, 60), AutoSize = true });
            tabGeneral.Controls.Add(new TextBox { Location = new Point(200, 58), Width = 250 });

            TabPage tabBackup = new TabPage("Backup / النسخ الاحتياطي");
            Button btnBackup = new Button { Text = "Backup Now / نسخ احتياطي الآن", Location = new Point(50, 50), Size = new Size(200, 50), BackColor = Color.Green, ForeColor = Color.White };
            btnBackup.Click += async (s, e) => {
                BackupRepository repo = new BackupRepository(AppSettings.ConnectionString);
                await repo.CreateBackupAsync("C:\\Backups\\SupermarketDB.bak");
                MessageBox.Show("Backup Created Successfully! / تم إنشاء نسخة احتياطية بنجاح");
            };

            Button btnRestore = new Button { Text = "Restore / استرجاع", Location = new Point(50, 120), Size = new Size(200, 50), BackColor = Color.Maroon, ForeColor = Color.White };
            btnRestore.Click += async (s, e) => {
                BackupRepository repo = new BackupRepository(AppSettings.ConnectionString);
                await repo.RestoreBackupAsync("C:\\Backups\\SupermarketDB.bak");
                MessageBox.Show("Database Restored Successfully! / تم استعادة قاعدة البيانات بنجاح");
            };
            tabBackup.Controls.AddRange(new Control[] { btnBackup, btnRestore });

            tabs.TabPages.AddRange(new TabPage[] { tabGeneral, tabBackup });
            this.Controls.Add(tabs);

            LanguageHelper.ApplyLanguage(this);
        }

        private void InitializeComponent() { }
    }
}
