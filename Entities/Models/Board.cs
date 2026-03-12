using System;
using System.Collections.Generic;

namespace Entities.Models
{
    public partial class Board
    {
        public long Id { get; set; }
        public long? ProjectId { get; set; }
        public string? Name { get; set; }
        public string? Type { get; set; }
        public DateTime? CreatedDate { get; set; }
    }
}
