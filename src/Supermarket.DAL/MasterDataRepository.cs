using System;
using System.Collections.Generic;
using System.Data;
using System.Threading.Tasks;
using Dapper;
using Microsoft.Data.SqlClient;
using Supermarket.Models.Entities;

namespace Supermarket.DAL
{
    public class MasterDataRepository
    {
        private readonly string _connectionString;

        public MasterDataRepository(string connectionString)
        {
            _connectionString = connectionString;
        }

        // Categories CRUD
        public async Task<int> UpsertCategoryAsync(int id, string name, string description)
        {
            using (IDbConnection db = new SqlConnection(_connectionString))
            {
                string sql = id == 0
                    ? "INSERT INTO Categories (CategoryName, Description) VALUES (@name, @description)"
                    : "UPDATE Categories SET CategoryName = @name, Description = @description WHERE CategoryID = @id";
                return await db.ExecuteAsync(sql, new { id, name, description });
            }
        }

        // Stores CRUD
        public async Task<int> UpsertStoreAsync(int id, string name, string location, bool isMain)
        {
            using (IDbConnection db = new SqlConnection(_connectionString))
            {
                string sql = id == 0
                    ? "INSERT INTO Stores (StoreName, Location, IsMainStore) VALUES (@name, @location, @isMain)"
                    : "UPDATE Stores SET StoreName = @name, Location = @location, IsMainStore = @isMain WHERE StoreID = @id";
                return await db.ExecuteAsync(sql, new { id, name, location, isMain });
            }
        }

        // Generic Delete
        public async Task<int> DeleteRecordAsync(string table, string column, int id)
        {
            using (IDbConnection db = new SqlConnection(_connectionString))
            {
                return await db.ExecuteAsync($"DELETE FROM {table} WHERE {column} = @id", new { id });
            }
        }
    }
}
