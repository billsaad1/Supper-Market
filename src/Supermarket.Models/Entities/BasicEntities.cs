using System;

namespace Supermarket.Models.Entities
{
    public class User
    {
        public int UserID { get; set; }
        public string Username { get; set; }
        public string PasswordHash { get; set; }
        public string FullName { get; set; }
        public string Role { get; set; }
        public bool IsActive { get; set; }
        public DateTime CreatedAt { get; set; }
    }

    public class Store
    {
        public int StoreID { get; set; }
        public string StoreName { get; set; }
        public string Location { get; set; }
        public bool IsMainStore { get; set; }
    }

    public class Item
    {
        public int ItemID { get; set; }
        public string Barcode { get; set; }
        public string ItemName { get; set; }
        public int? CategoryID { get; set; }
        public string Unit { get; set; }
        public decimal CostPrice { get; set; }
        public decimal SalePrice { get; set; }
        public decimal TaxRate { get; set; }
        public decimal MinimumStockLevel { get; set; }
        public bool HasExpiryDate { get; set; }
    }

    public class LocalizationResource
    {
        public string ResourceKey { get; set; }
        public string ArabicValue { get; set; }
        public string EnglishValue { get; set; }
    }
}
