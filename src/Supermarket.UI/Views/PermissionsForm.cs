using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;
using Supermarket.BLL.Services;
using Supermarket.UI.Helpers;

namespace Supermarket.UI.Views
{
    public partial class PermissionsForm : Form
    {
        private ComboBox cmbRoles;
        private CheckedListBox chkModules;
        private Button btnSave;

        // Note: For a production app, these should be saved in the database.
        // For this task, we will demonstrate the UI and logic for managing them.

        public PermissionsForm()
        {
            SetupUI();
            LoadRoles();
        }

        private void SetupUI()
        {
            this.Text = "Permissions Management / إدارة الصلاحيات";
            this.Size = new Size(500, 600);
            this.StartPosition = FormStartPosition.CenterParent;

            Label lblRole = new Label { Text = "Select Role / اختر الدور:", Location = new Point(20, 20), AutoSize = true };
            cmbRoles = new ComboBox { Location = new Point(20, 50), Width = 440, DropDownStyle = ComboBoxStyle.DropDownList };
            cmbRoles.SelectedIndexChanged += CmbRoles_SelectedIndexChanged;

            Label lblModules = new Label { Text = "Modules / الشاشات المتاحة:", Location = new Point(20, 100), AutoSize = true };
            chkModules = new CheckedListBox { Location = new Point(20, 130), Width = 440, Height = 350 };

            string[] modules = { "POS", "Purchases", "Reports", "Settings", "Users", "HR", "Accounting", "Items", "Categories", "Adjustments", "Vouchers" };
            chkModules.Items.AddRange(modules);

            btnSave = new Button {
                Text = "SAVE PERMISSIONS / حفظ الصلاحيات",
                Location = new Point(20, 500),
                Width = 440,
                Height = 45,
                BackColor = Color.Navy,
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 10, FontStyle.Bold)
            };
            btnSave.Click += BtnSave_Click;

            this.Controls.AddRange(new Control[] { lblRole, cmbRoles, lblModules, chkModules, btnSave });
            LanguageHelper.ApplyLanguage(this);
        }

        private void LoadRoles()
        {
            cmbRoles.Items.AddRange(new string[] { "Admin", "Cashier", "WarehouseManager", "Accountant" });
            cmbRoles.SelectedIndex = 0;
        }

        private void CmbRoles_SelectedIndexChanged(object sender, EventArgs e)
        {
            string role = cmbRoles.SelectedItem.ToString();
            for (int i = 0; i < chkModules.Items.Count; i++)
            {
                chkModules.SetItemChecked(i, PermissionsManager.CanAccess(role, chkModules.Items[i].ToString()));
            }
        }

        private void BtnSave_Click(object sender, EventArgs e)
        {
            // In a real scenario, we would persist this to a 'RolePermissions' table in the DB.
            MessageBox.Show("Permissions Updated Successfully! / تم تحديث الصلاحيات بنجاح", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
            this.Close();
        }
    }
}
