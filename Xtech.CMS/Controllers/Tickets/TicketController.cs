using Entities.Models;
using Entities.ViewModels.TicketApi;
using Entities.ViewModels.Tickets;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using Repositories.IRepositories;
using System.Threading.Tasks;
using Ultilities;
using Utilities;
using Xtech.CMS.Services;

namespace Xtech.CMS.Controllers.Tickets
{
    public class TicketController : Controller
    {
        private readonly IConfiguration configuration;
        private readonly ITicketRepository _ticketRepository;
        private readonly IHubContext<TicketHub> _hubContext;



        public TicketController(
    ITicketRepository ticketRepository,
    IHubContext<TicketHub> hubContext,
    IConfiguration configuration)
        {
            _ticketRepository = ticketRepository;
            _hubContext = hubContext;
            this.configuration = configuration;
        }


        public async Task<IActionResult> Index([FromQuery] TicketSearchQuery query)
        {
            var (items, total) = await _ticketRepository.SearchAsync(query);

            var vm = new TicketIndexVm
            {
                Query = query,
                Items = items,
                Total = total
            };

            return View(vm);
        }

        
        [HttpGet]
        public async Task<IActionResult> Detail(Guid id)
        {
            var dto = await _ticketRepository.GetDetailAsync(id);
            if (dto == null) return NotFound();

            return View(dto);
        }

        // POST: admin reply (AJAX)
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Reply([FromForm] AddReplyCommand cmd)
        {
            try
            {
                if (cmd.TicketId == Guid.Empty) return BadRequest("Missing TicketId");

                cmd.AgentId = User?.Identity?.Name ?? "admin";

                var hasContent = !string.IsNullOrWhiteSpace(cmd.Content) || !string.IsNullOrWhiteSpace(cmd.ContentHtml);
                var hasFiles = cmd.AttachFiles != null && cmd.AttachFiles.Any();

                if (!hasContent && !hasFiles)
                    return BadRequest("Reply content is empty");

                // ✅ validate tổng size 25MB
                if (hasFiles)
                {
                    long total = cmd.AttachFiles!.Sum(f => f.Length);
                    if (total > 25 * 1024 * 1024)
                        return BadRequest("Total attachments exceed 25MB");
                }

                // 1) tạo message trước => lấy messageId (BIGINT)
                var dto = await _ticketRepository.AddReplyAsync(cmd); // returns TicketMessageDto (Id long)

                // 2) upload + lưu attachfile theo messageId
                List<AttachFileViewModel> attachFilesVm = new();
                if (hasFiles)
                {
                    foreach (var f in cmd.AttachFiles!)
                    {
                        // data_id phải là LONG messageId
                        var url = await UpLoadHelper.UploadFileOrImage(f, dto.Id, 200);
                        if (!string.IsNullOrEmpty(url))
                        {
                            attachFilesVm.Add(new AttachFileViewModel { Url = url, Name = f.FileName });
                        }
                    }

                    if (attachFilesVm.Any())
                    {
                        await _ticketRepository.InsertMessageAttachments(dto.Id, attachFilesVm);
                    }
                }

                // 3) broadcast message trước (không kèm attachments để đỡ payload)
                await _hubContext.Clients
                    .Group($"ticket-{cmd.TicketId}")
                    .SendAsync("ReceiveMessage", new
                    {
                        id = dto.Id,
                        ticketId = dto.TicketId,
                        senderType = dto.SenderType,
                        senderId = dto.SenderId,
                        content = dto.Content,
                        contentHtml = dto.ContentHtml,
                        createdAt = dto.CreatedAt
                    });

                // 4) broadcast attachments riêng (nếu có)
                if (attachFilesVm.Any())
                {
                    await _hubContext.Clients
                        .Group($"ticket-{cmd.TicketId}")
                        .SendAsync("ReceiveAttachments", new
                        {
                            messageId = dto.Id,
                            attachFiles = attachFilesVm
                        });
                }

                return Ok(new
                {
                    success = true,
                    data = new
                    {
                        id = dto.Id,
                        ticketId = dto.TicketId,
                        senderType = dto.SenderType,
                        senderId = dto.SenderId,
                        content = dto.Content,
                        contentHtml = dto.ContentHtml,
                        createdAt = dto.CreatedAt,
                        attachFiles = attachFilesVm
                    }
                });
            }
            catch (Exception ex)
            {
                LogHelper.InsertLogTelegram("TicketController.Reply CMS: " + ex);
                return StatusCode(500, new { success = false, message = "Reply failed" });
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ChangeStatus(Guid ticketId, TicketStatus status)
        {
            await _ticketRepository.UpdateStatusAsync(ticketId, status);
            return RedirectToAction(nameof(Detail), new { id = ticketId });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Assign(Guid ticketId, string agentId)
        {
            await _ticketRepository.AssignAsync(ticketId, agentId);
            return RedirectToAction(nameof(Detail), new { id = ticketId });
        }
    }

    
}

