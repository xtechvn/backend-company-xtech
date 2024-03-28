using System;
using System.Collections.Generic;
using System.Text;

namespace Entities.ViewModels.Report
{
    public class ReportDetailClientDebtViewModel
    {
        public int ClientID { get; set; }
        public DateTime CreatedDate { get; set; }
        public string LicenceNo { get; set; }
        public string BillNo { get; set; }
        public string Description { get; set; }
        public string DebtAccount { get; set; }
        public string CorrespondingAccount { get; set; }
        public double? AmountOpeningBalance { get; set; }
        public double? AmountDebit { get; set; }
        public double? AmountCredit { get; set; }
        public double? AmountRemain { get; set; }
        public int TotalRow { get; set; }

    }
}
