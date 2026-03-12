using System;
using System.Collections.Generic;

namespace Entities.Models
{
    public partial class TaskComment
    {
        public long Id { get; set; }
        public long? TaskId { get; set; }
        public long? UserId { get; set; }
        public string? Content { get; set; }
        public DateTime? CreatedDate { get; set; }
    }
}
