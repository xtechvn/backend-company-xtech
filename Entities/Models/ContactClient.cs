using System;
using System.Collections.Generic;

namespace Entities.Models
{
    public partial class ContactClient
    {
        public ContactClient()
        {
            Orders = new HashSet<Order>();
        }

        public long Id { get; set; }
        public string Name { get; set; } = null!;
        public string Mobile { get; set; } = null!;
        public string? Email { get; set; }
        public DateTime CreateDate { get; set; }
        public long ClientId { get; set; }
        public long? OrderId { get; set; }

        public virtual ICollection<Order> Orders { get; set; }
    }
}
