using System;
using System.Collections.Generic;

namespace Entities.Models
{
    public partial class OtherBookingPackagesOptional
    {
        public long Id { get; set; }
        public long BookingId { get; set; }
        public string PackageName { get; set; }
        public int SuplierId { get; set; }
        public double Amount { get; set; }
        public string Note { get; set; }
        public int? CreatedBy { get; set; }
        public DateTime? CreatedDate { get; set; }
        public int? UpdatedBy { get; set; }
        public DateTime? UpdatedDate { get; set; }
        public double? BasePrice { get; set; }
        public int? Quantity { get; set; }
        public int? Status { get; set; }
    }
}
