using System;
using System.Drawing;
using System.Windows.Forms;

namespace Supermarket.UI
{
    public partial class ShiftClosingForm : Form
    {
        public ShiftClosingForm() { InitializeComponent(); SetupUI(); }
        private void SetupUI() { this.Text = "Close Shift"; this.Size = new Size(400, 300); this.Controls.Add(new Label { Text = "Closing logic here..." }); LanguageHelper.ApplyLanguage(this); }
        private void InitializeComponent() { }
    }
}
