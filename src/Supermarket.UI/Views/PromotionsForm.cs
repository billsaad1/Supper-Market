using Supermarket.UI.Helpers;
using System;
using System.Drawing;
using System.Windows.Forms;

namespace Supermarket.UI.Views
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
            this.Text = "Promotions Management / إدارة العروض والترويج";
            this.Size = new Size(900, 600);

            Panel left = new Panel { Dock = DockStyle.Left, Width = 350, BackColor = Color.WhiteSmoke, Padding = new Padding(20) };

            left.Controls.Add(new Label { Text = "Promotion Name / اسم العرض", Location = new Point(20, 20), AutoSize = true });
            left.Controls.Add(new TextBox { Location = new Point(20, 45), Width = 300 });

            left.Controls.Add(new Label { Text = "Type / النوع", Location = new Point(20, 90), AutoSize = true });
            ComboBox cbType = new ComboBox { Location = new Point(20, 115), Width = 300 };
            cbType.Items.AddRange(new string[] { "Percentage Discount / خصم مئوي", "Buy 1 Get 1 / اشتر 1 واحصل على 1", "Fixed Amount / مبلغ ثابت" });
            left.Controls.Add(cbType);

            left.Controls.Add(new Label { Text = "Value / القيمة", Location = new Point(20, 160), AutoSize = true });
            left.Controls.Add(new NumericUpDown { Location = new Point(20, 185), Width = 100 });

            Button btnSave = new Button { Text = "Add Promotion / إضافة العرض", Location = new Point(20, 250), Width = 300, Height = 50, BackColor = Color.Purple, ForeColor = Color.White, Font = new Font("Arial", 12, FontStyle.Bold) };
            left.Controls.Add(btnSave);

            DataGridView dgv = new DataGridView { Dock = DockStyle.Fill, BackgroundColor = Color.White };
            dgv.Columns.Add("Name", "Promotion");
            dgv.Columns.Add("Type", "Type");
            dgv.Columns.Add("Val", "Value");

            this.Controls.Add(dgv);
            this.Controls.Add(left);

            LanguageHelper.ApplyLanguage(this);
        }

        private void InitializeComponent() { }
    }
}
