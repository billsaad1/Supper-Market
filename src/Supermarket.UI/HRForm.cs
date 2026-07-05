using System;
using System.Drawing;
using System.Windows.Forms;
using System.Collections.Generic;
using Supermarket.DAL;
using Dapper;

namespace Supermarket.UI
{
    public partial class HRForm : Form
    {
        private AttendanceRepository _attRepo;
        private ComboBox cbEmployees;

        public HRForm()
        {
            InitializeComponent();
            _attRepo = new AttendanceRepository(AppSettings.ConnectionString);
            SetupUI();
        }

        private void SetupUI()
        {
            this.Text = "HR & Attendance / الموارد البشرية";
            this.Size = new Size(800, 600);

            TabControl hrTabs = new TabControl { Dock = DockStyle.Fill };

            TabPage tabAttendance = new TabPage("Daily Attendance / الحضور اليومي");
            Panel pnlAtt = new Panel { Dock = DockStyle.Fill, Padding = new Padding(20) };

            pnlAtt.Controls.Add(new Label { Text = "Select Employee / اختر الموظف", Location = new Point(20, 20), AutoSize = true });
            cbEmployees = new ComboBox { Location = new Point(200, 18), Width = 250 };
            LoadEmployees();
            pnlAtt.Controls.Add(cbEmployees);

            Button btnIn = new Button { Text = "CHECK IN / تسجيل حضور", Location = new Point(20, 80), Width = 200, Height = 60, BackColor = Color.Green, ForeColor = Color.White, Font = new Font("Arial", 12, FontStyle.Bold) };
            btnIn.Click += async (s, e) => {
                await _attRepo.LogAttendanceAsync(1, true);
                MessageBox.Show("Check-in recorded / تم تسجيل الحضور");
            };

            Button btnOut = new Button { Text = "CHECK OUT / تسجيل انصراف", Location = new Point(230, 80), Width = 200, Height = 60, BackColor = Color.Red, ForeColor = Color.White, Font = new Font("Arial", 12, FontStyle.Bold) };
            btnOut.Click += async (s, e) => {
                await _attRepo.LogAttendanceAsync(1, false);
                MessageBox.Show("Check-out recorded / تم تسجيل الانصراف");
            };

            pnlAtt.Controls.Add(btnIn);
            pnlAtt.Controls.Add(btnOut);
            tabAttendance.Controls.Add(pnlAtt);

            hrTabs.TabPages.Add(tabAttendance);
            this.Controls.Add(hrTabs);

            LanguageHelper.ApplyLanguage(this);
        }

        private async void LoadEmployees()
        {
            string conn = AppSettings.ConnectionString;
            using (var db = new Microsoft.Data.SqlClient.SqlConnection(conn))
            {
                var emps = await db.QueryAsync<dynamic>("SELECT EmployeeID, EmployeeName FROM Employees");
                foreach (var emp in emps)
                {
                    cbEmployees.Items.Add(new { ID = (int)emp.EmployeeID, Name = (string)emp.EmployeeName });
                }
                cbEmployees.DisplayMember = "Name";
                cbEmployees.ValueMember = "ID";
            }
        }

        private void InitializeComponent() { }
    }
}
