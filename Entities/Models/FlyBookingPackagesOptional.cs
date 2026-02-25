using System;

namespace Entities.Models
{
    public partial class FlyBookingPackagesOptional
    {
        public int Id { get; set; }                 // int NOT NULL
        public int? BookingId { get; set; }         // int NULL
        public int? SuplierId { get; set; }         // int NULL
        public decimal? Amount { get; set; }        // decimal(18,2) NULL
        public string? Note { get; set; }           // nvarchar(max) NULL
        public int? CreatedBy { get; set; }         // int NULL
        public DateTime? CreatedDate { get; set; }  // datetime NULL
        public int? UpdatedBy { get; set; }         // int NULL
        public DateTime? UpdatedDate { get; set; }  // datetime NULL
        public string? PackageName { get; set; }    // nvarchar(510) NULL
        public int? Status { get; set; }            // int NULL
    }
}