using System;
using System.Drawing;
using System.Windows.Forms;
using System.Collections.Generic;
using System.Data;
using Dapper;
using Microsoft.Data.SqlClient;

namespace Supermarket.UI
{
    public partial class AccountsForm : Form
    {
        private TreeView tvAccounts;

        public AccountsForm()
        {
            InitializeComponent();
            SetupUI();
        }

        private void SetupUI()
        {
            this.Text = "Chart of Accounts / دليل الحسابات";
            this.Size = new Size(600, 700);

            tvAccounts = new TreeView {
                Dock = DockStyle.Fill,
                Font = new Font("Arial", 12),
                ImageIndex = 0,
                SelectedImageIndex = 0
            };

            LoadAccounts();

            this.Controls.Add(tvAccounts);
            LanguageHelper.ApplyLanguage(this);
        }

        private void LoadAccounts()
        {
            tvAccounts.Nodes.Clear();

            TreeNode assets = new TreeNode("Assets / الأصول");
            assets.Nodes.Add("1101", "Cash / الصندوق");
            assets.Nodes.Add("1201", "Inventory / المخزون");
            assets.Nodes.Add("1102", "Customers / العملاء");

            TreeNode revenue = new TreeNode("Revenue / الإيرادات");
            revenue.Nodes.Add("4101", "Sales / المبيعات");

            TreeNode expenses = new TreeNode("Expenses / المصروفات");
            expenses.Nodes.Add("5101", "COGS / تكلفة المبيعات");

            tvAccounts.Nodes.AddRange(new TreeNode[] { assets, revenue, expenses });
            tvAccounts.ExpandAll();
        }

        private void InitializeComponent() { }
    }
}
