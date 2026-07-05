using System;
using System.Drawing;
using System.Windows.Forms;
using Supermarket.DAL;

namespace Supermarket.UI
{
    public partial class HRForm : Form
    {
        private AttendanceRepository _attRepo;

        public HRForm()
        {
            InitializeComponent();
            _attRepo = new AttendanceRepository(AppSettings.ConnectionString);
            SetupUI();
        }

        private void SetupUI()
        {
            this.Text = "Human Resources / الموارد البشرية";
            this.Size = new Size(900, 600);

            TabControl hrTabs = new TabControl { Dock = DockStyle.Fill };

            TabPage tabEmployees = new TabPage("Employees / الموظفون");
            DataGridView dgvEmp = new DataGridView { Dock = DockStyle.Fill };
            tabEmployees.Controls.Add(dgvEmp);

            TabPage tabAttendance = new TabPage("Attendance / الحضور والانصراف");
            Panel attControls = new Panel { Dock = DockStyle.Top, Height = 100 };

            Button btnCheckIn = new Button { Text = "Check-In / حضور", Location = new Point(20, 20), Width = 150, Height = 50, BackColor = Color.Green, ForeColor = Color.White };
            btnCheckIn.Click += async (s, e) => {
                await _attRepo.LogAttendanceAsync(1, true); // Dummy emp ID
                MessageBox.Show("Check-in logged / تم تسجيل الحضور");
            };

            Button btnCheckOut = new Button { Text = "Check-Out / انصراف", Location = new Point(190, 20), Width = 150, Height = 50, BackColor = Color.Red, ForeColor = Color.White };
            btnCheckOut.Click += async (s, e) => {
                await _attRepo.LogAttendanceAsync(1, false);
                MessageBox.Show("Check-out logged / تم تسجيل الانصراف");
            };

            attControls.Controls.AddRange(new Control[] { btnCheckIn, btnCheckOut });
            DataGridView dgvAtt = new DataGridView { Dock = DockStyle.Fill };
            tabAttendance.Controls.Add(dgvAtt);
            tabAttendance.Controls.Add(attControls);

            hrTabs.TabPages.AddRange(new TabPage[] { tabEmployees, tabAttendance });
            this.Controls.Add(hrTabs);

            LanguageHelper.ApplyLanguage(this);
        }

        private void InitializeComponent() { }
    }
}
