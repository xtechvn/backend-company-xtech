using Entities.Models;
using Entities.ViewModels.TicketApi;
using Entities.ViewModels.Tickets;

namespace Repositories.IRepositories
{
    public interface ITicketRepository
    {
        Task<(List<Ticket> Items, int Total)> SearchAsync(TicketSearchQuery query);
        Task<TicketDetailDto?> GetDetailAsync(Guid ticketId);
        Task<TicketMessageDto> AddReplyAsync(AddReplyCommand cmd);
        Task UpdateStatusAsync(Guid ticketId, TicketStatus status);
        Task AssignAsync(Guid ticketId, string agentId);


        //API ==========================================================

        Task<CreateTicketResultDto> CreateTicketWithFirstMessage(
    string userId, int serviceId, int departmentId, string subject, string content, int priority);

        Task<Guid> GetTicketIdByMessageId(long messageId);
            Task<TicketListResponse> GetMyTickets(string userId, int status, int page, int size);
            Task<TicketListResponse> GetTickets(int status, string keyword, int page, int size);
            Task<TicketDetailDtoApi> CreateTicket(string userId, int serviceId, int departmentId, string subject, string content, int priority);
            Task<TicketDetailDtoApi> GetTicketDetail(Guid ticketId);
            Task<TicketMessageDtoApi> AddMessage(Guid ticketId, string senderType, string senderId, string content, string contentHtml);
        Task<int> InsertMessageAttachments(long messageId, List<AttachFileViewModel> files);

    }
}
