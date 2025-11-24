using System;

namespace Entities.Models
{
    public partial class OtherBooking
    {
        public long Id { get; set; }

        public long OrderId { get; set; }            // có thể NULL
        public int? ServiceType { get; set; }         // có thể NULL
        public int? Status { get; set; }              // có thể NULL

        public string ServiceCode { get; set; }       // string => tự cho NULL ok

        public double Amount { get; set; }           // DB có thể NULL
        public double Profit { get; set; }           // DB có thể NULL

        public DateTime StartDate { get; set; }      // DB có thể NULL
        public DateTime EndDate { get; set; }        // DB có thể NULL

        public double? Price { get; set; }
        public int? SupplierId { get; set; }

        public string? Note { get; set; }

        public int? UpdatedBy { get; set; }
        public DateTime? UpdatedDate { get; set; }

        public int? OperatorId { get; set; }

        public int? CreatedBy { get; set; }
        public DateTime? CreatedDate { get; set; }

        public int? StatusOld { get; set; }

        public double? Commission { get; set; }
        public double? OthersAmount { get; set; }

        public string? ConfNo { get; set; }
        public string? SerialNo { get; set; }
        public string? RoomNo { get; set; }
    }
}
