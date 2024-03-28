using System;
using System.Collections.Generic;
using System.Text;

namespace Entities.ViewModels.Report
{
    public class ReportEmployeeViewModel
    {
        public string ParentDepartmentName { get; set; }
        public string DepartmentName { get; set; }
        public long TotalOrder { get; set; }
        public double Amount { get; set; }
        public double Price { get; set; }
        public double Comission { get; set; }
        public double Profit { get; set; }
        public double Percent { get; set; }
        public double AmountVat { get; set; }
        public double PriceVat { get; set; }
        public double ProfitVat { get; set; }
    }
}
