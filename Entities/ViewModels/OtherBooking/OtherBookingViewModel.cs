using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Entities.ViewModels.OtherBooking
{
    public class OtherBookingViewModel
    {
        public int Id { get; set; }
        public int ServiceType { get; set; }
        public float Amount { get; set; }
        public float Price { get; set; }
        public int OperatorID { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public string Note { get; set; }
        public int Status { get; set; }
        public float OthersAmount { get; set; }
        public string OtherBookingStatusName { get; set; }
        public string ServiceTypeName { get; set; }
    }
}
