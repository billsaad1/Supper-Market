using System;
using System.Drawing;
using System.Windows.Forms;

namespace Supermarket.UI
{
    public partial class HRForm : Form
    {
        public HRForm()
        {
            InitializeComponent();
            SetupUI();
        }

        private void SetupUI()
        {
            this.Text = "Human Resources / الموارد البشرية";
            this.Size = new Size(800, 500);

            TabControl hrTabs = new TabControl { Dock = DockStyle.Fill };

            TabPage tabEmployees = new TabPage("Employees / الموظفون");
            DataGridView dgvEmp = new DataGridView { Dock = DockStyle.Fill };
            tabEmployees.Controls.Add(dgvEmp);

            TabPage tabAttendance = new TabPage("Attendance / الحضور والانصراف");
            DataGridView dgvAtt = new DataGridView { Dock = DockStyle.Fill };
            tabAttendance.Controls.Add(dgvAtt);

            hrTabs.TabPages.AddRange(new TabPage[] { tabEmployees, tabAttendance });
            this.Controls.Add(hrTabs);

            LanguageHelper.ApplyLanguage(this);
        }

        private void InitializeComponent() { }
    }
}
