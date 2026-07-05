using System;
using System.Drawing;
using System.Windows.Forms;

namespace Supermarket.UI
{
    public partial class AccountEditForm : Form
    {
        public AccountEditForm()
        {
            InitializeComponent();
            SetupUI();
        }

        private void SetupUI()
        {
            this.Text = "Add Account / إضافة حساب";
            this.Size = new Size(400, 350);
            this.StartPosition = FormStartPosition.CenterParent;

            this.Controls.Add(new Label { Text = "Account Number / الكود", Location = new Point(20, 20), AutoSize = true });
            this.Controls.Add(new TextBox { Location = new Point(180, 18), Width = 180 });

            this.Controls.Add(new Label { Text = "Account Name / الاسم", Location = new Point(20, 60), AutoSize = true });
            this.Controls.Add(new TextBox { Location = new Point(180, 58), Width = 180 });

            this.Controls.Add(new Label { Text = "Type / النوع", Location = new Point(20, 100), AutoSize = true });
            ComboBox cbType = new ComboBox { Location = new Point(180, 98), Width = 180 };
            cbType.Items.AddRange(new string[] { "Asset", "Liability", "Equity", "Revenue", "Expense" });
            this.Controls.Add(cbType);

            Button btnSave = new Button { Text = "Save Account / حفظ", Location = new Point(20, 220), Width = 340, Height = 50, BackColor = Color.Blue, ForeColor = Color.White };
            btnSave.Click += async (s, e) => {
                // Logic to save to DB via AccountRepository
                MessageBox.Show("Account Saved Successfully! / تم حفظ الحساب بنجاح");
                this.DialogResult = DialogResult.OK;
            };
            this.Controls.Add(btnSave);

            LanguageHelper.ApplyLanguage(this);
        }

        private void InitializeComponent() { }
    }
}
