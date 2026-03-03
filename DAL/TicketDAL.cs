using DAL.Generic;
using DAL.StoreProcedure;
using Entities.Models;
using Entities.ViewModels.TicketApi;
using Microsoft.EntityFrameworkCore;
using StackExchange.Redis;
using Utilities;

namespace DAL
{
    public class TicketDAL
    {
        private readonly string _connection;
        public TicketDAL(string connection) => _connection = connection;


        //API=================================================================================

        public async Task<TicketListResponse> GetMyTickets(string userId, int status, int page, int size)
        {
            using var db = new EntityDataContext(_connection);

            var q = db.Set<Ticket>().AsNoTracking().Where(x => x.CreatedByUserId == userId);

            if (status >= 0) q = q.Where(x => (int)x.Status == status);

            var total = await q.CountAsync();

            var items = await q.OrderByDescending(x => x.LastMessageAt)
                .Skip((page - 1) * size).Take(size)
                .Select(x => new TicketListItemDto
                {
                    id = x.Id,
                    code = x.Code,
                    serviceId = x.ServiceId,
                    serviceName = "", // nếu cần join services thì fill thêm
                    subject = x.Subject,
                    status = (int)x.Status,
                    assignedAgentId = x.AssignedAgentId,
                    lastMessageAt = x.LastMessageAt
                })
                .ToListAsync();

            return new TicketListResponse { page = page, size = size, total = total, items = items };
        }

        public async Task<TicketListResponse> GetTickets(int status, string keyword, int page, int size)
        {
            using var db = new EntityDataContext(_connection);

            var q = db.Set<Ticket>().AsNoTracking().AsQueryable();

            if (status >= 0) q = q.Where(x => (int)x.Status == status);
            if (!string.IsNullOrWhiteSpace(keyword))
                q = q.Where(x => x.Subject.Contains(keyword) || x.Code.Contains(keyword));

            var total = await q.CountAsync();

            var items = await q.OrderByDescending(x => x.LastMessageAt)
                .Skip((page - 1) * size).Take(size)
                .Select(x => new TicketListItemDto
                {
                    id = x.Id,
                    code = x.Code,
                    serviceId = x.ServiceId,
                    serviceName = "",
                    subject = x.Subject,
                    status = (int)x.Status,
                    assignedAgentId = x.AssignedAgentId,
                    lastMessageAt = x.LastMessageAt
                })
                .ToListAsync();

            return new TicketListResponse { page = page, size = size, total = total, items = items };
        }

        public async Task<Ticket> CreateTicket(string userId, int serviceId, int departmentId, string subject, int priority)
        {
            using var db = new EntityDataContext(_connection);

            var now = DateTime.UtcNow;

            var ticket = new Ticket
            {
                Id = Guid.NewGuid(),
                Code = "TK-" + now.Ticks.ToString().Substring(10), // đơn giản
                ServiceId = serviceId,
                DepartmentId = departmentId,
                Subject = subject,
                Status = TicketStatus.Open,
                Priority = priority,
                CreatedByUserId = userId,
                AssignedAgentId = null,
                CreatedAt = now,
                UpdatedAt = now,
                LastMessageAt = now
            };

            db.Set<Ticket>().Add(ticket);
            await db.SaveChangesAsync();
            return ticket;
        }

        public async Task<TicketMessageDtoApi> AddMessage(Guid ticketId, string senderType, string senderId, string content, string contentHtml)
        {
            using var db = new EntityDataContext(_connection);

            var now = DateTime.UtcNow;

            var msg = new TicketMessage
            {
                //Id = Guid.NewGuid(),
                TicketId = ticketId,
                SenderType = senderType == "Agent" ? MessageSenderType.Agent : MessageSenderType.Customer,
                SenderId = senderId,
                Content = content,
                ContentHtml = contentHtml,
                CreatedAt = now
            };

            db.Set<TicketMessage>().Add(msg);

            var ticket = await db.Set<Ticket>().FirstOrDefaultAsync(x => x.Id == ticketId);
            if (ticket != null)
            {
                ticket.LastMessageAt = now;
                ticket.UpdatedAt = now;

                // nếu Agent reply thì chuyển InProgress
                if (senderType == "Agent" && ticket.Status == TicketStatus.Open)
                    ticket.Status = TicketStatus.InProgress;
            }

            await db.SaveChangesAsync();

            return new TicketMessageDtoApi
            {
                id = msg.Id,
                ticketId = msg.TicketId,
                senderType = senderType,
                senderId = senderId,
                content = msg.Content,
                contentHtml = msg.ContentHtml,
                createdAt = msg.CreatedAt
            };
        }


        public async Task<CreateTicketResultDto> CreateTicketWithFirstMessage(
    string userId, int serviceId, int departmentId, string subject, string content, int priority)
        {
            using var db = new EntityDataContext(_connection);
            var now = DateTime.UtcNow;

            // 1) create ticket
            var ticket = new Ticket
            {
                Id = Guid.NewGuid(),
                Code = "TK-" + now.Ticks.ToString().Substring(10), // đơn giản
                ServiceId = serviceId,
                DepartmentId = departmentId,
                Subject = subject,
                Status = TicketStatus.Open,
                Priority = priority,
                CreatedByUserId = userId,
                AssignedAgentId = null,
                CreatedAt = now,
                UpdatedAt = now,
                LastMessageAt = now
            };

            db.Set<Ticket>().Add(ticket);
            await db.SaveChangesAsync();

            // 2) create first message (Customer)
            var msg = new TicketMessage
            {
                // Id = identity bigint -> KHÔNG set
                TicketId = ticket.Id,
                SenderType = MessageSenderType.Customer,
                SenderId = userId,
                Content = content,
                ContentHtml = content,
                CreatedAt = now
            };

            db.Set<TicketMessage>().Add(msg);
            await db.SaveChangesAsync(); // ✅ msg.Id có rồi

            // update last message
            ticket.LastMessageAt = now;
            ticket.UpdatedAt = now;
            await db.SaveChangesAsync();

            return new CreateTicketResultDto
            {
                ticket_id = ticket.Id,
                code = ticket.Code,
                first_message_id = msg.Id
            };
        }

        public async Task<Guid> GetTicketIdByMessageId(long messageId)
        {
            using var db = new EntityDataContext(_connection);
            return await db.Set<TicketMessage>()
                .Where(x => x.Id == messageId)
                .Select(x => x.TicketId)
                .FirstAsync();
        }
        public async Task<int> InsertMessageAttachmentsGuid(long messageId, List<AttachFileViewModel> files)
        {
            try
            {
                
                if (files == null || files.Count == 0) return 0;

                using var db = new EntityDataContext(_connection);

                var now = DateTime.UtcNow;

                foreach (var f in files)
                {
                    if (string.IsNullOrWhiteSpace(f?.Url)) continue;

                    var url = f.Url.Trim();
                    var name = f.Name ?? "";

                    // lấy extension an toàn
                    var ext = "";
                    if (!string.IsNullOrWhiteSpace(name))
                        ext = Path.GetExtension(name);

                    if (string.IsNullOrWhiteSpace(ext))
                    {
                        try { ext = Path.GetExtension(new Uri(url).AbsolutePath); }
                        catch { ext = Path.GetExtension(url); }
                    }

                    db.Set<AttachFile>().Add(new AttachFile
                    {
                        
                        DataId = messageId,              // 🔥 link tới message
                        UserId = 0,                     // nếu cần lưu user thì truyền vào param
                        Type = 200,                     // 🔥 tự define: 200 = TicketMessage
                        Path = url,
                        Ext = ext,
                        Capacity = 0,                   // nếu muốn lưu size thì bổ sung
                        CreateDate = now
                    });
                }

                return await db.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                LogHelper.InsertLogTelegram($"InsertMessageAttachmentsGuid Error: {ex}");
                return 0;
            }
        }

        public async Task<TicketDetailDtoApi> GetTicketDetail(Guid ticketId)
        {
            using var db = new EntityDataContext(_connection);

            var t = await db.Set<Ticket>().AsNoTracking().FirstOrDefaultAsync(x => x.Id == ticketId);
            if (t == null) return null;

            // 1) messages
            var messages = await db.Set<TicketMessage>().AsNoTracking()
                .Where(x => x.TicketId == ticketId)
                .OrderBy(x => x.CreatedAt)
                .Select(x => new TicketMessageDtoApi
                {
                    id = x.Id,
                    ticketId = x.TicketId,
                    senderType = x.SenderType == MessageSenderType.Agent ? "Agent" : "Customer",
                    senderId = x.SenderId,
                    content = x.Content,
                    contentHtml = x.ContentHtml,
                    createdAt = x.CreatedAt,
                    AttachFiles = new List<AttachFileViewModel>() // init
                })
                .ToListAsync();

            if (messages.Count > 0)
            {
                var messageIds = messages.Select(x => x.id).ToList();

                // 2) attachments
                // NOTE: nếu AttachFile.Id/DataId là BIGINT thì entity properties phải là long
                var atts = await db.Set<AttachFile>().AsNoTracking()
                    .Where(a => a.Type == 200 && messageIds.Contains(a.DataId))
                    .OrderBy(a => a.CreateDate)
                    .Select(a => new
                    {
                        DataId = a.DataId,
                        Url = a.Path,
                        Name = a.Path,      // nếu bảng không có Name thì dùng Path để lấy tên file
                        Ext = a.Ext
                    })
                    .ToListAsync();

                // group by messageId
                var map = atts
                    .GroupBy(x => x.DataId)
                    .ToDictionary(
                        g => g.Key,
                        g => g.Select(x => new AttachFileViewModel
                        {
                            Url = x.Url,
                            Name = !string.IsNullOrWhiteSpace(x.Name)
                                    ? System.IO.Path.GetFileName(x.Name)
                                    : System.IO.Path.GetFileName(x.Url)
                        }).ToList()
                    );

                // 3) attach into messages
                foreach (var m in messages)
                {
                    if (map.TryGetValue(m.id, out var files))
                        m.AttachFiles = files;
                }
            }

            return new TicketDetailDtoApi
            {
                ticket = new TicketDto
                {
                    id = t.Id,
                    code = t.Code,
                    serviceId = t.ServiceId,
                    departmentId = t.DepartmentId,
                    subject = t.Subject,
                    status = (int)t.Status,
                    priority = t.Priority,
                    createdByUserId = t.CreatedByUserId,
                    assignedAgentId = t.AssignedAgentId,
                    createdAt = t.CreatedAt,
                    updatedAt = t.UpdatedAt,
                    lastMessageAt = t.LastMessageAt
                },
                messages = messages
            };
        }







        //=====================================================================================

        public async Task<List<Ticket>> GetTicketsAsync(
            string? q, int? status, int? serviceId, int? departmentId,
            int page, int pageSize)
        {
            using var db = new EntityDataContext(_connection);

            var query = db.Set<Ticket>().AsNoTracking().AsQueryable();

            // filter
            if (status.HasValue)
                query = query.Where(x => x.Status == (TicketStatus)status.Value);
            if (serviceId.HasValue) query = query.Where(x => x.ServiceId == serviceId.Value);
            if (departmentId.HasValue) query = query.Where(x => x.DepartmentId == departmentId.Value);

            if (!string.IsNullOrWhiteSpace(q))
            {
                var keyword = q.Trim().ToLower();
                query = query.Where(x =>
                    x.Subject.ToLower().Contains(keyword) ||
                    x.Code.ToLower().Contains(keyword));
            }

            // sort: newest activity first (matches CMS)
            query = query.OrderByDescending(x => x.LastMessageAt);

            // paging
            var skip = (page - 1) * pageSize;
            return await query.Skip(skip).Take(pageSize).ToListAsync();
        }

        public async Task<int> CountTicketsAsync(string? q, int? status, int? serviceId, int? departmentId)
        {
            using var db = new EntityDataContext(_connection);

            var query = db.Set<Ticket>().AsNoTracking().AsQueryable();

            if (status.HasValue)
                query = query.Where(x => x.Status == (TicketStatus)status.Value);
            if (serviceId.HasValue) query = query.Where(x => x.ServiceId == serviceId.Value);
            if (departmentId.HasValue) query = query.Where(x => x.DepartmentId == departmentId.Value);

            if (!string.IsNullOrWhiteSpace(q))
            {
                var keyword = q.Trim().ToLower();
                query = query.Where(x =>
                    x.Subject.ToLower().Contains(keyword) ||
                    x.Code.ToLower().Contains(keyword));
            }

            return await query.CountAsync();
        }

        public async Task<Ticket?> GetTicketByIdAsync(Guid id)
        {
            using var db = new EntityDataContext(_connection);
            return await db.Set<Ticket>().AsNoTracking().FirstOrDefaultAsync(x => x.Id == id);
        }

        public async Task<List<TicketMessage>> GetMessagesAsync(Guid ticketId)
        {
            using var db = new EntityDataContext(_connection);
            return await db.Set<TicketMessage>()
                .AsNoTracking()
                .Where(x => x.TicketId == ticketId)
                .OrderBy(x => x.CreatedAt)
                .ToListAsync();
        }

        public async Task<List<AttachFileRow>> GetAttachmentsByMessageIdsAsync(List<long> messageIds, int type)
        {
            using var db = new EntityDataContext(_connection);

            return await db.Set<AttachFile>().AsNoTracking()
                .Where(a => a.Type == type && messageIds.Contains(a.DataId))
                .OrderBy(a => a.CreateDate)
                .Select(a => new AttachFileRow
                {
                    Id = a.Id,
                    DataId = a.DataId,
                    Path = a.Path,
                    Ext = a.Ext,
                    CreateDate = a.CreateDate,
                    FileName = null // nếu bảng có Name thì map vào đây
                })
                .ToListAsync();
        }

        public async Task AddMessageAsync(TicketMessage message)
        {
            using var db = new EntityDataContext(_connection);

            db.Set<TicketMessage>().Add(message);

            var ticket = await db.Set<Ticket>().FirstOrDefaultAsync(x => x.Id == message.TicketId);
            if (ticket != null)
            {
                ticket.LastMessageAt = message.CreatedAt;
                ticket.UpdatedAt = DateTime.UtcNow;

                if (message.SenderType == MessageSenderType.Agent && ticket.Status == TicketStatus.Open)
                    ticket.Status = TicketStatus.InProgress;
            }

            await db.SaveChangesAsync();
        }

        public async Task UpdateTicketStatusAsync(Guid ticketId, TicketStatus status)
        {
            using var db = new EntityDataContext(_connection);
            var ticket = await db.Set<Ticket>().FirstOrDefaultAsync(x => x.Id == ticketId);
            if (ticket == null) return;

            ticket.Status = status;
            ticket.UpdatedAt = DateTime.UtcNow;
            await db.SaveChangesAsync();
        }

        public async Task AssignAgentAsync(Guid ticketId, string agentId)
        {
            using var db = new EntityDataContext(_connection);
            var ticket = await db.Set<Ticket>().FirstOrDefaultAsync(x => x.Id == ticketId);
            if (ticket == null) return;

            ticket.AssignedAgentId = agentId;
            ticket.UpdatedAt = DateTime.UtcNow;
            await db.SaveChangesAsync();
        }
    }
}
