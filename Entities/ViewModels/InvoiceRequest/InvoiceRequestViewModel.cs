using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace Entities.ViewModels.InvoiceRequest
{
    public class InvoiceRequestViewModel
    {
        public int Id { get; set; }
        public string InvoiceRequestNo { get; set; }
        public string DeclineReason { get; set; }
        public string OrderNo { get; set; }
        public int OrderId { get; set; }
        public int Status { get; set; }
        public string InvoiceRequestStatus { get; set; }
        public string ClientName { get; set; }
        public int ClientId { get; set; }
        public long Price { get; set; }
        public long TotalPrice { get; set; }
        public long PriceExtra { get; set; }
        public DateTime ExportDate { get; set; }
        public long PriceExtraExport { get; set; }
        public int VAT { get; set; }
        public string Note { get; set; }
        public int InvoiceRequestDetailId { get; set; }
        public string AttachFile { get; set; }
        public int Quantity { get; set; }
        public DateTime CreatedDate { get; set; }
        public DateTime PlanDate { get; set;}
        public DateTime VerifyDate { get; set; }
        public string UserVerifyName { get; set; }
        public string Unit { get; set; }
        public string TaxNo { get; set; }
        public string ProductName { get; set; }
        public string CompanyName { get; set;}
        public string Address { get; set; }
        public string UserName { get; set; }
        public int CreatedBy { get; set; }
    }
}
