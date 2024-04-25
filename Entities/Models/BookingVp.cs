using System;
using System.Collections.Generic;

namespace Entities.Models
{
    public partial class BookingVp
    {
        public int Id { get; set; }
        public string? Mem { get; set; }
        public string? Cpu { get; set; }
        public string? Ssd { get; set; }
        public string? Net { get; set; }
        public string? Nip { get; set; }
        public string? Nmonth { get; set; }
        public string? Quantity { get; set; }
        public int? ClientId { get; set; }
        public double? Amount { get; set; }
        public int? CreatedBy { get; set; }
        public DateTime? CreatedDate { get; set; }
        public int? UpdatedBy { get; set; }
        public DateTime? UpdatedDate { get; set; }
    }
}
