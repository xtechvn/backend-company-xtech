using System;
using System.Collections.Generic;

namespace Entities.Models
{
    public partial class Projects
    {
        public long Id { get; set; }
        public string ProjectName { get; set; }
        public string Description { get; set; }
        public long? CreatedBy { get; set; }
        public DateTime? CreatedDate { get; set; }


    }
}
