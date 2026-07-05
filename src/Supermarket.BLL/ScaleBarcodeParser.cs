using System;

namespace Supermarket.BLL
{
    public class ScaleBarcodeParser
    {
        public static ScaleData Parse(string barcode)
        {
            // Standard Scale Barcode (Example: 21XXXXXWWWWW)
            // 21: Prefix for weight-based items
            // XXXXX: Item Code
            // WWWWW: Weight or Price

            if (barcode.Length == 13 && barcode.StartsWith("21"))
            {
                string itemCode = barcode.Substring(2, 5);
                decimal weight = decimal.Parse(barcode.Substring(7, 5)) / 1000m; // Grams to Kg

                return new ScaleData { ItemCode = itemCode, Weight = weight, IsScaleItem = true };
            }

            return new ScaleData { IsScaleItem = false };
        }
    }

    public class ScaleData
    {
        public string ItemCode { get; set; }
        public decimal Weight { get; set; }
        public bool IsScaleItem { get; set; }
    }
}
