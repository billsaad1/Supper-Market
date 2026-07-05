using System;
using System.Data;
using System.Threading.Tasks;
using Microsoft.Data.SqlClient;

namespace Supermarket.DAL
{
    public class BackupRepository
    {
        private readonly string _connectionString;

        public BackupRepository(string connectionString)
        {
            _connectionString = connectionString;
        }

        public async Task CreateBackupAsync(string path)
        {
            using (SqlConnection conn = new SqlConnection(_connectionString))
            {
                await conn.OpenAsync();
                string dbName = conn.Database;
                string sql = $"BACKUP DATABASE [{dbName}] TO DISK = '{path}'";
                using (SqlCommand cmd = new SqlCommand(sql, conn))
                {
                    await cmd.ExecuteNonQueryAsync();
                }
            }
        }

        public async Task RestoreBackupAsync(string path)
        {
            using (SqlConnection conn = new SqlConnection(_connectionString))
            {
                await conn.OpenAsync();
                string dbName = conn.Database;
                // Need to switch to master to restore
                string sql = $@"USE master;
                                ALTER DATABASE [{dbName}] SET SINGLE_USER WITH ROLLBACK IMMEDIATE;
                                RESTORE DATABASE [{dbName}] FROM DISK = '{path}' WITH REPLACE;
                                ALTER DATABASE [{dbName}] SET MULTI_USER;";
                using (SqlCommand cmd = new SqlCommand(sql, conn))
                {
                    await cmd.ExecuteNonQueryAsync();
                }
            }
        }
    }
}
