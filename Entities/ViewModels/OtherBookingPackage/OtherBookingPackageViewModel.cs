using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Entities.ViewModels.OtherBookingPackage
{
    public class OtherBookingPackageViewModel
    {
        public int Id { get; set; }
        public int BookingId { get; set; }
        public string Name { get; set; }
        public decimal BasePrice { get; set; }
        public decimal Amount { get; set; }
        public int Quantity { get; set; }
        public float Profit { get; set; }
        public float SalePrice { get; set; }
        public int ServiceType { get; set; }
        public string Note { get; set; }
    }
}
