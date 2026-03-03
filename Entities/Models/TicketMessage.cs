using Entities.ViewModels.TicketApi;
using ENTITIES.ViewModels.AttachFiles;
using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Entities.Models
{
    [Table("TicketMessages")]
    public class TicketMessage
    {
        [Key]
        public long Id { get; set; } 

        public Guid TicketId { get; set; }

        public MessageSenderType SenderType { get; set; }
        //public List<AttachFileViewModel> AttachFiles { get; set; } = new List<AttachFileViewModel>();

        [Required]
        [MaxLength(450)]
        public string SenderId { get; set; }

        public string? Content { get; set; }

        public string? ContentHtml { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        [NotMapped]
        public List<AttachFileViewModel> AttachFiles { get; set; } = new();
    }
}
