using Supermarket.UI.Helpers;
using System;
using System.Drawing;
using System.Windows.Forms;
using System.Collections.Generic;
using System.Data;
using Dapper;
using Microsoft.Data.SqlClient;

namespace Supermarket.UI.Views
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

        private async void LoadAccounts()
        {
            tvAccounts.Nodes.Clear();
            try
            {
                using (var db = new Microsoft.Data.SqlClient.SqlConnection(AppSettings.ConnectionString))
                {
                    var accounts = await db.QueryAsync<dynamic>(@"
                        SELECT a.*, (SELECT ISNULL(SUM(Debit - Credit), 0) FROM JournalEntryDetails WHERE AccountID = a.AccountID) as Balance
                        FROM ChartOfAccounts a");

                    var mainNodes = new Dictionary<string, TreeNode>();
                    foreach (var acc in accounts.Where(a => a.ParentAccountID == null))
                    {
                        var node = new TreeNode($"{acc.AccountNumber} - {acc.AccountName} ({acc.Balance:F2})");
                        mainNodes.Add(acc.AccountID.ToString(), node);
                        tvAccounts.Nodes.Add(node);
                    }

                    foreach (var acc in accounts.Where(a => a.ParentAccountID != null))
                    {
                        if (mainNodes.ContainsKey(acc.ParentAccountID.ToString()))
                        {
                            mainNodes[acc.ParentAccountID.ToString()].Nodes.Add($"{acc.AccountNumber} - {acc.AccountName} ({acc.Balance:F2})");
                        }
                    }
                }
                tvAccounts.ExpandAll();
            }
            catch { /* Handled if DB not ready */ }
        }

        private void InitializeComponent() { }
    }
}
