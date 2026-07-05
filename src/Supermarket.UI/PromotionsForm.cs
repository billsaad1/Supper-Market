using System;
using System.Drawing;
using System.Windows.Forms;
using System.Collections.Generic;
using Dapper;
using Microsoft.Data.SqlClient;

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
            this.Text = "Manage Promotions / إدارة العروض";
            this.Size = new Size(800, 500);

            Panel pnlEditor = new Panel { Dock = DockStyle.Top, Height = 120, BackColor = Color.WhiteSmoke };
            pnlEditor.Controls.Add(new Label { Text = "Promo Name / اسم العرض", Location = new Point(20, 20), AutoSize = true });
            pnlEditor.Controls.Add(new TextBox { Location = new Point(180, 18), Width = 200 });

            pnlEditor.Controls.Add(new Label { Text = "Discount % / الخصم", Location = new Point(20, 60), AutoSize = true });
            pnlEditor.Controls.Add(new NumericUpDown { Location = new Point(180, 58), Width = 100 });

            Button btnAdd = new Button { Text = "Add / إضافة", Location = new Point(400, 15), Width = 120, Height = 70, BackColor = Color.Purple, ForeColor = Color.White };
            pnlEditor.Controls.Add(btnAdd);

            DataGridView dgv = new DataGridView { Dock = DockStyle.Fill, BackgroundColor = Color.White };
            dgv.Columns.Add("ID", "ID");
            dgv.Columns.Add("Name", "Promo / العرض");
            dgv.Columns.Add("Disc", "Discount / الخصم");

            this.Controls.Add(dgv);
            this.Controls.Add(pnlEditor);

            LanguageHelper.ApplyLanguage(this);
        }

        private void InitializeComponent() { }
    }
}
