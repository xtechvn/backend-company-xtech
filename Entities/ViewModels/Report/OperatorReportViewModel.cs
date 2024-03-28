using System;
using System.Collections.Generic;
using System.Text;

namespace Entities.ViewModels.Report
{
    public class OperatorReportViewModel
    {
        public string ParentDepartmentName { get; set; }
        public string DepartmentName { get; set; }
        public string OrderNo { get; set; }
        public long OrderId { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public double Amount { get; set; }
        public double? AmountPay { get; set; }
        public double? AmountRemain { get; set; }
        public double? Price { get; set; }
        public double? Comission { get; set; }
        public double? Profit { get; set; }
        public string InvoiceNo { get; set; }
        public DateTime? ExportDate { get; set; }
        public string BankId { get; set; }
        public string AccountNumber { get; set; }
        public int TotalRow { get; set; }
        public int ParentDepartmentId { get; set; }
        public int DepartmentId { get; set; }
        public string OperatorName { get; set; }
        public string FullName { get; set; }
        public string OrderStatusName { get; set; }
        public double? PricePay { get; set; }
        public double? PriceRemain { get; set; }
        public long ClientId { get; set; }
        public string Label { get; set; }
        public string BranchName { get; set; }
        public string ClientCode { get; set; }
        public string ClientName { get; set; }

    }
    public class SumOperatorReportViewModel
    {
       
        public double AMOUNT { get; set; }
        public double? AmountPay { get; set; }
        public double? AmountRemain { get; set; }
        public double? Price { get; set; }
        public double? PricePay { get; set; }
        public double? PriceRemain { get; set; }
        public double? Comission { get; set; }
        public double? Profit { get; set; }

    }
}
