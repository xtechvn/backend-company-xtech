using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Entities.ViewModels.TicketApi
{
    public class CreateTicketResultDto
    {
        public Guid ticket_id { get; set; }
        public string code { get; set; }
        public long first_message_id { get; set; }   // ✅ quan trọng
    }
}
