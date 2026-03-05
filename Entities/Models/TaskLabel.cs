using System;
using System.Collections.Generic;

namespace Entities.Models
{
    public partial class TaskLabel
    {
        public long? TaskId { get; set; }
        public long? LabelId { get; set; }
    }
}
