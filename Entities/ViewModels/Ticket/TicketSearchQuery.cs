using Entities.Models;
using ENTITIES.ViewModels.AttachFiles;
using Microsoft.AspNetCore.Http;

namespace Entities.ViewModels.Tickets
{
    public class TicketSearchQuery
    {
        public string? Q { get; set; }
        public int? Status { get; set; }
        public int? ServiceId { get; set; }
        public int? DepartmentId { get; set; }
        public int Page { get; set; } = 1;
        public int PageSize { get; set; } = 20;
    }

    public class TicketDetailDto
    {
        public Ticket Ticket { get; set; } = default!;
        public List<TicketMessage> Messages { get; set; } = new();
        public List<FileViewModel> AttachFiles { get; set; } = new List<FileViewModel>();
    }

    public class TicketIndexVm
    {
        public TicketSearchQuery Query { get; set; } = new();
        public List<TicketListItemVm> Items { get; set; } = new();
        public int Total { get; set; }
    }

    public class AddReplyCommand
    {
        public Guid TicketId { get; set; }
        public string AgentId { get; set; } = default!;
        public string? Content { get; set; }
        public string? ContentHtml { get; set; }
        // ✅ thêm
        public List<IFormFile>? AttachFiles { get; set; }
    }
}