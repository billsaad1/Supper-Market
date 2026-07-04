using System;
using System.Data;
using System.Threading.Tasks;
using Dapper;
using Microsoft.Data.SqlClient;

namespace Supermarket.DAL
{
    public class ShiftRepository
    {
        private readonly string _connectionString;

        public ShiftRepository(string connectionString)
        {
            _connectionString = connectionString;
        }

        public async Task<int> OpenShiftAsync(int userId, decimal openingBalance)
        {
            using (IDbConnection db = new SqlConnection(_connectionString))
            {
                string sql = @"INSERT INTO CashierShifts (UserID, StartTime, OpeningBalance, Status)
                               VALUES (@userId, GETDATE(), @openingBalance, 'Open');
                               SELECT CAST(SCOPE_IDENTITY() as int)";
                return await db.QuerySingleAsync<int>(sql, new { userId, openingBalance });
            }
        }

        public async Task CloseShiftAsync(int shiftId, decimal actualAmount)
        {
            using (IDbConnection db = new SqlConnection(_connectionString))
            {
                string sql = @"UPDATE CashierShifts
                               SET EndTime = GETDATE(), ActualAmount = @actualAmount, Status = 'Closed'
                               WHERE ShiftID = @shiftId";
                await db.ExecuteAsync(sql, new { shiftId, actualAmount });
            }
        }
    }
}
