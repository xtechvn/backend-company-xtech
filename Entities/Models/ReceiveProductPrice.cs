using System;
using System.Collections.Generic;

namespace Entities.Models
{
    public partial class ReceiveProductPrice
    {
        public int Id { get; set; }
        public string? ClientEmail { get; set; }
        public int? Status { get; set; }
        public DateTime? CreatedDate { get; set; }
        public DateTime? UpdatedDate { get; set; }
    }
}
