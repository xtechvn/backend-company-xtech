using System;
using System.Collections.Generic;

namespace Entities.Models
{
    public partial class Priority
    {
        public int Id { get; set; }
        public string? Name { get; set; }
        public string? Color { get; set; }
    }
}
