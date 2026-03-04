using Entities.ViewModels.TicketApi;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Repositories.IRepositories;
using StackExchange.Redis;
using Ultilities;
using Ultilities.Constants;
using Ultilities.RedisWorker;
using Utilities;

namespace Xtech.CMS.APIControllers
{
    [ApiController]
    [Route("api/ticket")]
    public class TicketAPIController : ControllerBase
    {
        private readonly IConfiguration configuration;
        private readonly ITicketRepository ticketRepository;
        private readonly RedisConn _redisService;
        private readonly ISubscriber _subscriber;

        public TicketAPIController(
            IConfiguration configuration,
            ITicketRepository ticketRepository)
        {
            this.configuration = configuration;
            this.ticketRepository = ticketRepository;
            _redisService = new RedisConn(configuration);

            // ✅ Kết nối Redis để Publish realtime
            // Bỏ IHubContext<TicketHub> - không dùng SignalR nữa
            var redisConn = ConnectionMultiplexer.Connect(
                configuration["Redis:Host"] + ":" + configuration["Redis:Port"]);
            _subscriber = redisConn.GetSubscriber();
        }

        // =====================================================================
        // 1) USER: list tickets của user (NO REDIS CACHE)
        // =====================================================================
        [HttpPost("get-my-tickets.json")]
        public async Task<ActionResult> GetMyTickets([FromForm] string token)
        {
            try
            {
                JArray objParr = null;
                if (!CommonHelper.GetParamWithKey(token, out objParr, configuration["DataBaseConfig:key_api:b2c"]))
                    return Ok(new { status = (int)ResponseType.ERROR, msg = "Key ko hop le" });

                var obj = objParr[0];
                var user_id = obj["user_id"]?.ToString();

                if (string.IsNullOrWhiteSpace(user_id))
                    return Ok(new { status = (int)ResponseType.ERROR, msg = "Missing user_id" });

                int status = obj["status"] != null && int.TryParse(obj["status"]?.ToString(), out var s) ? s : -1;
                int page = obj["page"] != null && int.TryParse(obj["page"]?.ToString(), out var p) ? p : 1;
                int size = obj["size"] != null && int.TryParse(obj["size"]?.ToString(), out var z) ? z : 20;

                var data = await ticketRepository.GetMyTickets(user_id, status, page, size);

                return Ok(new { status = (int)ResponseType.SUCCESS, msg = "Success", data });
            }
            catch (Exception ex)
            {
                LogHelper.InsertLogTelegram("TicketAPIController - GetMyTickets: " + ex + "\n Token: " + token);
                return Ok(new { status = (int)ResponseType.FAILED, msg = "Error: " + ex.ToString() });
            }
        }

        // =====================================================================
        // 2) USER: create ticket + first message
        // =====================================================================
        [HttpPost("create-ticket.json")]
        public async Task<ActionResult> CreateTicket([FromForm] string token)
        {
            try
            {
                JArray objParr = null;
                if (!CommonHelper.GetParamWithKey(token, out objParr, configuration["DataBaseConfig:key_api:b2c"]))
                    return Ok(new { status = (int)ResponseType.ERROR, msg = "Key ko hop le" });

                var user_id = objParr[0]["user_id"]?.ToString();
                var service_id = Convert.ToInt32(objParr[0]["service_id"]);
                var department_id = Convert.ToInt32(objParr[0]["department_id"]);
                var subject = objParr[0]["subject"]?.ToString();
                var content = objParr[0]["content"]?.ToString();
                var priority = objParr[0]["priority"] != null
                                    ? Convert.ToInt32(objParr[0]["priority"]) : 0;

                if (string.IsNullOrWhiteSpace(user_id) ||
                    string.IsNullOrWhiteSpace(subject) ||
                    string.IsNullOrWhiteSpace(content))
                    return Ok(new { status = (int)ResponseType.ERROR, msg = "Missing required fields" });

                var created = await ticketRepository.CreateTicketWithFirstMessage(
                    user_id, service_id, department_id, subject, content, priority);

                return Ok(new { status = (int)ResponseType.SUCCESS, msg = "Success", data = created });
            }
            catch (Exception ex)
            {
                LogHelper.InsertLogTelegram("TicketAPIController - CreateTicket: " + ex + "\n Token: " + token);
                return Ok(new { status = (int)ResponseType.FAILED, msg = "Error: " + ex.ToString() });
            }
        }

        // =====================================================================
        // 3) USER/CMS: ticket detail
        // =====================================================================
        [HttpPost("get-ticket-detail.json")]
        public async Task<ActionResult> GetTicketDetail([FromForm] string token)
        {
            try
            {
                JArray objParr = null;
                if (!CommonHelper.GetParamWithKey(token, out objParr, configuration["DataBaseConfig:key_api:b2c"]))
                    return Ok(new { status = (int)ResponseType.ERROR, msg = "Key ko hop le" });

                var ticket_id_str = objParr[0]["ticket_id"]?.ToString();
                if (!Guid.TryParse(ticket_id_str, out var ticketId))
                    return Ok(new { status = (int)ResponseType.ERROR, msg = "Invalid ticket_id" });

                var detail = await ticketRepository.GetTicketDetail(ticketId);
                if (detail == null)
                    return Ok(new { status = (int)ResponseType.ERROR, msg = "Ticket not found" });

                return Ok(new { status = (int)ResponseType.SUCCESS, msg = "Success", data = detail });
            }
            catch (Exception ex)
            {
                LogHelper.InsertLogTelegram("TicketAPIController - GetTicketDetail: " + ex + "\n Token: " + token);
                return Ok(new { status = (int)ResponseType.FAILED, msg = "Error: " + ex.ToString() });
            }
        }

        // =====================================================================
        // 4) USER/CMS: reply chat
        //    Lưu message → Redis PUBLISH TICKET_{ticketId}
        //    WebUser SSE và CMS SSE đều subscribe => nhận realtime 2 chiều ✅
        //    Không còn bị Mixed Content HTTP/HTTPS vì xử lý server-side
        // =====================================================================
        [HttpPost("reply-ticket.json")]
        public async Task<ActionResult> ReplyTicket([FromForm] string token)
        {
            try
            {
                JArray objParr = null;
                if (!CommonHelper.GetParamWithKey(token, out objParr, configuration["DataBaseConfig:key_api:b2c"]))
                    return Ok(new { status = (int)ResponseType.ERROR, msg = "Key ko hop le" });

                if (!Guid.TryParse(objParr[0]["ticket_id"]?.ToString(), out var ticketId))
                    return Ok(new { status = (int)ResponseType.ERROR, msg = "Invalid ticket_id" });

                var sender_type = objParr[0]["sender_type"]?.ToString();
                var sender_id = objParr[0]["sender_id"]?.ToString();
                var content = objParr[0]["content"]?.ToString();
                var content_html = objParr[0]["content_html"]?.ToString();

                if (string.IsNullOrWhiteSpace(sender_type) || string.IsNullOrWhiteSpace(sender_id))
                    return Ok(new { status = (int)ResponseType.ERROR, msg = "Missing sender" });

                // 1) Lưu message vào DB
                var message = await ticketRepository.AddMessage(
                    ticketId, sender_type, sender_id, content, content_html);

                // 2) ✅ Publish lên Redis thay vì SignalR broadcast
                //    WebUser SSE (/support/ticket-stream) subscribe => nhận ✅
                //    CMS SSE (/Ticket/Stream) subscribe             => nhận ✅
                var payload = new
                {
                    id = message.id,
                    ticketId = message.ticketId,
                    senderType = message.senderType,
                    senderId = message.senderId,
                    content = message.content,
                    contentHtml = message.contentHtml,
                    createdAt = message.createdAt.ToString("dd/MM/yyyy HH:mm"),
                    attachFiles = new List<object>()
                };

                var json = System.Text.Json.JsonSerializer.Serialize(payload);
                await _subscriber.PublishAsync($"TICKET_{ticketId}", json);

                return Ok(new { status = (int)ResponseType.SUCCESS, msg = "Success", data = message });
            }
            catch (Exception ex)
            {
                LogHelper.InsertLogTelegram("TicketAPIController - ReplyTicket: " + ex + "\n Token: " + token);
                return Ok(new { status = (int)ResponseType.FAILED, msg = "Error: " + ex.ToString() });
            }
        }

        // =====================================================================
        // 5) USER/CMS: lưu attachments + Publish lên Redis
        // =====================================================================
        [HttpPost("add-message-attachments.json")]
        public async Task<ActionResult> AddMessageAttachments([FromForm] string token)
        {
            try
            {
                JArray objParr = null;
                if (!CommonHelper.GetParamWithKey(token, out objParr, configuration["DataBaseConfig:key_api:b2c"]))
                    return Ok(new { status = (int)ResponseType.ERROR, msg = "Key ko hop le" });

                var messageId = Convert.ToInt64(objParr[0]["message_id"]);
                var attachFiles = objParr[0]["attach_files"]?.ToObject<List<AttachFileViewModel>>() ?? new();

                // 1) Lưu DB
                await ticketRepository.InsertMessageAttachments(messageId, attachFiles);

                // 2) Lấy ticketId để Publish đúng channel
                var ticketId = await ticketRepository.GetTicketIdByMessageId(messageId);

                // 3) ✅ Publish attachments lên Redis
                //    FE nhận event type="attachments" => updateMessageAttachments()
                var payload = new
                {
                    type = "attachments",
                    messageId = messageId,
                    ticketId = ticketId,
                    attachFiles = attachFiles.Select(f => new { url = f.Url, name = f.Name })
                };

                var json = System.Text.Json.JsonSerializer.Serialize(payload);
                await _subscriber.PublishAsync($"TICKET_{ticketId}", json);

                return Ok(new { status = (int)ResponseType.SUCCESS, msg = "Success" });
            }
            catch (Exception ex)
            {
                LogHelper.InsertLogTelegram("TicketAPIController - AddMessageAttachments: " + ex + "\n Token: " + token);
                return Ok(new { status = (int)ResponseType.FAILED, msg = "Error: " + ex.ToString() });
            }
        }

        // =====================================================================
        // 6) CMS: list tickets (admin)
        // =====================================================================
        [HttpPost("get-tickets.json")]
        public async Task<ActionResult> GetTickets([FromForm] string token)
        {
            try
            {
                JArray objParr = null;
                if (!CommonHelper.GetParamWithKey(token, out objParr, configuration["DataBaseConfig:key_api:b2c"]))
                    return Ok(new { status = (int)ResponseType.ERROR, msg = "Key ko hop le" });

                var status = objParr[0]["status"] != null ? Convert.ToInt32(objParr[0]["status"]) : -1;
                var page = objParr[0]["page"] != null ? Convert.ToInt32(objParr[0]["page"]) : 1;
                var size = objParr[0]["size"] != null ? Convert.ToInt32(objParr[0]["size"]) : 20;
                var keyword = objParr[0]["keyword"]?.ToString();

                var data = await ticketRepository.GetTickets(status, keyword, page, size);

                return Ok(new { status = (int)ResponseType.SUCCESS, msg = "Success", data });
            }
            catch (Exception ex)
            {
                LogHelper.InsertLogTelegram("TicketAPIController - GetTickets: " + ex + "\n Token: " + token);
                return Ok(new { status = (int)ResponseType.FAILED, msg = "Error: " + ex.ToString() });
            }
        }
    }
}