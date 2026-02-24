using System;
using System.Collections.Generic;
using System.Text;

namespace Entities.ViewModels.SetServices
{
    public class OtherBookingSearchViewModel
    {
        public long Id { get; set; }
        public string OrderNo { get; set; }
        public int OrderStatus { get; set; }
        public string OrderStatusName { get; set; }
        public string ServiceCode { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public DateTime CreatedDate { get; set; }
        public string SalerName { get; set; }
        public string OperatorName { get; set; }
        public string ServiceTypeName { get; set; }
        public int OtherBookingStatus { get; set; }
        public int CreatedBy { get; set; }
        public string OtherBookingStatusName { get; set; }
       
        public long OrderId { get; set; }
        public long ServiceOperatorID { get; set; }
        public long OperatorId { get; set; }
        public string DepartmentName { get; set; }
        public string BookingCode { get; set; }
        public long TotalRow { get; set; }
        public double? Amount { get; set; }
        public double? Price { get; set; }

        public string SalerFullName { get; set; }
        public string OperatorFullNameName { get; set; }

    }
}
