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
                Font fTitle = new Font("Arial", 12, FontStyle.Bold);
                Font fNormal = new Font("Arial", 10);

                g.DrawString("--- RECEIPT / إيصال ---", fTitle, Brushes.Black, 20, 20);
                g.DrawString($"Inv: {invoiceNum}", fNormal, Brushes.Black, 20, 50);
                g.DrawString($"Cashier: {cashier}", fNormal, Brushes.Black, 20, 70);

                int y = 100;
                foreach(var item in items) {
                    g.DrawString($"{item.Name} x{item.Qty}", fNormal, Brushes.Black, 20, y);
                    g.DrawString($"{item.Price * item.Qty:F2} ريال", fNormal, Brushes.Black, 200, y);
                    y += 20;
                }

                g.DrawString($"Total: {total:F2} ريال", fTitle, Brushes.Black, 20, y + 20);
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
