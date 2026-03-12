using System;
using System.Collections.Generic;

namespace Entities.Models
{
    public partial class OtherBookingPackage
    {
        public long Id { get; set; }
        public long BookingId { get; set; }
        public string Name { get; set; } = null!;
        public decimal BasePrice { get; set; }
        public decimal Amount { get; set; }
        public int Quantity { get; set; }
        public double? Profit { get; set; }
        public double? SalePrice { get; set; }
        public int? UpdatedBy { get; set; }
        public DateTime? UpdatedDate { get; set; }
        public int? ServiceType { get; set; }
        public string? Note { get; set; }
        public decimal? Commission { get; set; }
    }
}
