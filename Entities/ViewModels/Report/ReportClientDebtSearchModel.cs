using System;
using System.Collections.Generic;
using System.Text;

namespace Entities.ViewModels.Report
{
    public class ReportClientDebtSearchModel
    {
        public int? BranchCode { get; set; }
        public DateTime? FromDate { get; set; }
        public DateTime? ToDate { get; set; }
        public long? ClientID { get; set; }
        public int? PageIndex { get; set; }
        public int? PageSize { get; set; }
    }
}
