using System.Collections.Generic;
using System.Data;
using System.Threading.Tasks;
using Dapper;
using Microsoft.Data.SqlClient;
using Supermarket.Models.Entities;

namespace Supermarket.DAL
{
    public class ContactRepository
    {
        private readonly string _connectionString;

        public ContactRepository(string connectionString)
        {
            _connectionString = connectionString;
        }

        // Suppliers
        public async Task<IEnumerable<dynamic>> GetAllSuppliersAsync()
        {
            using (IDbConnection db = new SqlConnection(_connectionString))
            {
                return await db.QueryAsync("SELECT * FROM Suppliers");
            }
        }

        // Customers
        public async Task<IEnumerable<dynamic>> GetAllCustomersAsync()
        {
            using (IDbConnection db = new SqlConnection(_connectionString))
            {
                return await db.QueryAsync("SELECT * FROM Customers");
            }
        }
    }
}
