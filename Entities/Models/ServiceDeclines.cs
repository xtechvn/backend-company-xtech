using System;
using System.Collections.Generic;

namespace Entities.Models
{
    public partial class ServiceDeclines
    {
        public long Id { get; set; }
        public long? OrderId { get; set; }
        public string ServiceId { get; set; }
        public int? Type { get; set; }
        public string Note { get; set; }
        public int? CreatedBy { get; set; }
        public DateTime? CreatedDate { get; set; }
        public int? UpdatedBy { get; set; }
        public DateTime? UpdatedDate { get; set; }
    }
}
