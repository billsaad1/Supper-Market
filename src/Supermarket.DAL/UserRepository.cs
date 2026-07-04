using System.Data;
using System.Threading.Tasks;
using Dapper;
using Microsoft.Data.SqlClient;
using Supermarket.Models.Entities;

namespace Supermarket.DAL
{
    public class UserRepository
    {
        private readonly string _connectionString;

        public UserRepository(string connectionString)
        {
            _connectionString = connectionString;
        }

        public async Task<User> GetUserByUsernameAsync(string username)
        {
            using (IDbConnection db = new SqlConnection(_connectionString))
            {
                return await db.QueryFirstOrDefaultAsync<User>(
                    "SELECT * FROM Users WHERE Username = @Username AND IsActive = 1",
                    new { Username = username });
            }
        }
    }
}
