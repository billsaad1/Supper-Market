using System;
using System.Collections.Generic;

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

    public class Category
    {
        public int CategoryID { get; set; }
        public string CategoryName { get; set; }
        public string Description { get; set; }
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

    public class PurchaseInvoice
    {
        public int PurchaseID { get; set; }
        public string InvoiceNumber { get; set; }
        public int SupplierID { get; set; }
        public int StoreID { get; set; }
        public decimal TotalAmount { get; set; }
        public decimal TaxAmount { get; set; }
        public decimal NetAmount { get; set; }
        public int CreatedBy { get; set; }
    }

    public class PurchaseInvoiceItem
    {
        public int PurchaseItemID { get; set; }
        public int PurchaseID { get; set; }
        public int ItemID { get; set; }
        public decimal Quantity { get; set; }
        public decimal UnitPrice { get; set; }
        public decimal TaxAmount { get; set; }
        public decimal TotalAmount { get; set; }
    }

    public class SalesInvoice
    {
        public int SalesID { get; set; }
        public string InvoiceNumber { get; set; }
        public int? CustomerID { get; set; }
        public int StoreID { get; set; }
        public decimal TotalAmount { get; set; }
        public decimal TaxAmount { get; set; }
        public decimal NetAmount { get; set; }
        public string PaymentType { get; set; }
        public int CreatedBy { get; set; }
        public string QRCode { get; set; }
    }

    public class SalesInvoiceItem
    {
        public int SalesItemID { get; set; }
        public int SalesID { get; set; }
        public int ItemID { get; set; }
        public decimal Quantity { get; set; }
        public decimal UnitPrice { get; set; }
        public decimal TaxAmount { get; set; }
        public decimal TotalAmount { get; set; }
    }
}
