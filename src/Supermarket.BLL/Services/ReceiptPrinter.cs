using System;
using System.Drawing;
using System.Drawing.Printing;
using System.Collections.Generic;

namespace Supermarket.BLL.Services
{
    public class ReceiptPrinter
    {
        public void PrintReceipt(string invoiceNum, string cashier, List<ReceiptItem> items, decimal subtotal, decimal tax, decimal total, string qrCode)
        {
            PrintDocument pd = new PrintDocument();
            pd.PrintPage += (s, e) => {
                Graphics g = e.Graphics;
                g.DrawString("--- RECEIPT ---", new Font("Arial", 10), Brushes.Black, 10, 10);
                g.DrawString($"Inv: {invoiceNum}", new Font("Arial", 8), Brushes.Black, 10, 30);
                g.DrawString($"Total: {total:F2}", new Font("Arial", 10, FontStyle.Bold), Brushes.Black, 10, 50);
            };
            try { pd.Print(); } catch { /* Ignore printer errors in demo */ }
        }
    }

    public class ReceiptItem
    {
        public string Name { get; set; }
        public decimal Qty { get; set; }
        public decimal Price { get; set; }
    }
}
