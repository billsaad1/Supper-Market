using System.Collections.Generic;

namespace Supermarket.BLL.Services
{
    public static class PermissionsManager
    {
        private static Dictionary<string, List<string>> _rolePermissions = new Dictionary<string, List<string>>
        {
            { "Admin", new List<string> { "POS", "Purchases", "Reports", "Settings", "Users", "HR", "Accounting" } },
            { "Cashier", new List<string> { "POS", "Shifts" } },
            { "WarehouseManager", new List<string> { "Purchases", "Items", "Categories", "Adjustments" } },
            { "Accountant", new List<string> { "Accounting", "Reports", "Vouchers" } }
        };

        public static bool CanAccess(string role, string module)
        {
            if (_rolePermissions.ContainsKey(role))
            {
                return _rolePermissions[role].Contains(module);
            }
            return false;
        }
    }
}
