using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Entities.Models
{
    [Table("Tickets")]
    public class Ticket
    {
        [Key]
        public Guid Id { get; set; } = Guid.NewGuid();

        [Required]
        [MaxLength(30)]
        public string Code { get; set; }

        public int ServiceId { get; set; }

        public int DepartmentId { get; set; }

        [Required]
        [MaxLength(300)]
        public string? Subject { get; set; }

        public TicketStatus Status { get; set; } = TicketStatus.Open;

        public int? Priority { get; set; }

        [Required]
        [MaxLength(450)]
        public string CreatedByUserId { get; set; }

        [MaxLength(450)]
        public string? AssignedAgentId { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

        public DateTime LastMessageAt { get; set; } = DateTime.UtcNow;

        [Timestamp]
        public byte[] RowVer { get; set; }
    }
}
