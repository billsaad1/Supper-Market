using System;
using System.Data;
using System.Threading.Tasks;
using Dapper;
using Microsoft.Data.SqlClient;

namespace Supermarket.DAL
{
    public class AttendanceRepository
    {
        private readonly string _connectionString;

        public AttendanceRepository(string connectionString)
        {
            _connectionString = connectionString;
        }

        public async Task LogAttendanceAsync(int employeeId, bool isCheckIn)
        {
            using (IDbConnection db = new SqlConnection(_connectionString))
            {
                if (isCheckIn)
                {
                    string sql = "INSERT INTO Attendance (EmployeeID, Date, TimeIn) VALUES (@employeeId, CAST(GETDATE() AS DATE), CAST(GETDATE() AS TIME))";
                    await db.ExecuteAsync(sql, new { employeeId });
                }
                else
                {
                    string sql = "UPDATE Attendance SET TimeOut = CAST(GETDATE() AS TIME) WHERE EmployeeID = @employeeId AND Date = CAST(GETDATE() AS DATE)";
                    await db.ExecuteAsync(sql, new { employeeId });
                }
            }
        }
    }
}
