using System.Drawing;
using System.Windows.Forms;

namespace Supermarket.UI.Helpers
{
    public static class UITheme
    {
        // Colors from the reference image
        public static Color PrimaryColor = Color.FromArgb(0, 123, 255);      // Vibrant Blue
        public static Color SecondaryColor = Color.FromArgb(108, 117, 125); // Gray
        public static Color SuccessColor = Color.FromArgb(40, 167, 69);     // Green
        public static Color DangerColor = Color.FromArgb(220, 53, 69);      // Red
        public static Color WarningColor = Color.FromArgb(255, 193, 7);     // Yellow
        public static Color InfoColor = Color.FromArgb(23, 162, 184);       // Cyan/Teal

        public static Color SidebarBg = Color.FromArgb(45, 45, 48);        // Professional dark sidebar
        public static Color SidebarSelected = Color.FromArgb(0, 122, 255); // Active blue
        public static Color HeaderBg = Color.White;
        public static Color ContentBg = Color.FromArgb(240, 242, 245);
        public static Color GridHeaderBg = Color.FromArgb(0, 115, 183);

        public static Color TextPrimary = Color.FromArgb(33, 37, 41);
        public static Color TextSecondary = Color.FromArgb(108, 117, 125);
        public static Color TextOnPrimary = Color.White;

        public static Font HeaderFont = new Font("Segoe UI", 12, FontStyle.Bold);
        public static Font MainFont = new Font("Segoe UI", 10);
        public static Font GridFont = new Font("Segoe UI", 9);

        public static void ApplyModernStyle(Control control)
        {
            if (control is Button btn)
            {
                btn.FlatStyle = FlatStyle.Flat;
                btn.FlatAppearance.BorderSize = 0;
                btn.Cursor = Cursors.Hand;
                btn.Font = new Font("Segoe UI", 9, FontStyle.Bold);

                if (btn.BackColor == SystemColors.Control || btn.BackColor == Color.Transparent)
                {
                    btn.BackColor = PrimaryColor;
                    btn.ForeColor = Color.White;
                }
            }
            else if (control is DataGridView dgv)
            {
                dgv.EnableHeadersVisualStyles = false;
                dgv.BackgroundColor = Color.White;
                dgv.BorderStyle = BorderStyle.None;
                dgv.ColumnHeadersBorderStyle = DataGridViewHeaderBorderStyle.None;
                dgv.ColumnHeadersDefaultCellStyle.BackColor = GridHeaderBg;
                dgv.ColumnHeadersDefaultCellStyle.ForeColor = Color.White;
                dgv.ColumnHeadersDefaultCellStyle.Font = new Font("Segoe UI", 10, FontStyle.Bold);
                dgv.ColumnHeadersHeight = 40;
                dgv.RowTemplate.Height = 35;
                dgv.AlternatingRowsDefaultCellStyle.BackColor = Color.FromArgb(240, 240, 240);
                dgv.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
                dgv.CellBorderStyle = DataGridViewCellBorderStyle.SingleHorizontal;
                dgv.GridColor = Color.FromArgb(224, 224, 224);
            }
            else if (control is Panel p && p.Tag?.ToString() == "Card")
            {
                p.BackColor = Color.White;
                p.BorderStyle = BorderStyle.None;
                // WinForms doesn't support shadows easily, but we can use border
            }

            foreach (Control child in control.Controls)
            {
                ApplyModernStyle(child);
            }
        }
    }
}
