using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Supermarket.DAL;

using Supermarket.Models.Entities;

namespace Supermarket.BLL.Services
{
    public class BusinessFlowService
    {
        private readonly IntegratedPurchaseRepository _purchaseRepo;
        private readonly SalesRepository _salesRepo;
        private readonly ReportRepository _reportRepo;

        public BusinessFlowService(string connString)
        {
            _purchaseRepo = new IntegratedPurchaseRepository(connString);
            _salesRepo = new SalesRepository(connString);
            _reportRepo = new ReportRepository(connString);
        }

        public async Task<int> RecordPurchaseAsync(PurchaseInvoice invoice, List<PurchaseInvoiceItem> items)
        {
            return await _purchaseRepo.SavePurchaseInvoiceAsync(invoice, items);
        }

        public async Task<int> RecordSaleAsync(SalesInvoice invoice, List<SalesInvoiceItem> items)
        {
            invoice.QRCode = ZatcaHelper.GenerateQrCode("MySupermarket", "1234567890", DateTime.Now, invoice.NetAmount, invoice.TaxAmount);
            return await _salesRepo.SaveSalesInvoiceAsync(invoice, items);
        }

        public async Task<dynamic> GetFinancialSummaryAsync(DateTime from, DateTime to)
        {
            return await _reportRepo.GetProfitAndLossAsync(from, to);
        }
    }
}
