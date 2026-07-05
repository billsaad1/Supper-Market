using System;
using System.Collections.Generic;
using System.Text;

namespace Supermarket.BLL.Services
{
    public static class ZatcaHelper
    {
        public static string GenerateQrCode(string sellerName, string taxNumber, DateTime timestamp, decimal totalAmount, decimal taxAmount)
        {
            // Simplified TLV encoding for ZATCA Phase 1
            byte[] tag1 = GetTlv(1, sellerName);
            byte[] tag2 = GetTlv(2, taxNumber);
            byte[] tag3 = GetTlv(3, timestamp.ToString("yyyy-MM-ddTHH:mm:ssZ"));
            byte[] tag4 = GetTlv(4, totalAmount.ToString("F2"));
            byte[] tag5 = GetTlv(5, taxAmount.ToString("F2"));

            List<byte> fullTlv = new List<byte>();
            fullTlv.AddRange(tag1);
            fullTlv.AddRange(tag2);
            fullTlv.AddRange(tag3);
            fullTlv.AddRange(tag4);
            fullTlv.AddRange(tag5);

            return Convert.ToBase64String(fullTlv.ToArray());
        }

        private static byte[] GetTlv(int tag, string value)
        {
            byte[] valueBytes = Encoding.UTF8.GetBytes(value);
            byte[] tlv = new byte[2 + valueBytes.Length];
            tlv[0] = (byte)tag;
            tlv[1] = (byte)valueBytes.Length;
            Array.Copy(valueBytes, 0, tlv, 2, valueBytes.Length);
            return tlv;
        }
    }
}
