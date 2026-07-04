using System.Collections.Generic;
using System.Data;
using System.Threading.Tasks;
using Dapper;
using Microsoft.Data.SqlClient;
using Supermarket.Models.Entities;

namespace Supermarket.DAL
{
    public class SettingsRepository
    {
        private readonly string _connectionString;

        public SettingsRepository(string connectionString)
        {
            _connectionString = connectionString;
        }

        // Stores
        public async Task<IEnumerable<Store>> GetAllStoresAsync()
        {
            using (IDbConnection db = new SqlConnection(_connectionString))
            {
                return await db.QueryAsync<Store>("SELECT * FROM Stores");
            }
        }

        public async Task<int> AddStoreAsync(Store store)
        {
            using (IDbConnection db = new SqlConnection(_connectionString))
            {
                string sql = "INSERT INTO Stores (StoreName, Location, IsMainStore) VALUES (@StoreName, @Location, @IsMainStore)";
                return await db.ExecuteAsync(sql, store);
            }
        }

        // Items
        public async Task<IEnumerable<Item>> GetAllItemsAsync()
        {
            using (IDbConnection db = new SqlConnection(_connectionString))
            {
                return await db.QueryAsync<Item>("SELECT * FROM Items");
            }
        }

        public async Task<int> AddItemAsync(Item item)
        {
            using (IDbConnection db = new SqlConnection(_connectionString))
            {
                string sql = @"INSERT INTO Items (Barcode, ItemName, CategoryID, Unit, CostPrice, SalePrice, TaxRate, MinimumStockLevel, HasExpiryDate)
                               VALUES (@Barcode, @ItemName, @CategoryID, @Unit, @CostPrice, @SalePrice, @TaxRate, @MinimumStockLevel, @HasExpiryDate)";
                return await db.ExecuteAsync(sql, item);
            }
        }
    }
}
