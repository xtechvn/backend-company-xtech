using System;
using System.Collections.Generic;
using System.Text;

namespace Entities.ViewModels.Report
{
    public class ReportDetailClientDebtSearchModel
    {
        public DateTime? FromDate { get; set; }
        public DateTime? ToDate { get; set; }
        public int ClientID { get; set; }
        public int PageIndex { get; set; }
        public int PageSize { get; set; }
        public double OpeningCredit { get; set; }
        public double OpeningCreditValue { get; set; }

    }
}
