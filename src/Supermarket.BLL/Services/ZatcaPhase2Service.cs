using System;
using System.Collections.Generic;
using System.Text;
using System.Xml;

namespace Supermarket.BLL.Services
{
    public class ZatcaPhase2Service
    {
        public string GenerateInvoiceXml(string invoiceNumber, DateTime issueDate, decimal netAmount, decimal taxAmount, string sellerName, string sellerVat)
        {
            // Simple UBL 2.1 XML Generator for ZATCA
            XmlDocument doc = new XmlDocument();

            // Note: This is a simplified skeleton of UBL 2.1
            XmlElement invoice = doc.CreateElement("Invoice", "urn:oasis:names:specification:ubl:schema:xsd:Invoice-2");
            invoice.SetAttribute("xmlns:cac", "urn:oasis:names:specification:ubl:schema:xsd:CommonAggregateComponents-2");
            invoice.SetAttribute("xmlns:cbc", "urn:oasis:names:specification:ubl:schema:xsd:CommonBasicComponents-2");

            XmlElement id = doc.CreateElement("cbc", "ID", "urn:oasis:names:specification:ubl:schema:xsd:CommonBasicComponents-2");
            id.InnerText = invoiceNumber;
            invoice.AppendChild(id);

            XmlElement date = doc.CreateElement("cbc", "IssueDate", "urn:oasis:names:specification:ubl:schema:xsd:CommonBasicComponents-2");
            date.InnerText = issueDate.ToString("yyyy-MM-dd");
            invoice.AppendChild(date);

            // Seller Info
            XmlElement party = doc.CreateElement("cac", "AccountingSupplierParty", "urn:oasis:names:specification:ubl:schema:xsd:CommonAggregateComponents-2");
            XmlElement partyName = doc.CreateElement("cbc", "RegistrationName", "urn:oasis:names:specification:ubl:schema:xsd:CommonBasicComponents-2");
            partyName.InnerText = sellerName;
            party.AppendChild(partyName);
            invoice.AppendChild(party);

            // Total Amount
            XmlElement legalMonetaryTotal = doc.CreateElement("cac", "LegalMonetaryTotal", "urn:oasis:names:specification:ubl:schema:xsd:CommonAggregateComponents-2");
            XmlElement payableAmount = doc.CreateElement("cbc", "PayableAmount", "urn:oasis:names:specification:ubl:schema:xsd:CommonBasicComponents-2");
            payableAmount.SetAttribute("currencyID", "SAR");
            payableAmount.InnerText = (netAmount + taxAmount).ToString("F2");
            legalMonetaryTotal.AppendChild(payableAmount);
            invoice.AppendChild(legalMonetaryTotal);

            doc.AppendChild(invoice);

            return doc.OuterXml;
        }

        public bool SendToZatca(string xml)
        {
            // Logic for ZATCA API Integration (Compliance or Clearance API)
            // 1. Hash the XML
            // 2. Digital Signature
            // 3. Post to ZATCA endpoint
            return true;
        }
    }
}
