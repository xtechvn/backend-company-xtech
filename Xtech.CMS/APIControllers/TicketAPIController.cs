    using Entities.ViewModels.TicketApi;
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.AspNetCore.SignalR;
    using Newtonsoft.Json;
    using Newtonsoft.Json.Linq;
    using Repositories.IRepositories;
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
            private readonly IHubContext<TicketHub> _hubContext;

            public TicketAPIController(
                IConfiguration configuration,
                ITicketRepository ticketRepository, IHubContext<TicketHub> hubContext)
            {
                this.configuration = configuration;
                this.ticketRepository = ticketRepository;
                _hubContext = hubContext;
                _redisService = new RedisConn(configuration);
            }

            //// 1) USER: list tickets của user
            //[HttpPost("get-my-tickets.json")]
            //public async Task<ActionResult> GetMyTickets([FromForm] string token)
            //{
            //    try
            //    {
            //        JArray objParr = null;
            //        if (!CommonHelper.GetParamWithKey(token, out objParr, configuration["DataBaseConfig:key_api:b2c"]))
            //        {
            //            return Ok(new { status = (int)ResponseType.ERROR, msg = "Key ko hop le" });
            //        }

            //        // param mẫu: { "user_id":"customer01", "status":-1, "page":1, "size":20 }
            //        var user_id = objParr[0]["user_id"]?.ToString();
            //        var status = objParr[0]["status"] != null ? Convert.ToInt32(objParr[0]["status"]) : -1;
            //        var page = objParr[0]["page"] != null ? Convert.ToInt32(objParr[0]["page"]) : 1;
            //        var size = objParr[0]["size"] != null ? Convert.ToInt32(objParr[0]["size"]) : 20;

            //        if (string.IsNullOrEmpty(user_id))
            //            return Ok(new { status = (int)ResponseType.ERROR, msg = "Missing user_id" });

            //        // optional cache list (theo user + status + page)
            //        string cache_name = $"TICKET_MY_{user_id}_{status}_{page}_{size}";
            //        string j_data = null;
            //        try
            //        {
            //            j_data = await _redisService.GetAsync(cache_name, Convert.ToInt32(configuration["Redis:Database:db_common"]));
            //        }
            //        catch (Exception ex)
            //        {
            //            LogHelper.InsertLogTelegram("TicketController - GetMyTickets(redis): " + ex + "\n Token: " + token);
            //        }

            //        TicketListResponse data = null;
            //        if (!string.IsNullOrEmpty(j_data))
            //        {
            //            data = JsonConvert.DeserializeObject<TicketListResponse>(j_data);
            //        }
            //        else
            //        {
            //            data = await ticketRepository.GetMyTickets(user_id, status, page, size);
            //            try
            //            {
            //                _redisService.Set(cache_name, JsonConvert.SerializeObject(data), Convert.ToInt32(configuration["Redis:Database:db_common"]));
            //            }
            //            catch (Exception ex)
            //            {
            //                LogHelper.InsertLogTelegram("TicketController - GetMyTickets(set redis): " + ex + "\n Token: " + token);
            //            }
            //        }

            //        return Ok(new
            //        {
            //            status = (int)ResponseType.SUCCESS,
            //            msg = "Success",
            //            data = data
            //        });
            //    }
            //    catch (Exception ex)
            //    {
            //        LogHelper.InsertLogTelegram("TicketController - GetMyTickets: " + ex + "\n Token: " + token);
            //        return Ok(new { status = (int)ResponseType.FAILED, msg = "Error: " + ex.ToString() });
            //    }
            //}
            // 1) USER: list tickets của user (NO REDIS)
            [HttpPost("get-my-tickets.json")]
            public async Task<ActionResult> GetMyTickets([FromForm] string token)
            {
                try
                {
                    JArray objParr = null;
                    if (!CommonHelper.GetParamWithKey(token, out objParr, configuration["DataBaseConfig:key_api:b2c"]))
                    {
                        return Ok(new { status = (int)ResponseType.ERROR, msg = "Key ko hop le" });
                    }

                    var obj = objParr[0];

                    var user_id = obj["user_id"]?.ToString();
                    if (string.IsNullOrWhiteSpace(user_id))
                        return Ok(new { status = (int)ResponseType.ERROR, msg = "Missing user_id" });

                    int status = obj["status"] != null && int.TryParse(obj["status"]?.ToString(), out var s) ? s : -1;
                    int page = obj["page"] != null && int.TryParse(obj["page"]?.ToString(), out var p) ? p : 1;
                    int size = obj["size"] != null && int.TryParse(obj["size"]?.ToString(), out var z) ? z : 20;

                    // ✅ gọi thẳng DB
                    var data = await ticketRepository.GetMyTickets(user_id, status, page, size);

                    return Ok(new
                    {
                        status = (int)ResponseType.SUCCESS,
                        msg = "Success",
                        data
                    });
                }
                catch (Exception ex)
                {
                    try
                    {
                        LogHelper.InsertLogTelegram("TicketController - GetMyTickets(NO REDIS): " + ex + "\n Token: " + token);
                    }
                    catch { }

                    return Ok(new
                    {
                        status = (int)ResponseType.FAILED,
                        msg = "Error: " + ex.ToString()
                    });
                }
            }
        // 2) USER: create ticket
        // 2) USER: create ticket (create ticket + first message)
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
                var priority = objParr[0]["priority"] != null ? Convert.ToInt32(objParr[0]["priority"]) : 0;

                if (string.IsNullOrWhiteSpace(user_id) || string.IsNullOrWhiteSpace(subject) || string.IsNullOrWhiteSpace(content))
                    return Ok(new { status = (int)ResponseType.ERROR, msg = "Missing required fields" });

                // ✅ Create ticket + create first message => trả first_message_id
                var created = await ticketRepository.CreateTicketWithFirstMessage(user_id, service_id, department_id, subject, content, priority);

                return Ok(new
                {
                    status = (int)ResponseType.SUCCESS,
                    msg = "Success",
                    data = created
                });
            }
            catch (Exception ex)
            {
                LogHelper.InsertLogTelegram("TicketController - CreateTicket: " + ex + "\n Token: " + token);
                return Ok(new { status = (int)ResponseType.FAILED, msg = "Error: " + ex.ToString() });
            }
        }




        // 3) USER/CMS: ticket detail
        [HttpPost("get-ticket-detail.json")]
            public async Task<ActionResult> GetTicketDetail([FromForm] string token)
            {
                try
                {
                    JArray objParr = null;
                    if (!CommonHelper.GetParamWithKey(token, out objParr, configuration["DataBaseConfig:key_api:b2c"]))
                        return Ok(new { status = (int)ResponseType.ERROR, msg = "Key ko hop le" });

                    // param mẫu: { "ticket_id":"GUID", "viewer":"customer01" }
                    var ticket_id_str = objParr[0]["ticket_id"]?.ToString();
                    if (!Guid.TryParse(ticket_id_str, out var ticketId))
                        return Ok(new { status = (int)ResponseType.ERROR, msg = "Invalid ticket_id" });

                    var detail = await ticketRepository.GetTicketDetail(ticketId);

                    if (detail == null)
                        return Ok(new { status = (int)ResponseType.ERROR, msg = "Ticket not found" });

                    return Ok(new
                    {
                        status = (int)ResponseType.SUCCESS,
                        msg = "Success",
                        data = detail
                    });
                }
                catch (Exception ex)
                {
                    LogHelper.InsertLogTelegram("TicketController - GetTicketDetail: " + ex + "\n Token: " + token);
                    return Ok(new { status = (int)ResponseType.FAILED, msg = "Error: " + ex.ToString() });
                }
            }

        // 4) USER/CMS: reply chat
        [HttpPost("reply-ticket.json")]
        public async Task<ActionResult> ReplyTicket([FromForm] string token)
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

            //if (string.IsNullOrWhiteSpace(content) && string.IsNullOrWhiteSpace(content_html))
            //    return Ok(new { status = (int)ResponseType.ERROR, msg = "Empty content" });

            var message = await ticketRepository.AddMessage(ticketId, sender_type, sender_id, content, content_html);

            await _hubContext.Clients.Group($"ticket-{ticketId}")
                .SendAsync("ReceiveMessage", new
                {
                    id = message.id, // long
                    ticketId = message.ticketId,
                    senderType = message.senderType,
                    senderId = message.senderId,
                    content = message.content,
                    contentHtml = message.contentHtml,
                    createdAt = message.createdAt.ToString("dd/MM/yyyy HH:mm"),
                    attachFiles = new List<object>() // chưa có
                });

            return Ok(new { status = (int)ResponseType.SUCCESS, msg = "Success", data = message });
        }

        [HttpPost("add-message-attachments.json")]
        public async Task<ActionResult> AddMessageAttachments([FromForm] string token)
        {
            JArray objParr = null;
            if (!CommonHelper.GetParamWithKey(token, out objParr, configuration["DataBaseConfig:key_api:b2c"]))
                return Ok(new { status = (int)ResponseType.ERROR, msg = "Key ko hop le" });

            var messageId = Convert.ToInt64(objParr[0]["message_id"]);
            var attachFiles = objParr[0]["attach_files"]?.ToObject<List<AttachFileViewModel>>() ?? new();

            await ticketRepository.InsertMessageAttachments(messageId, attachFiles);

            // ✅ lấy ticketId của message để broadcast đúng group
            var ticketId = await ticketRepository.GetTicketIdByMessageId(messageId);

            // ✅ broadcast event riêng để FE update message đã render
            await _hubContext.Clients.Group($"ticket-{ticketId}")
                .SendAsync("ReceiveAttachments", new
                {
                    messageId = messageId,
                    attachFiles = attachFiles
                });

            return Ok(new { status = (int)ResponseType.SUCCESS, msg = "Success" });
        }
        [HttpPost("internal-broadcast")]
        public async Task<IActionResult> InternalBroadcast([FromBody] InternalBroadcastDto dto)
        {
            var secret = Request.Headers["X-Internal-Secret"].ToString();
            //if (secret != configuration["InternalSecret"])
            //    return Unauthorized();

            await _hubContext.Clients
                .Group($"ticket-{dto.TicketId}")
                .SendAsync("ReceiveMessage", new
                {
                    id = dto.Id,
                    ticketId = dto.TicketId,
                    senderType = dto.SenderType,
                    senderId = dto.SenderId,
                    content = dto.Content,
                    contentHtml = dto.ContentHtml,
                    createdAt = dto.CreatedAt,
                    attachFiles = dto.AttachFiles ?? new List<object>()
                });

            return Ok(new { success = true });
        }

        public class InternalBroadcastDto
        {
            public string TicketId { get; set; }
            public long Id { get; set; }
            public string SenderType { get; set; }
            public string SenderId { get; set; }
            public string Content { get; set; }
            public string ContentHtml { get; set; }
            public string CreatedAt { get; set; }
            public List<object> AttachFiles { get; set; }
        }

        // 5) CMS: list tickets (admin)
        [HttpPost("get-tickets.json")]
            public async Task<ActionResult> GetTickets([FromForm] string token)
            {
                try
                {
                    JArray objParr = null;
                    if (!CommonHelper.GetParamWithKey(token, out objParr, configuration["DataBaseConfig:key_api:b2c"]))
                        return Ok(new { status = (int)ResponseType.ERROR, msg = "Key ko hop le" });

                    // param mẫu: { "status":-1, "page":1, "size":20, "keyword":"" }
                    var status = objParr[0]["status"] != null ? Convert.ToInt32(objParr[0]["status"]) : -1;
                    var page = objParr[0]["page"] != null ? Convert.ToInt32(objParr[0]["page"]) : 1;
                    var size = objParr[0]["size"] != null ? Convert.ToInt32(objParr[0]["size"]) : 20;
                    var keyword = objParr[0]["keyword"]?.ToString();

                    var data = await ticketRepository.GetTickets(status, keyword, page, size);

                    return Ok(new
                    {
                        status = (int)ResponseType.SUCCESS,
                        msg = "Success",
                        data = data
                    });
                }
                catch (Exception ex)
                {
                    LogHelper.InsertLogTelegram("TicketController - GetTickets: " + ex + "\n Token: " + token);
                    return Ok(new { status = (int)ResponseType.FAILED, msg = "Error: " + ex.ToString() });
                }
            }
        }
    }
