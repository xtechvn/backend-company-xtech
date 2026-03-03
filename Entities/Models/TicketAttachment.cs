using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Entities.Models
{
    [Table("TicketAttachments")]
    public class TicketAttachment
    {
        [Key]
        public Guid Id { get; set; } = Guid.NewGuid();

        public Guid MessageId { get; set; }

        [Required]
        [MaxLength(255)]
        public string? FileName { get; set; }

        [Required]
        [MaxLength(1000)]
        public string? FileUrl { get; set; }

        [MaxLength(100)]
        public string? ContentType { get; set; }

        public long? Size { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}
