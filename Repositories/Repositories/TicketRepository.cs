using DAL;
using Entities.ConfigModels;
using Entities.Models;
using Entities.ViewModels.TicketApi;
using Entities.ViewModels.Tickets;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;
using Repositories.IRepositories;
using Repositories.Repositories.BaseRepos;
using Ultilities.RedisWorker;

namespace Repositories.Repositories
{
    public class TicketRepository : ITicketRepository
    {
        private readonly TicketDAL _ticketDAL;
        private readonly IConfiguration _configuration;

        public TicketRepository(IOptions<DataBaseConfig> dataBaseConfig, IOptions<DomainConfig> domainConfig, IConfiguration configuration)
        {
            var _StrConnection = dataBaseConfig.Value.SqlServer.ConnectionString;
            _ticketDAL = new TicketDAL(_StrConnection);
        }
        //aPI ================================================================================


        public Task<TicketListResponse> GetMyTickets(string userId, int status, int page, int size)
        => _ticketDAL.GetMyTickets(userId, status, page, size);

        public Task<TicketListResponse> GetTickets(int status, string keyword, int page, int size)
            => _ticketDAL.GetTickets(status, keyword, page, size);

        public async Task<TicketDetailDtoApi> CreateTicket(string userId, int serviceId, int departmentId, string subject, string content, int priority)
        {
            var ticket = await _ticketDAL.CreateTicket(userId, serviceId, departmentId, subject, priority);

            // tạo message đầu tiên
            var msg = await _ticketDAL.AddMessage(ticket.Id, "Customer", userId, content, null);

            return await GetTicketDetail(ticket.Id);
        }

        public Task<TicketDetailDtoApi> GetTicketDetail(Guid ticketId)
            => _ticketDAL.GetTicketDetail(ticketId);

        public Task<TicketMessageDtoApi> AddMessage(Guid ticketId, string senderType, string senderId, string content, string contentHtml)
            => _ticketDAL.AddMessage(ticketId, senderType, senderId, content, contentHtml);
        public Task<int> InsertMessageAttachments(long messageId, List<AttachFileViewModel> files)
            => _ticketDAL.InsertMessageAttachmentsGuid (messageId,  files);
        public  Task<Guid> GetTicketIdByMessageId(long messageId)
        => _ticketDAL.GetTicketIdByMessageId(messageId);
        public Task<CreateTicketResultDto> CreateTicketWithFirstMessage(
    string userId, int serviceId, int departmentId, string subject, string content, int priority)
            => _ticketDAL.CreateTicketWithFirstMessage(userId, serviceId, departmentId, subject, content, priority);





        //=============================================================
        public async Task<(List<TicketListItemVm> Items, int Total)> SearchAsync(TicketSearchQuery query)
        {
            var items = await _ticketDAL.GetTicketsAsync(query.Q, query.Status, query.ServiceId, query.DepartmentId, query.Page, query.PageSize);
            var total = await _ticketDAL.CountTicketsAsync(query.Q, query.Status, query.ServiceId, query.DepartmentId);
            return (items, total);
        }

        public async Task<TicketDetailDto?> GetDetailAsync(Guid ticketId)
        {
            var ticket = await _ticketDAL.GetTicketByIdAsync(ticketId);
            if (ticket == null) return null;

            var messages = await _ticketDAL.GetMessagesAsync(ticketId); // List<TicketMessageVm> hoặc entity

            var msgIds = messages.Select(m => m.Id).ToList();
            var attachments = msgIds.Count == 0
                ? new List<AttachFileRow>()
                : await _ticketDAL.GetAttachmentsByMessageIdsAsync(msgIds, type: 200);

            var map = attachments
                .GroupBy(x => x.DataId)
                .ToDictionary(
                    g => g.Key,
                    g => g.Select(x => new AttachFileViewModel
                    {
                        Url = x.Path,
                        Name = string.IsNullOrWhiteSpace(x.FileName)
                            ? System.IO.Path.GetFileName(x.Path)
                            : x.FileName
                    }).ToList()
                );

            foreach (var m in messages)
            {
                if (map.TryGetValue(m.Id, out var files))
                    m.AttachFiles = files;
            }

            return new TicketDetailDto
            {
                Ticket = ticket,
                Messages = messages
            };
        }

        public async Task<TicketMessageDto> AddReplyAsync(AddReplyCommand cmd)
        {
            var message = new TicketMessage
            {
                //Id = Guid.NewGuid(),
                TicketId = cmd.TicketId,
                SenderType = MessageSenderType.Agent,
                SenderId = cmd.AgentId,
                Content = cmd.Content,
                ContentHtml = cmd.ContentHtml,
                CreatedAt = DateTime.UtcNow
            };

            await _ticketDAL.AddMessageAsync(message);

            return new TicketMessageDto
            {
                Id = message.Id,
                TicketId = message.TicketId,
                SenderType = "Agent",
                SenderId = message.SenderId,
                Content = message.Content,
                ContentHtml = message.ContentHtml,
                CreatedAt = message.CreatedAt.ToLocalTime().ToString("dd/MM/yyyy HH:mm")
            };
        }

        public Task UpdateStatusAsync(Guid ticketId, TicketStatus status)
            => _ticketDAL.UpdateTicketStatusAsync(ticketId, status);

        public Task AssignAsync(Guid ticketId, string agentId)
            => _ticketDAL.AssignAgentAsync(ticketId, agentId);
    }
}
