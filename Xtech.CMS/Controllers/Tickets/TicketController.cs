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
            return View(new TicketIndexVm { Query = query, Items = items, Total = total });
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

                if (hasFiles)
                {
                    long totalSize = cmd.AttachFiles!.Sum(f => f.Length);
                    if (totalSize > 25 * 1024 * 1024)
                        return BadRequest("Total attachments exceed 25MB");
                }

                // 1) Lưu message
                var dto = await _ticketRepository.AddReplyAsync(cmd);

                // 2) Upload + lưu attachments
                var attachFilesVm = new List<AttachFileViewModel>();
                if (hasFiles)
                {
                    foreach (var f in cmd.AttachFiles!)
                    {
                        var url = await UpLoadHelper.UploadFileOrImage(f, dto.Id, 200);
                        if (!string.IsNullOrEmpty(url))
                            attachFilesVm.Add(new AttachFileViewModel { Url = url, Name = f.FileName });
                    }

                    if (attachFilesVm.Any())
                        await _ticketRepository.InsertMessageAttachments(dto.Id, attachFilesVm);
                }

                // 3) Publish Redis
                var createdAtStr = dto.CreatedAt; 

                var payload = new
                {
                    id = dto.Id,
                    ticketId = dto.TicketId,
                    senderType = dto.SenderType,
                    senderId = dto.SenderId,
                    content = dto.Content,
                    contentHtml = dto.ContentHtml,
                    createdAt = createdAtStr,
                    attachFiles = attachFilesVm.Select(f => new { url = f.Url, name = f.Name })
                };

                await _subscriber.PublishAsync(
                    $"TICKET_{cmd.TicketId}",
                    JsonSerializer.Serialize(payload));

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
        // CLOSE TICKET (AJAX) - CMS staff đóng ticket
        // POST /Ticket/CloseTicket
        // =====================================================================
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CloseTicket([FromForm] Guid ticketId)
        {
            try
            {
                if (ticketId == Guid.Empty)
                    return Ok(new { success = false, message = "Missing TicketId" });

                // 1) Đổi status => Closed (3)
                await _ticketRepository.UpdateStatusAsync(ticketId, TicketStatus.Closed);

                // 2) Publish Redis => WebUser SSE nhận => hiển thị thông báo ticket đã đóng
                var payload = new
                {
                    type = "status_changed",
                    ticketId = ticketId,
                    status = (int)TicketStatus.Closed,
                    statusText = "Closed"
                };

                await _subscriber.PublishAsync(
                    $"TICKET_{ticketId}",
                    JsonSerializer.Serialize(payload));

                return Ok(new { success = true, message = "Ticket đã được đóng." });
            }
            catch (Exception ex)
            {
                LogHelper.InsertLogTelegram("TicketController.CloseTicket: " + ex);
                return StatusCode(500, new { success = false, message = "Close ticket failed" });
            }
        }

        // =====================================================================
        // REOPEN TICKET (AJAX) - CMS staff mở lại ticket nếu cần
        // POST /Ticket/ReopenTicket
        // =====================================================================
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ReopenTicket([FromForm] Guid ticketId)
        {
            try
            {
                if (ticketId == Guid.Empty)
                    return Ok(new { success = false, message = "Missing TicketId" });

                // 1) Đổi status => Open (0)
                await _ticketRepository.UpdateStatusAsync(ticketId, TicketStatus.Open);

                // 2) Publish Redis => WebUser SSE nhận => cập nhật UI
                var payload = new
                {
                    type = "status_changed",
                    ticketId = ticketId,
                    status = (int)TicketStatus.Open,
                    statusText = "Open"
                };

                await _subscriber.PublishAsync(
                    $"TICKET_{ticketId}",
                    JsonSerializer.Serialize(payload));

                return Ok(new { success = true, message = "Ticket đã được mở lại." });
            }
            catch (Exception ex)
            {
                LogHelper.InsertLogTelegram("TicketController.ReopenTicket: " + ex);
                return StatusCode(500, new { success = false, message = "Reopen ticket failed" });
            }
        }

        // =====================================================================
        // CHANGE STATUS (form submit - giữ nguyên cho các case khác)
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