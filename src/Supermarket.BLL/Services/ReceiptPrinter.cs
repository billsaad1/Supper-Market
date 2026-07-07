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
            pd.DefaultPageSettings.PaperSize = new PaperSize("Receipt", 300, 800);
            pd.PrintPage += (s, e) => {
                Graphics g = e.Graphics;
                Font fTitle = new Font("Segoe UI", 12, FontStyle.Bold);
                Font fNormal = new Font("Segoe UI", 9);
                Font fSmall = new Font("Segoe UI", 8);

                int center = 150;
                StringFormat sf = new StringFormat { Alignment = StringAlignment.Center };
                StringFormat rf = new StringFormat { Alignment = StringAlignment.Far };

                g.DrawString("SUPERMARKET SYSTEM", fTitle, Brushes.Black, center, 20, sf);
                g.DrawString("نظام السوبر ماركت المتكامل", fNormal, Brushes.Black, center, 45, sf);

                g.DrawString(new string('-', 40), fNormal, Brushes.Black, center, 65, sf);

                g.DrawString($"Invoice: {invoiceNum}", fSmall, Brushes.Black, 20, 85);
                g.DrawString($"Date: {DateTime.Now:yyyy-MM-dd HH:mm}", fSmall, Brushes.Black, 20, 100);
                g.DrawString($"Cashier: {cashier}", fSmall, Brushes.Black, 20, 115);

                g.DrawString(new string('=', 40), fNormal, Brushes.Black, center, 135, sf);

                int y = 155;
                foreach(var item in items) {
                    g.DrawString(item.Name, fNormal, Brushes.Black, 20, y);
                    g.DrawString($"{item.Qty} x {item.Price:F2}", fSmall, Brushes.Black, 20, y + 18);
                    g.DrawString($"{item.Price * item.Qty:F2}", fNormal, Brushes.Black, 280, y, rf);
                    y += 40;
                }

                g.DrawString(new string('-', 40), fNormal, Brushes.Black, center, y + 10, sf);
                y += 30;

                g.DrawString("Subtotal / المجموع:", fNormal, Brushes.Black, 20, y);
                g.DrawString($"{subtotal:F2} ريال", fNormal, Brushes.Black, 280, y, rf);

                g.DrawString("VAT 15% / الضريبة:", fNormal, Brushes.Black, 20, y + 20);
                g.DrawString($"{tax:F2} ريال", fNormal, Brushes.Black, 280, y + 20, rf);

                g.DrawString("TOTAL / الإجمالي:", fTitle, Brushes.Black, 20, y + 45);
                g.DrawString($"{total:F2} ريال", fTitle, Brushes.Black, 280, y + 45, rf);

                y += 80;
                // QR Code Placeholder (Draw a box if no real QR gen)
                g.DrawRectangle(Pens.Black, center - 40, y, 80, 80);
                g.DrawString("ZATCA QR", fSmall, Brushes.Black, center, y + 30, sf);

                y += 100;
                g.DrawString("Thank you for your visit!", fNormal, Brushes.Black, center, y, sf);
                g.DrawString("شكراً لزيارتكم!", fNormal, Brushes.Black, center, y + 20, sf);
            };
            try { pd.Print(); } catch { /* Ignore printer errors in demo */ }
        }

        public void PrintPurchaseInvoice(string invNum, string supplier, string store, List<ReceiptItem> items, decimal total)
        {
            PrintDocument pd = new PrintDocument();
            pd.DefaultPageSettings.PaperSize = new PaperSize("A4", 827, 1169);
            pd.PrintPage += (s, e) => {
                Graphics g = e.Graphics;
                Font fTitle = new Font("Segoe UI", 16, FontStyle.Bold);
                Font fHeader = new Font("Segoe UI", 11, FontStyle.Bold);
                Font fNormal = new Font("Segoe UI", 10);

                g.DrawString("PURCHASE INVOICE / فاتورة مشتريات", fTitle, Brushes.Black, 250, 50);
                g.DrawString($"Invoice #: {invNum}", fNormal, Brushes.Black, 50, 100);
                g.DrawString($"Supplier: {supplier}", fNormal, Brushes.Black, 50, 120);
                g.DrawString($"Store: {store}", fNormal, Brushes.Black, 50, 140);
                g.DrawString($"Date: {DateTime.Now:yyyy-MM-dd}", fNormal, Brushes.Black, 600, 100);

                int y = 180;
                g.DrawRectangle(Pens.Black, 50, y, 700, 30);
                g.DrawString("Item / الصنف", fHeader, Brushes.Black, 60, y + 5);
                g.DrawString("Qty", fHeader, Brushes.Black, 400, y + 5);
                g.DrawString("Price", fHeader, Brushes.Black, 500, y + 5);
                g.DrawString("Total", fHeader, Brushes.Black, 650, y + 5);

                y += 40;
                foreach(var item in items) {
                    g.DrawString(item.Name, fNormal, Brushes.Black, 60, y);
                    g.DrawString(item.Qty.ToString(), fNormal, Brushes.Black, 400, y);
                    g.DrawString(item.Price.ToString("F2"), fNormal, Brushes.Black, 500, y);
                    g.DrawString((item.Qty * item.Price).ToString("F2"), fNormal, Brushes.Black, 650, y);
                    y += 25;
                }

                g.DrawLine(Pens.Black, 50, y + 10, 750, y + 10);
                g.DrawString($"GRAND TOTAL: {total:F2} ريال", fHeader, Brushes.Black, 550, y + 30);
            };
            try { pd.Print(); } catch { }
        }
    }

    public class ReceiptItem
    {
        public string Name { get; set; }
        public decimal Qty { get; set; }
        public decimal Price { get; set; }
    }
}
