using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Entities.ViewModels.InvoiceRequest
{
    public class InvoiceSearchViewModel
    {
        public string InvoiceRequestNo { get; set; }
        public DateTime PlanDateFrom { get; set; }
        public DateTime PlanDateTo { get; set;}
        public string InvoiceNo { get; set; }
        public string InvoiceCode { get; set; }
        public DateTime ExportDateFrom { get; set; }
        public DateTime ExportDateTo { get; set;}
        public string InvoiceRequestStatus { get; set; }
        public string IsHasBill { get; set; } 
        public int ClientId { get; set;}
        public string UserCreate { get; set; }
        public DateTime CreateDateFrom { get; set; }
        public DateTime CreateDateTo { get; set;}
        public string UserVerify { get; set;}
        public DateTime VerifyDateFrom { get; set;}
        public DateTime VerifyDateTo { get; set;}
        public int PageIndex { get; set; } = 1;
        public int PageSize { get; set; } = 10;
    }
}
