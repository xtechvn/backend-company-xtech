// CMS - TicketController.cs
// Dùng Redis Pub/Sub để broadcast message
// Không còn dùng IHubContext để broadcast nữa

using Entities.Models;
using Entities.ViewModels.TicketApi;
using Entities.ViewModels.Tickets;
using Microsoft.AspNetCore.Mvc;
using Repositories.IRepositories;
using StackExchange.Redis;
using System.Text.Json;
using Ultilities;
using Utilities;

namespace Xtech.CMS.Controllers.Tickets
{
    public class TicketController : Controller
    {
        private readonly IConfiguration _configuration;
        private readonly ITicketRepository _ticketRepository;
        private readonly ISubscriber _subscriber;

        public TicketController(
            ITicketRepository ticketRepository,
            IConfiguration configuration)
        {
            _ticketRepository = ticketRepository;
            _configuration = configuration;

            // Kết nối Redis (cùng server với BE)
            var redisConn = ConnectionMultiplexer.Connect(
                _configuration["Redis:Host"] + ":" + _configuration["Redis:Port"]);
            _subscriber = redisConn.GetSubscriber();
        }

        // =====================================================================
        // INDEX
        // =====================================================================
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

        // =====================================================================
        // DETAIL
        // =====================================================================
        [HttpGet]
        public async Task<IActionResult> Detail(Guid id)
        {
            var dto = await _ticketRepository.GetDetailAsync(id);
            if (dto == null) return NotFound();

            return View(dto);
        }

        // =====================================================================
        // REPLY (AJAX)
        // =====================================================================
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Reply([FromForm] AddReplyCommand cmd)
        {
            try
            {
                if (cmd.TicketId == Guid.Empty)
                    return BadRequest("Missing TicketId");

                cmd.AgentId = User?.Identity?.Name ?? "admin";

                var hasContent = !string.IsNullOrWhiteSpace(cmd.Content)
                              || !string.IsNullOrWhiteSpace(cmd.ContentHtml);
                var hasFiles = cmd.AttachFiles != null && cmd.AttachFiles.Any();

                if (!hasContent && !hasFiles)
                    return BadRequest("Reply content is empty");

                // Validate tổng size 25MB
                if (hasFiles)
                {
                    long totalSize = cmd.AttachFiles!.Sum(f => f.Length);
                    if (totalSize > 25 * 1024 * 1024)
                        return BadRequest("Total attachments exceed 25MB");
                }

                // 1) Lưu message vào DB => lấy messageId (BIGINT)
                var dto = await _ticketRepository.AddReplyAsync(cmd);

                // 2) Upload file + lưu attachments
                var attachFilesVm = new List<AttachFileViewModel>();
                if (hasFiles)
                {
                    foreach (var f in cmd.AttachFiles!)
                    {
                        var url = await UpLoadHelper.UploadFileOrImage(f, dto.Id, 200);
                        if (!string.IsNullOrEmpty(url))
                        {
                            attachFilesVm.Add(new AttachFileViewModel
                            {
                                Url = url,
                                Name = f.FileName
                            });
                        }
                    }

                    if (attachFilesVm.Any())
                        await _ticketRepository.InsertMessageAttachments(dto.Id, attachFilesVm);
                }

                // 3) Publish lên Redis channel TICKET_{ticketId}
                //    - BE đang subscribe => BE HubContext broadcast tới WebUser browser ✅
                //    - CMS đang subscribe => CMS HubContext broadcast tới CMS browser ✅
                var createdAtStr = dto.CreatedAt;

                var payload = new
                {
                    id = dto.Id,
                    ticketId = dto.TicketId,
                    senderType = dto.SenderType,   // "Agent"
                    senderId = dto.SenderId,
                    content = dto.Content,
                    contentHtml = dto.ContentHtml,
                    createdAt = createdAtStr,
                    attachFiles = attachFilesVm.Select(f => new { url = f.Url, name = f.Name })
                };

                var json = JsonSerializer.Serialize(payload);
                await _subscriber.PublishAsync($"TICKET_{cmd.TicketId}", json);

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
                        createdAt = createdAtStr,
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

        // =====================================================================
        // CHANGE STATUS
        // =====================================================================
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ChangeStatus(Guid ticketId, TicketStatus status)
        {
            await _ticketRepository.UpdateStatusAsync(ticketId, status);
            return RedirectToAction(nameof(Detail), new { id = ticketId });
        }

        // =====================================================================
        // ASSIGN
        // =====================================================================
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Assign(Guid ticketId, string agentId)
        {
            await _ticketRepository.AssignAsync(ticketId, agentId);
            return RedirectToAction(nameof(Detail), new { id = ticketId });
        }
    }
}