using System;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using Dapper;
using Microsoft.Data.SqlClient;
using System.Text.RegularExpressions;

namespace Supermarket.DAL
{
    public static class DatabaseInitializer
    {
        public static async Task InitializeDatabaseAsync(string connectionString, string script)
        {
            // 1. Get Master Connection String
            var builder = new SqlConnectionStringBuilder(connectionString);
            string databaseName = builder.InitialCatalog;
            builder.InitialCatalog = "master";
            string masterConnectionString = builder.ConnectionString;

            using (IDbConnection masterDb = new SqlConnection(masterConnectionString))
            {
                // 2. Check if Database Exists
                var dbId = await masterDb.ExecuteScalarAsync<int?>(
                    "SELECT database_id FROM sys.databases WHERE name = @name", new { name = databaseName });

                if (dbId == null)
                {
                    // 3. Create Database
                    await masterDb.ExecuteAsync($"CREATE DATABASE [{databaseName}]");
                }

                // 4. Run Schema Script if tables are missing
                using (IDbConnection targetDb = new SqlConnection(connectionString))
                {
                    targetDb.Open();

                    // Check if a core table exists
                    var tableExists = await targetDb.ExecuteScalarAsync<int?>(
                        "SELECT 1 FROM sys.tables WHERE name = 'Users'");

                    if (tableExists == null)
                    {
                        // Split script by GO
                        var commands = Regex.Split(script, @"^\s*GO\s*$", RegexOptions.Multiline | RegexOptions.IgnoreCase);
                        foreach (var cmd in commands)
                        {
                            if (!string.IsNullOrWhiteSpace(cmd))
                            {
                                // Skip USE [Database] and CREATE DATABASE since we handle that manually or it's already there
                                string trimmedCmd = cmd.Trim();
                                if (trimmedCmd.StartsWith("USE ", StringComparison.OrdinalIgnoreCase) ||
                                    trimmedCmd.StartsWith("CREATE DATABASE ", StringComparison.OrdinalIgnoreCase))
                                    continue;

                                try
                                {
                                    await targetDb.ExecuteAsync(cmd);
                                }
                                catch (Exception ex)
                                {
                                    // Log or handle error if a specific command fails but continue with others
                                    Console.WriteLine($"Error executing script command: {ex.Message}");
                                }
                            }
                        }
                    }
                }
            }
        }
    }
}
