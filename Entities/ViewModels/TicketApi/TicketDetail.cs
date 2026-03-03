using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Entities.ViewModels.TicketApi
{
    public class TicketDetailDtoApi
    {
        public TicketDto ticket { get; set; }
        public List<TicketMessageDtoApi> messages { get; set; } = new();
    }

    public class TicketDto
    {
        public Guid id { get; set; }
        public string code { get; set; }
        public int serviceId { get; set; }
        public int departmentId { get; set; }
        public string subject { get; set; }
        public int status { get; set; }
        public int? priority { get; set; }
        public string createdByUserId { get; set; }
        public string assignedAgentId { get; set; }
        public DateTime createdAt { get; set; }
        public DateTime updatedAt { get; set; }
        public DateTime lastMessageAt { get; set; }
    }

    public class TicketMessageDtoApi
    {
        public long id { get; set; }
        public Guid ticketId { get; set; }
        public string senderType { get; set; } // "Customer" | "Agent"
        public string senderId { get; set; }
        public string content { get; set; }
        public string contentHtml { get; set; }
        public DateTime createdAt { get; set; }
        // Danh sách file đính kèm
        public List<AttachFileViewModel> AttachFiles { get; set; } = new List<AttachFileViewModel>();
    }

}
