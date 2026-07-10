using System;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using Dapper;
using Microsoft.Data.SqlClient;
using System.Text.RegularExpressions;

namespace Supermarket.DAL
{
    public static class DatabaseInitializer
    {
        public const string DefaultSchemaScript = @"
-- 1. Table: Users
CREATE TABLE Users (
    UserID INT PRIMARY KEY IDENTITY(1,1),
    Username NVARCHAR(50) NOT NULL UNIQUE,
    PasswordHash NVARCHAR(MAX) NOT NULL,
    FullName NVARCHAR(100),
    Role NVARCHAR(20) NOT NULL, -- Admin, WarehouseManager, Cashier, Accountant
    IsActive BIT DEFAULT 1,
    CreatedAt DATETIME DEFAULT GETDATE()
);

-- 2. Table: Stores (Warehouses/Branches)
CREATE TABLE Stores (
    StoreID INT PRIMARY KEY IDENTITY(1,1),
    StoreName NVARCHAR(100) NOT NULL,
    Location NVARCHAR(200),
    IsMainStore BIT DEFAULT 0,
    CreatedAt DATETIME DEFAULT GETDATE()
);

-- 3. Table: ChartOfAccounts
CREATE TABLE ChartOfAccounts (
    AccountID INT PRIMARY KEY IDENTITY(1,1),
    AccountNumber NVARCHAR(20) NOT NULL UNIQUE,
    AccountName NVARCHAR(100) NOT NULL,
    AccountType NVARCHAR(50), -- Asset, Liability, Equity, Revenue, Expense
    ParentAccountID INT NULL,
    FOREIGN KEY (ParentAccountID) REFERENCES ChartOfAccounts(AccountID)
);

-- 4. Table: Categories
CREATE TABLE Categories (
    CategoryID INT PRIMARY KEY IDENTITY(1,1),
    CategoryName NVARCHAR(100) NOT NULL,
    Description NVARCHAR(MAX)
);

-- 5. Table: Items (Products)
CREATE TABLE Items (
    ItemID INT PRIMARY KEY IDENTITY(1,1),
    Barcode NVARCHAR(50) UNIQUE,
    ItemName NVARCHAR(200) NOT NULL,
    CategoryID INT,
    Unit NVARCHAR(20),
    CostPrice DECIMAL(18, 2),
    SalePrice DECIMAL(18, 2),
    TaxRate DECIMAL(5, 2) DEFAULT 15.0, -- Default VAT 15%
    MinimumStockLevel DECIMAL(18, 2) DEFAULT 0,
    HasExpiryDate BIT DEFAULT 0,
    FOREIGN KEY (CategoryID) REFERENCES Categories(CategoryID)
);

-- 6. Table: Stock
CREATE TABLE Stock (
    StockID INT PRIMARY KEY IDENTITY(1,1),
    ItemID INT,
    StoreID INT,
    Quantity DECIMAL(18, 2) DEFAULT 0,
    ExpiryDate DATE NULL,
    FOREIGN KEY (ItemID) REFERENCES Items(ItemID),
    FOREIGN KEY (StoreID) REFERENCES Stores(StoreID)
);

-- 7. Table: StockMovement
CREATE TABLE StockMovement (
    MovementID INT PRIMARY KEY IDENTITY(1,1),
    ItemID INT,
    StoreID INT,
    MovementType NVARCHAR(50), -- Purchase, Sale, Adjustment, Waste, Return
    Quantity DECIMAL(18, 2),
    MovementDate DATETIME DEFAULT GETDATE(),
    ReferenceID INT, -- PurchaseInvoiceItemID or SalesInvoiceItemID
    Notes NVARCHAR(MAX),
    FOREIGN KEY (ItemID) REFERENCES Items(ItemID),
    FOREIGN KEY (StoreID) REFERENCES Stores(StoreID)
);

-- 8. Table: Suppliers
CREATE TABLE Suppliers (
    SupplierID INT PRIMARY KEY IDENTITY(1,1),
    SupplierName NVARCHAR(100) NOT NULL,
    ContactPerson NVARCHAR(100),
    Phone NVARCHAR(20),
    Email NVARCHAR(100),
    Address NVARCHAR(MAX),
    TaxNumber NVARCHAR(50)
);

-- 9. Table: PurchaseInvoices
CREATE TABLE PurchaseInvoices (
    PurchaseID INT PRIMARY KEY IDENTITY(1,1),
    InvoiceNumber NVARCHAR(50) UNIQUE,
    SupplierID INT,
    StoreID INT,
    InvoiceDate DATETIME DEFAULT GETDATE(),
    TotalAmount DECIMAL(18, 2),
    TaxAmount DECIMAL(18, 2),
    DiscountAmount DECIMAL(18, 2),
    NetAmount DECIMAL(18, 2),
    PaymentStatus NVARCHAR(20), -- Paid, Partial, Unpaid
    PaymentType NVARCHAR(20), -- Cash, Credit
    CreatedBy INT,
    FOREIGN KEY (SupplierID) REFERENCES Suppliers(SupplierID),
    FOREIGN KEY (StoreID) REFERENCES Stores(StoreID),
    FOREIGN KEY (CreatedBy) REFERENCES Users(UserID)
);

-- 10. Table: PurchaseInvoiceItems
CREATE TABLE PurchaseInvoiceItems (
    PurchaseItemID INT PRIMARY KEY IDENTITY(1,1),
    PurchaseID INT,
    ItemID INT,
    Quantity DECIMAL(18, 2),
    UnitPrice DECIMAL(18, 2),
    TaxAmount DECIMAL(18, 2),
    TotalAmount DECIMAL(18, 2),
    ExpiryDate DATE,
    FOREIGN KEY (PurchaseID) REFERENCES PurchaseInvoices(PurchaseID),
    FOREIGN KEY (ItemID) REFERENCES Items(ItemID)
);

-- 11. Table: Customers
CREATE TABLE Customers (
    CustomerID INT PRIMARY KEY IDENTITY(1,1),
    CustomerName NVARCHAR(100) NOT NULL,
    Phone NVARCHAR(20),
    Email NVARCHAR(100),
    TaxNumber NVARCHAR(50)
);

-- 12. Table: SalesInvoices
CREATE TABLE SalesInvoices (
    SalesID INT PRIMARY KEY IDENTITY(1,1),
    InvoiceNumber NVARCHAR(50) UNIQUE,
    CustomerID INT NULL,
    StoreID INT,
    InvoiceDate DATETIME DEFAULT GETDATE(),
    TotalAmount DECIMAL(18, 2),
    TaxAmount DECIMAL(18, 2),
    DiscountAmount DECIMAL(18, 2),
    NetAmount DECIMAL(18, 2),
    PaymentType NVARCHAR(20), -- Cash, Card, Credit
    CreatedBy INT,
    QRCode NVARCHAR(MAX), -- ZATCA Phase 1 QR
    FOREIGN KEY (CustomerID) REFERENCES Customers(CustomerID),
    FOREIGN KEY (StoreID) REFERENCES Stores(StoreID),
    FOREIGN KEY (CreatedBy) REFERENCES Users(UserID)
);

-- 13. Table: SalesInvoiceItems
CREATE TABLE SalesInvoiceItems (
    SalesItemID INT PRIMARY KEY IDENTITY(1,1),
    SalesID INT,
    ItemID INT,
    Quantity DECIMAL(18, 2),
    UnitPrice DECIMAL(18, 2),
    TaxAmount DECIMAL(18, 2),
    TotalAmount DECIMAL(18, 2),
    FOREIGN KEY (SalesID) REFERENCES SalesInvoices(SalesID),
    FOREIGN KEY (ItemID) REFERENCES Items(ItemID)
);

-- 14. Table: CashierShifts
CREATE TABLE CashierShifts (
    ShiftID INT PRIMARY KEY IDENTITY(1,1),
    UserID INT,
    StartTime DATETIME DEFAULT GETDATE(),
    EndTime DATETIME NULL,
    OpeningBalance DECIMAL(18, 2),
    ExpectedAmount DECIMAL(18, 2) DEFAULT 0,
    ActualAmount DECIMAL(18, 2) DEFAULT 0,
    DifferenceAmount DECIMAL(18, 2) DEFAULT 0,
    Status NVARCHAR(20), -- Open, Closed
    FOREIGN KEY (UserID) REFERENCES Users(UserID)
);

-- 15. Table: JournalEntries
CREATE TABLE JournalEntries (
    JournalID INT PRIMARY KEY IDENTITY(1,1),
    EntryDate DATETIME DEFAULT GETDATE(),
    ReferenceNumber NVARCHAR(50),
    Description NVARCHAR(MAX),
    CreatedBy INT,
    FOREIGN KEY (CreatedBy) REFERENCES Users(UserID)
);

-- 16. Table: JournalEntryDetails
CREATE TABLE JournalEntryDetails (
    DetailID INT PRIMARY KEY IDENTITY(1,1),
    JournalID INT,
    AccountID INT,
    Debit DECIMAL(18, 2) DEFAULT 0,
    Credit DECIMAL(18, 2) DEFAULT 0,
    Description NVARCHAR(MAX),
    FOREIGN KEY (JournalID) REFERENCES JournalEntries(JournalID),
    FOREIGN KEY (AccountID) REFERENCES ChartOfAccounts(AccountID)
);

-- 17. Table: Vouchers (Receipt/Payment)
CREATE TABLE Vouchers (
    VoucherID INT PRIMARY KEY IDENTITY(1,1),
    VoucherType NVARCHAR(20), -- Receipt, Payment
    VoucherDate DATETIME DEFAULT GETDATE(),
    Amount DECIMAL(18, 2),
    AccountID INT, -- The opposite account (e.g., Supplier or Customer)
    PaymentType NVARCHAR(20), -- Cash, Bank
    Notes NVARCHAR(MAX),
    CreatedBy INT,
    FOREIGN KEY (AccountID) REFERENCES ChartOfAccounts(AccountID),
    FOREIGN KEY (CreatedBy) REFERENCES Users(UserID)
);

-- 18. Table: Employees
CREATE TABLE Employees (
    EmployeeID INT PRIMARY KEY IDENTITY(1,1),
    EmployeeName NVARCHAR(100),
    JobTitle NVARCHAR(50),
    Salary DECIMAL(18, 2),
    HireDate DATE,
    Phone NVARCHAR(20)
);

-- 19. Table: Attendance
CREATE TABLE Attendance (
    AttendanceID INT PRIMARY KEY IDENTITY(1,1),
    EmployeeID INT,
    Date DATE DEFAULT GETDATE(),
    TimeIn TIME,
    TimeOut TIME,
    FOREIGN KEY (EmployeeID) REFERENCES Employees(EmployeeID)
);

-- 20. Table: Promotions
CREATE TABLE Promotions (
    PromotionID INT PRIMARY KEY IDENTITY(1,1),
    PromotionName NVARCHAR(100),
    ItemID INT,
    DiscountPercentage DECIMAL(5, 2),
    StartDate DATE,
    EndDate DATE,
    FOREIGN KEY (ItemID) REFERENCES Items(ItemID)
);

-- 21. Table: Localization
CREATE TABLE Localization (
    ResourceKey NVARCHAR(200) PRIMARY KEY,
    ArabicValue NVARCHAR(MAX),
    EnglishValue NVARCHAR(MAX)
);

-- 22. Table: Settings
CREATE TABLE Settings (
    SettingKey NVARCHAR(100) PRIMARY KEY,
    SettingValue NVARCHAR(MAX)
);

-- Insert basic data
INSERT INTO Users (Username, PasswordHash, FullName, Role) VALUES ('admin', 'admin123', 'Administrator', 'Admin');
INSERT INTO Stores (StoreName, IsMainStore) VALUES (N'المخزن الرئيسي', 1);
INSERT INTO Settings (SettingKey, SettingValue) VALUES ('TaxNumber', '1234567890'), ('CompanyName', 'Supermarket');

-- Insert basic Chart of Accounts
SET IDENTITY_INSERT ChartOfAccounts ON;
INSERT INTO ChartOfAccounts (AccountID, AccountNumber, AccountName, AccountType) VALUES
(1, '1101', N'الصندوق', 'Asset'),
(2, '4101', N'المبيعات', 'Revenue'),
(3, '2101', N'ضريبة المخرجات', 'Liability'),
(4, '5101', N'تكلفة البضاعة المباعة', 'Expense'),
(5, '1201', N'المخزون', 'Asset'),
(6, '1102', N'مدينون - عملاء', 'Asset'),
(7, '5102', N'تسويات مخزون', 'Expense'),
(8, '5103', N'عجز الصندوق', 'Expense'),
(9, '4102', N'زيادة الصندوق', 'Revenue'),
(10, '4103', N'مرتجعات مبيعات', 'Revenue'),
(11, '1202', N'ضريبة المدخلات', 'Asset'),
(12, '2102', N'دائنون - موردون', 'Liability'),
(13, '1103', N'البنك - بطاقة', 'Asset');
SET IDENTITY_INSERT ChartOfAccounts OFF;

INSERT INTO Suppliers (SupplierName, Phone) VALUES (N'المورد الافتراضي', '0500000000');
INSERT INTO Employees (EmployeeName, JobTitle, Salary) VALUES (N'موظف افتراضي', N'كاشير', 3000);
INSERT INTO Customers (CustomerName, Phone) VALUES (N'عميل نقدي', '0500000000');
INSERT INTO Categories (CategoryName) VALUES (N'عام');

INSERT INTO Localization (ResourceKey, ArabicValue, EnglishValue) VALUES
('lblUsername', N'اسم المستخدم', 'Username'),
('lblPassword', N'كلمة المرور', 'Password'),
('btnLogin', N'تسجيل الدخول', 'Login'),
('POS / نقطة البيع', N'نقطة البيع', 'Point of Sale'),
('Purchases / المشتريات', N'المشتريات', 'Purchases'),
('Reports / التقارير', N'التقارير', 'Reports'),
('Items / الأصناف', N'الأصناف', 'Items'),
('Settings / الإعدادات', N'الإعدادات', 'Settings');
";

        public static async Task InitializeDatabaseAsync(string connectionString, string script = null)
        {
            if (string.IsNullOrEmpty(script)) script = DefaultSchemaScript;

            // 1. Get Master Connection String
            var builder = new SqlConnectionStringBuilder(connectionString);
            string databaseName = builder.InitialCatalog;
            builder.InitialCatalog = "master";
            string masterConnectionString = builder.ConnectionString;

            using (IDbConnection masterDb = new SqlConnection(masterConnectionString))
            {
                // 2. Check if Database Exists
                var dbId = await masterDb.ExecuteScalarAsync<int?>(
                    "SELECT database_id FROM sys.databases WHERE name = @name", new { name = databaseName });

                if (dbId == null)
                {
                    // 3. Create Database
                    await masterDb.ExecuteAsync($"CREATE DATABASE [{databaseName}]");
                }

                // 4. Run Schema Script if tables are missing
                using (IDbConnection targetDb = new SqlConnection(connectionString))
                {
                    targetDb.Open();

                    // Check if a core table exists
                    var tableExists = await targetDb.ExecuteScalarAsync<int?>(
                        "SELECT 1 FROM sys.tables WHERE name = 'Users'");

                    if (tableExists == null)
                    {
                        // Split script by GO
                        var commands = Regex.Split(script, @"^\s*GO\s*$", RegexOptions.Multiline | RegexOptions.IgnoreCase);
                        foreach (var cmd in commands)
                        {
                            if (!string.IsNullOrWhiteSpace(cmd))
                            {
                                // Skip USE [Database] and CREATE DATABASE since we handle that manually or it's already there
                                string trimmedCmd = cmd.Trim();
                                if (trimmedCmd.StartsWith("USE ", StringComparison.OrdinalIgnoreCase) ||
                                    trimmedCmd.StartsWith("CREATE DATABASE ", StringComparison.OrdinalIgnoreCase))
                                    continue;

                                try
                                {
                                    await targetDb.ExecuteAsync(cmd);
                                }
                                catch (Exception ex)
                                {
                                    // Log or handle error if a specific command fails but continue with others
                                    Console.WriteLine($"Error executing script command: {ex.Message}");
                                }
                            }
                        }
                    }
                }
            }
        }
    }
}
