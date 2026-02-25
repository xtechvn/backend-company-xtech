using System;

namespace Entities.Models
{
    public partial class OtherBookingPackagesOptional
    {
        public long Id { get; set; }                // bigint NOT NULL
        public long BookingId { get; set; }         // bigint NOT NULL
        public string? PackageName { get; set; }    // nvarchar(100) NULL
        public int SuplierId { get; set; }          // int NOT NULL
        public double Amount { get; set; }          // float NOT NULL
        public string? Note { get; set; }           // nvarchar(1000) NULL
        public int? CreatedBy { get; set; }         // int NULL
        public DateTime? CreatedDate { get; set; }  // datetime NULL
        public int? UpdatedBy { get; set; }         // int NULL
        public DateTime? UpdatedDate { get; set; }  // datetime NULL
        public double? BasePrice { get; set; }      // float NULL
        public int? Quantity { get; set; }          // int NULL
        public int? Status { get; set; }            // int NULL
    }
}