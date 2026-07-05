using Supermarket.UI.Helpers;
using System;
using System.Drawing;
using System.Windows.Forms;

namespace Supermarket.UI.Views
{
    public partial class CategoryEditForm : Form
    {
        public CategoryEditForm()
        {
            InitializeComponent();
            SetupUI();
        }

        private void SetupUI()
        {
            this.Text = "Category / المجموعة";
            this.Size = new Size(400, 250);
            this.StartPosition = FormStartPosition.CenterParent;

            this.Controls.Add(new Label { Text = "Name / الاسم", Location = new Point(20, 30), AutoSize = true });
            this.Controls.Add(new TextBox { Location = new Point(150, 28), Width = 200 });

            this.Controls.Add(new Label { Text = "Description / الوصف", Location = new Point(20, 70), AutoSize = true });
            this.Controls.Add(new TextBox { Location = new Point(150, 68), Width = 200, Multiline = true, Height = 60 });

            Button btnSave = new Button { Text = "Save / حفظ", Location = new Point(20, 150), Width = 330, Height = 40, BackColor = Color.Green, ForeColor = Color.White };
            this.Controls.Add(btnSave);

            LanguageHelper.ApplyLanguage(this);
        }

        private void InitializeComponent() { }
    }

    public partial class StoreEditForm : Form
    {
        public StoreEditForm()
        {
            InitializeComponent();
            SetupUI();
        }

        private void SetupUI()
        {
            this.Text = "Store / المخزن";
            this.Size = new Size(400, 300);
            this.StartPosition = FormStartPosition.CenterParent;

            this.Controls.Add(new Label { Text = "Store Name / الاسم", Location = new Point(20, 30), AutoSize = true });
            this.Controls.Add(new TextBox { Location = new Point(150, 28), Width = 200 });

            this.Controls.Add(new Label { Text = "Location / الموقع", Location = new Point(20, 70), AutoSize = true });
            this.Controls.Add(new TextBox { Location = new Point(150, 68), Width = 200 });

            this.Controls.Add(new CheckBox { Text = "Main Store / مخزن رئيسي", Location = new Point(150, 110), AutoSize = true });

            Button btnSave = new Button { Text = "Save / حفظ", Location = new Point(20, 180), Width = 330, Height = 40, BackColor = Color.Green, ForeColor = Color.White };
            this.Controls.Add(btnSave);

            LanguageHelper.ApplyLanguage(this);
        }

        private void InitializeComponent() { }
    }
}
