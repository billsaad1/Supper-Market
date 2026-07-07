using System;
using System.Drawing;
using System.Drawing.Printing;

namespace Supermarket.BLL.Services
{
    public class BarcodeLabelPrinter
    {
        private string _itemName;
        private string _barcode;
        private decimal _price;

        public void PrintLabel(string itemName, string barcode, decimal price)
        {
            _itemName = itemName;
            _barcode = barcode;
            _price = price;

            PrintDocument pd = new PrintDocument();
            // Standard label size 40mm x 25mm approx
            pd.DefaultPageSettings.PaperSize = new PaperSize("Label", 150, 100);
            pd.PrintPage += Pd_PrintPage;
            pd.Print();
        }

        private void Pd_PrintPage(object sender, PrintPageEventArgs e)
        {
            Graphics g = e.Graphics;
            Font fSmall = new Font("Arial", 8);
            Font fLarge = new Font("Arial", 10, FontStyle.Bold);

            g.DrawString(_itemName, fSmall, Brushes.Black, 10, 10);
            g.DrawString($"Price: {_price:F2} ريال", fLarge, Brushes.Black, 10, 30);

            // Barcode representation (simplified)
            g.DrawRectangle(Pens.Black, 10, 55, 130, 25);
            g.DrawString(_barcode, fSmall, Brushes.Black, 40, 80);
        }
    }
}
