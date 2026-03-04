// CMS - TicketSSEController.cs
// SSE endpoint cho CMS browser kết nối nhận tin nhắn realtime
// Subscribe Redis channel TICKET_{ticketId} => stream SSE xuống browser

using Microsoft.AspNetCore.Mvc;
using StackExchange.Redis;
using System.Collections.Concurrent;
using Utilities;

namespace Xtech.CMS.Controllers.Tickets
{
    public class TicketSSEController : Controller
    {
        private readonly IConfiguration _configuration;
        private readonly ISubscriber _subscriber;

        public TicketSSEController(IConfiguration configuration)
        {
            _configuration = configuration;

            var redisConn = ConnectionMultiplexer.Connect(
                _configuration["Redis:Host"] + ":" + _configuration["Redis:Port"]);
            _subscriber = redisConn.GetSubscriber();
        }

        // =====================================================================
        // GET /Ticket/Stream?ticketId=xxx
        // CMS browser kết nối SSE vào đây
        // =====================================================================
        [HttpGet]
        public async Task Stream(string ticketId)
        {
            if (string.IsNullOrWhiteSpace(ticketId))
            {
                Response.StatusCode = 400;
                return;
            }

            // SSE headers
            Response.Headers.Add("Content-Type", "text/event-stream");
            Response.Headers.Add("Cache-Control", "no-cache");
            Response.Headers.Add("X-Accel-Buffering", "no"); // tắt buffer Nginx

            var dataQueue = new ConcurrentQueue<string>();

            // Subscribe Redis channel TICKET_{ticketId}
            await _subscriber.SubscribeAsync($"TICKET_{ticketId}", (channel, message) =>
            {
                // ✅ Thêm dòng này để kiểm tra có nhận được không
                LogHelper.InsertLogTelegram($"CMS SSE nhận Redis message: {message}");
                dataQueue.Enqueue(message!);
            });

            try
            {
                while (!HttpContext.RequestAborted.IsCancellationRequested)
                {
                    while (dataQueue.TryDequeue(out var message))
                    {
                        // SSE format: "data: {json}\n\n"
                        var sseData = $"data: {message}\n\n";
                        var buffer = System.Text.Encoding.UTF8.GetBytes(sseData);

                        await Response.Body.WriteAsync(buffer, 0, buffer.Length);
                        await Response.Body.FlushAsync();
                    }

                    await Task.Delay(20, HttpContext.RequestAborted);
                }
            }
            catch (OperationCanceledException)
            {
                // Client ngắt kết nối - bình thường
            }
            catch (Exception ex)
            {
                LogHelper.InsertLogTelegram("TicketSSEController.Stream: " + ex);
            }
            finally
            {
                await _subscriber.UnsubscribeAsync($"TICKET_{ticketId}");
            }
        }
    }
}