using System.Collections.Generic;
using System.Data;
using System.Threading.Tasks;
using Dapper;
using Microsoft.Data.SqlClient;
using Supermarket.Models.Entities;

namespace Supermarket.DAL
{
    public class LocalizationRepository
    {
        private readonly string _connectionString;

        public LocalizationRepository(string connectionString)
        {
            _connectionString = connectionString;
        }

        public async Task<IEnumerable<LocalizationResource>> GetAllResourcesAsync()
        {
            using (IDbConnection db = new SqlConnection(_connectionString))
            {
                return await db.QueryAsync<LocalizationResource>("SELECT * FROM Localization");
            }
        }
    }
}
