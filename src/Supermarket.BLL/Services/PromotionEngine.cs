using System;
using System.Collections.Generic;
using Supermarket.Models.Entities;

namespace Supermarket.BLL.Services
{
    public class PromotionEngine
    {
        public decimal CalculateDiscount(int itemId, decimal quantity, decimal unitPrice)
        {
            decimal discount = 0;

            // Percentage Discount Simulation (e.g. 10%)
            if (quantity >= 3) {
                discount += (unitPrice * quantity) * 0.10m;
            }

            return discount;
        }

        public decimal ApplyBogo(int itemId, decimal quantity, decimal unitPrice)
        {
            // Buy 1 Get 1 Free logic
            // For every 2 items, 1 is free
            int freeItems = (int)(quantity / 2);
            return freeItems * unitPrice;
        }
    }
}
