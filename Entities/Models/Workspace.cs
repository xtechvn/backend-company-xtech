using System;
using System.Collections.Generic;

namespace Entities.Models
{
    public partial class Workspace
    {
        public long Id { get; set; }
        public string? Name { get; set; }
        public string? Description { get; set; }
        public long? CreatedBy { get; set; }
        public DateTime? CreatedDate { get; set; }
    }
}
