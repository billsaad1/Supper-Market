using System;
using System.Collections.Generic;
using Supermarket.Models.Entities;

namespace Supermarket.BLL
{
    public class PromotionEngine
    {
        public decimal CalculateDiscount(int itemId, decimal quantity, decimal unitPrice)
        {
            // Simple Logic: 10% discount for demonstration
            // Real logic would fetch from Promotions table
            if (quantity >= 5)
                return (unitPrice * quantity) * 0.10m;

            return 0;
        }

        public void ApplyBuyOneGetOne(int itemId, ref decimal quantity)
        {
            // logic for BOGO
        }
    }
}
