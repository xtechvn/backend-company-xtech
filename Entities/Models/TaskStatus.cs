using System;
using System.Collections.Generic;

namespace Entities.Models
{
    public partial class TaskStatus
    {
        public int Id { get; set; }
        public long? BoardId { get; set; }
        public string? Name { get; set; }
        public int? SortOrder { get; set; }
    }
}
