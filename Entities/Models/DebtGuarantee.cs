using System;
using System.Collections.Generic;

namespace Entities.Models
{
    public partial class DebtGuarantee
    {
        public int Id { get; set; }
        public string Code { get; set; }
        public int? Orderid { get; set; }
        public int? ClientId { get; set; }
        public int? Status { get; set; }
        public int? CreatedBy { get; set; }
        public DateTime? CreatedDate { get; set; }
        public int? UpdatedBy { get; set; }
        public DateTime? UpdatedDate { get; set; }
    }
}
