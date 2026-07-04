using System;
using System.Drawing;
using System.Windows.Forms;

namespace Supermarket.UI
{
    public partial class PromotionsForm : Form
    {
        public PromotionsForm()
        {
            InitializeComponent();
            SetupUI();
        }

        private void SetupUI()
        {
            this.Text = "Promotions / العروض الترويجية";
            this.Size = new Size(600, 400);

            DataGridView dgv = new DataGridView { Dock = DockStyle.Fill };
            dgv.Columns.Add("Name", "Promo Name / اسم العرض");
            dgv.Columns.Add("Item", "Item / الصنف");
            dgv.Columns.Add("Discount", "Discount % / الخصم");

            Button btnAdd = new Button { Text = "Add Promotion / إضافة عرض", Dock = DockStyle.Bottom, Height = 40, BackColor = Color.Purple, ForeColor = Color.White };

            this.Controls.Add(dgv);
            this.Controls.Add(btnAdd);

            LanguageHelper.ApplyLanguage(this);
        }

        private void InitializeComponent() { }
    }
}
