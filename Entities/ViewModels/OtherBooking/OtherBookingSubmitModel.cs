using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Entities.ViewModels.OtherBooking
{
    public class OtherBookingSubmitModel
    {
        public int? Id { get; set; }
        public int? OrderId { get; set; }
        public int ServiceType { get; set; }
        public int? Status { get; set; }
        public string ServiceCode { get; set; }
        public float Amount { get; set; }
        public float OthersAmount { get; set; }
        public int OperatorID { get; set; }
        public float Profit { get; set; }
        public string Note { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
    }
}
