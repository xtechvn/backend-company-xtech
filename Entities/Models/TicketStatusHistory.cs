using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Entities.Models
{
    [Table("TicketStatusHistory")]
    public class TicketStatusHistory
    {
        [Key]
        public Guid Id { get; set; } = Guid.NewGuid();

        public Guid TicketId { get; set; }

        public TicketStatus FromStatus { get; set; }

        public TicketStatus ToStatus { get; set; }

        [Required]
        [MaxLength(450)]
        public string ChangedByUserId { get; set; }

        [MaxLength(500)]
        public string Note { get; set; }

        public DateTime ChangedAt { get; set; } = DateTime.UtcNow;
    }
}
