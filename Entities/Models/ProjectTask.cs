using System;
using System.Collections.Generic;

namespace Entities.Models
{
    public partial class ProjectTask
    {
        public long Id { get; set; }
        public string Title { get; set; } = null!;
        public string? Description { get; set; }
        public int? AssigneeId { get; set; }
        public int? ReporterId { get; set; }
        public int? PriorityId { get; set; }
        public long? SprintId { get; set; }
        public int? StoryPoint { get; set; }
        public DateTime? DueDate { get; set; }
        public string? Label { get; set; }
        public string? Attachment { get; set; }
        public int? TaskOrder { get; set; }
        public int? CreatedBy { get; set; }
        public DateTime? CreatedDate { get; set; }
        public int? ModifiedBy { get; set; }
        public DateTime? ModifiedDate { get; set; }
        public int StatusId { get; set; }
        public long? ProjectId { get; set; }
        public int? TaskType { get; set; }


    }
}
