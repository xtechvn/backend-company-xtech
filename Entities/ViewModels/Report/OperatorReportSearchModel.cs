using System;
using System.Collections.Generic;
using System.Text;

namespace Entities.ViewModels.Report
{
    public class OperatorReportSearchModel
    {
        public DateTime? FromDate { get; set; }
        public DateTime? ToDate { get; set; }
        public DateTime? StartDateFrom { get; set; }
        public DateTime? StartDateTo { get; set; }
        public DateTime? EndDateFrom { get; set; }
        public DateTime? EndDateTo { get; set; }
        public string OrderStatus { get; set; }
        public string InvoiceStatus { get; set; }
        public DateTime? ExportDateFrom { get; set; }
        public DateTime? ExportDateTo { get; set; }
        public int? SalerId { get; set; }
        public int? DepartmentId { get; set; }
        public string DepartmentIdSearch { get; set; }
        public string SalerPermission { get; set; }
        public int? Branch { get; set; }
    }
}
