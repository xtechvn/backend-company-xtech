using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Entities.ViewModels.TicketApi
{
    public class TicketListResponse
    {
        public int page { get; set; }
        public int size { get; set; }
        public int total { get; set; }
        public List<TicketListItemDto> items { get; set; } = new();
    }

    public class TicketListItemDto
    {
        public Guid id { get; set; }
        public string code { get; set; } = "";
        public int serviceId { get; set; }
        public string? serviceName { get; set; }

        public int departmentId { get; set; }
        public string? departmentName { get; set; }

        public string subject { get; set; } = "";
        public int status { get; set; }

        public string? assignedAgentId { get; set; }
        public DateTime? lastMessageAt { get; set; }
    }
}
