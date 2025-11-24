using System;
using System.Collections.Generic;
using System.Text;

namespace Entities.ViewModels.SetServices
{
    public class SetServiceFlyBookingDetailViewModel
    {
        public double BaseTotalAmount { get; set; }
        public double BaseSalerTotalAmount { get; set; }
        public double OperatorOrderTotalAmount { get; set; }
        public double OperatorOrderProfit { get; set; }
        public string UserCreate { get; set; }
        public string UserUpdate { get; set; }
        public DateTime UpdatedDate { get; set; }
        public long SuplierId { get; set; }
        public string SuplierName { get; set; }
        public double TotalSalerProfit { get; set; }
        public double TotalOrderAmount { get; set; }
        public double Refund { get; set; }

    }
}
