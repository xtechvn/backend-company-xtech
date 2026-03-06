using System;
using System.Collections.Generic;

namespace Entities.Models
{
    public partial class TaskHistory
    {
        public long Id { get; set; }
        public long? TaskId { get; set; }
        public int? FromStatusId { get; set; }
        public int? ToStatusId { get; set; }
        public long? ChangedBy { get; set; }
        public DateTime? ChangedDate { get; set; }
    }
}
