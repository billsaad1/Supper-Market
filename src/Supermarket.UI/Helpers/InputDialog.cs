using System;
using System.Drawing;
using System.Windows.Forms;

namespace Supermarket.UI.Helpers
{
    public class InputDialog : Form
    {
        private TextBox txtInput;
        public string Value => txtInput.Text;

        public InputDialog(string title, string prompt, string defaultValue = "")
        {
            this.Text = title;
            this.Size = new Size(350, 180);
            this.StartPosition = FormStartPosition.CenterParent;
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.MinimizeBox = false;
            this.MaximizeBox = false;

            Label lbl = new Label { Text = prompt, Location = new Point(20, 20), AutoSize = true };
            txtInput = new TextBox { Text = defaultValue, Location = new Point(20, 50), Width = 290 };

            Button btnOk = new Button { Text = "OK", Location = new Point(130, 90), Width = 80, DialogResult = DialogResult.OK };
            Button btnCancel = new Button { Text = "Cancel", Location = new Point(220, 90), Width = 80, DialogResult = DialogResult.Cancel };

            this.Controls.AddRange(new Control[] { lbl, txtInput, btnOk, btnCancel });
            this.AcceptButton = btnOk;
        }

        public static string Show(string title, string prompt, string defaultValue = "")
        {
            using (var dlg = new InputDialog(title, prompt, defaultValue))
            {
                if (dlg.ShowDialog() == DialogResult.OK) return dlg.Value;
                return null;
            }
        }
    }
}
