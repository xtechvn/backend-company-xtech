using System;
using System.Collections.Generic;
using System.Text;

namespace Entities.ViewModels.Report
{
    public class SearchReportEmployeeViewModel
    {
        public long DepartmentId { get; set; }
        public string SalerId { get; set; }
        public long Vat { get; set; } = 0;
        public string FromDate { get; set; }
        public string ToDate { get; set; }
        public string SalerPermission { get; set; }
        public int? PageIndex { get; set; }
        public int? PageSize { get; set; }
    }
}
