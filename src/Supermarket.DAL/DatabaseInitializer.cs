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

                    // 4. Run Schema Script
                    using (IDbConnection targetDb = new SqlConnection(connectionString))
                    {
                        targetDb.Open();
                        // Split script by GO
                        var commands = Regex.Split(script, @"^\s*GO\s*$", RegexOptions.Multiline | RegexOptions.IgnoreCase);
                        foreach (var cmd in commands)
                        {
                            if (!string.IsNullOrWhiteSpace(cmd))
                            {
                                // Skip USE [Database] since we are already connecting to it or it might conflict
                                if (cmd.Trim().StartsWith("USE ", StringComparison.OrdinalIgnoreCase) ||
                                    cmd.Trim().StartsWith("CREATE DATABASE ", StringComparison.OrdinalIgnoreCase))
                                    continue;

                                await targetDb.ExecuteAsync(cmd);
                            }
                        }
                    }
                }
            }
        }
    }
}
