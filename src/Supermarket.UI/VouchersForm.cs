using System;
using System.Drawing;
using System.Windows.Forms;

namespace Supermarket.UI
{
    public partial class VouchersForm : Form
    {
        public VouchersForm()
        {
            InitializeComponent();
            SetupUI();
        }

        private void SetupUI()
        {
            this.Text = "Accounting Vouchers / السندات المحاسبية";
            this.Size = new Size(700, 500);

            TabControl tabs = new TabControl { Dock = DockStyle.Fill };

            TabPage tabReceipt = new TabPage("Receipt Vouchers / سندات القبض");
            tabReceipt.Controls.Add(new DataGridView { Dock = DockStyle.Fill });

            TabPage tabPayment = new TabPage("Payment Vouchers / سندات الصرف");
            tabPayment.Controls.Add(new DataGridView { Dock = DockStyle.Fill });

            tabs.TabPages.AddRange(new TabPage[] { tabReceipt, tabPayment });
            this.Controls.Add(tabs);

            LanguageHelper.ApplyLanguage(this);
        }

        private void InitializeComponent() { }
    }
}
