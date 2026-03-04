using Entities.Models;
using Entities.ViewModels.TicketApi;
using Entities.ViewModels.Tickets;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using Repositories.IRepositories;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Ultilities;
using Utilities;
using Xtech.CMS.Services;

namespace Xtech.CMS.Controllers.Tickets
{
    public class TicketController : Controller
    {
        private readonly IConfiguration _configuration;
        private readonly ITicketRepository _ticketRepository;
        private readonly IHubContext<TicketHub> _hubContext;
        private readonly IHttpClientFactory _httpClientFactory;

        public TicketController(
            ITicketRepository ticketRepository,
            IHubContext<TicketHub> hubContext,
            IConfiguration configuration,
            IHttpClientFactory httpClientFactory)
        {
            _ticketRepository = ticketRepository;
            _hubContext = hubContext;
            _configuration = configuration;
            _httpClientFactory = httpClientFactory;
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

                // 2) Upload file + lưu attachments theo messageId
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

                // 3) Broadcast qua BE Hub (chứa tất cả SignalR connections)
                //    - Không dùng _hubContext của CMS vì Hub thật chạy ở BE
                _ = Task.Run(() => BroadcastViaBEAsync(cmd.TicketId, dto, attachFilesVm));

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

        // =====================================================================
        // PRIVATE: Gọi BE để broadcast SignalR tới tất cả clients
        // (cả WebUser lẫn CMS đều connect BE Hub nên đều nhận được)
        // =====================================================================
        private async Task BroadcastViaBEAsync(
            Guid ticketId,
            dynamic dto,
            List<AttachFileViewModel> attachFiles)
        {
            try
            {
                var beBaseUrl = _configuration["BESettings:BaseUrl"];
                // Ví dụ appsettings: "BESettings:BaseUrl": "http://be.x-tech.vn"
                // Local:             "BESettings:BaseUrl": "https://localhost:56834"

                var secret = _configuration["BESettings:InternalSecret"];

                using var http = _httpClientFactory.CreateClient();
                http.DefaultRequestHeaders.Add("X-Internal-Secret", secret);

                var payload = new
                {
                    ticketId = ticketId.ToString(),
                    id = dto.Id,
                    senderType = dto.SenderType,   // "Agent"
                    senderId = dto.SenderId,
                    content = dto.Content,
                    contentHtml = dto.ContentHtml,
                    createdAt = dto.CreatedAt is DateTime dt
                        ? dt.ToString("dd/MM/yyyy HH:mm")
                        : dto.CreatedAt?.ToString() ?? "",
                    attachFiles = attachFiles.Select(f => new { url = f.Url, name = f.Name })
                };

                var json = JsonSerializer.Serialize(payload);
                var httpContent = new StringContent(json, Encoding.UTF8, "application/json");

                var response = await http.PostAsync(
                    $"{beBaseUrl}/api/ticket/internal-broadcast",
                    httpContent);

                if (!response.IsSuccessStatusCode)
                {
                    var body = await response.Content.ReadAsStringAsync();
                    LogHelper.InsertLogTelegram(
                        $"BroadcastViaBEAsync failed: {response.StatusCode} - {body}");
                }
            }
            catch (Exception ex)
            {
                LogHelper.InsertLogTelegram("BroadcastViaBEAsync exception: " + ex);
            }
        }
    }
}