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

        public async Task<IEnumerable<Item>> GetAllItemsAsync()
        {
            using (IDbConnection db = new SqlConnection(_connectionString))
            {
                return await db.QueryAsync<Item>("SELECT * FROM Items");
            }
        }

        public async Task<int> UpsertItemAsync(Item item)
        {
            using (IDbConnection db = new SqlConnection(_connectionString))
            {
                string sql = item.ItemID == 0
                    ? @"INSERT INTO Items (Barcode, ItemName, CategoryID, Unit, CostPrice, SalePrice, TaxRate, MinimumStockLevel)
                        VALUES (@Barcode, @ItemName, @CategoryID, @Unit, @CostPrice, @SalePrice, @TaxRate, @MinimumStockLevel)"
                    : @"UPDATE Items SET Barcode = @Barcode, ItemName = @ItemName, CategoryID = @CategoryID,
                        Unit = @Unit, CostPrice = @CostPrice, SalePrice = @SalePrice, TaxRate = @TaxRate,
                        MinimumStockLevel = @MinimumStockLevel WHERE ItemID = @ItemID";
                return await db.ExecuteAsync(sql, item);
            }
        }

        public async Task<Item> GetItemByBarcodeAsync(string barcode)
        {
            using (IDbConnection db = new SqlConnection(_connectionString))
            {
                return await db.QueryFirstOrDefaultAsync<Item>("SELECT * FROM Items WHERE Barcode = @barcode", new { barcode });
            }
        }

        public async Task<IEnumerable<Category>> GetAllCategoriesAsync()
        {
            using (IDbConnection db = new SqlConnection(_connectionString))
            {
                return await db.QueryAsync<Category>("SELECT * FROM Categories");
            }
        }

        public async Task<int> UpsertCategoryAsync(Category category)
        {
            using (IDbConnection db = new SqlConnection(_connectionString))
            {
                string sql = category.CategoryID == 0
                    ? "INSERT INTO Categories (CategoryName, Description) VALUES (@CategoryName, @Description)"
                    : "UPDATE Categories SET CategoryName = @CategoryName, Description = @Description WHERE CategoryID = @CategoryID";
                return await db.ExecuteAsync(sql, category);
            }
        }

        public async Task<IEnumerable<Store>> GetAllStoresAsync()
        {
            using (IDbConnection db = new SqlConnection(_connectionString))
            {
                return await db.QueryAsync<Store>("SELECT * FROM Stores");
            }
        }

        public async Task<int> UpsertStoreAsync(Store store)
        {
            using (IDbConnection db = new SqlConnection(_connectionString))
            {
                string sql = store.StoreID == 0
                    ? "INSERT INTO Stores (StoreName, Location, IsMainStore) VALUES (@StoreName, @Location, @IsMainStore)"
                    : "UPDATE Stores SET StoreName = @StoreName, Location = @Location, IsMainStore = @IsMainStore WHERE StoreID = @StoreID";
                return await db.ExecuteAsync(sql, store);
            }
        }
    }
}
