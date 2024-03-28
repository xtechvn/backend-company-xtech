using System;
using System.Collections.Generic;
using System.Text;

namespace Entities.ViewModels.Report
{
    public class ReportClientDebtViewModel
    {
        public int ClientId { get; set; }
        public string ClientName { get; set; }
        public int DebtAccount { get; set; }
        public double? AmountOpeningBalanceCredit { get; set; }
        public double? AmountOpeningBalanceDebit { get; set; }
        public double? AmountDebit { get; set; }
        public double? AmountCredit { get; set; }
        public double? AmountClosingBalanceDebit { get; set; }
        public double? AmountClosingBalanceCredit { get; set; }
        public int TotalRow { get; set; }
    }
    public class SumReportClientDebtViewModel
    {
        public double? AmountOpeningBalanceCredit { get; set; }
        public double? AmountOpeningBalanceDebit { get; set; }
        public double? AmountDebit { get; set; }
        public double? AmountCredit { get; set; }
        public double? AmountClosingBalanceDebit { get; set; }
        public double? AmountClosingBalanceCredit { get; set; }
    }
}
