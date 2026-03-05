using System;
using System.Collections.Generic;

namespace Entities.Models
{
    public partial class Sprint
    {
        public long Id { get; set; }
        public string? SprintName { get; set; }
        public string? Goal { get; set; }
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public int? Status { get; set; }
        public DateTime? CreatedDate { get; set; }
        public long? CreatedBy { get; set; }
        public long? ProjectId { get; set; }
        public long? ModifiedBy { get; set; }
        public DateTime? ModifiedDate { get; set; }

    }
}
