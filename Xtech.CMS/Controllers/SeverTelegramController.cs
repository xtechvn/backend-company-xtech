using System;
using System.Threading.Tasks;
using Entities.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using Repositories.IRepositories;
using Ultilities.Constants;
using Ultilities.RedisWorker;
using Utilities;
using Utilities.Contants;
using Entities.ViewModels;

namespace Xtech.CMS.Controllers
{
    // Đặt lại tên class cho khớp log + constructor
    public class SeverTelegramController : Controller
    {
        private readonly ITelegramRepository _telegramRepository;
        private readonly IConfiguration _configuration;
        private readonly RedisConn _redisConn;

        public SeverTelegramController(ITelegramRepository telegramRepository, IConfiguration configuration)
        {
            _telegramRepository = telegramRepository;
            _configuration = configuration;
            _redisConn = new RedisConn(configuration);
            _redisConn.Connect();
        }

        public IActionResult Index()
        {
            return View();
        }

        [HttpPost]
        public IActionResult BotDetail(int id)
        {
            TeleBotServer model;

            if (id != 0)
            {
                model = _telegramRepository.GetTelegrambyid(id);
                if (model == null)
                {
                    model = new TeleBotServer();
                }
            }
            else
            {
                model = new TeleBotServer();
            }

            return PartialView(model); // BotDetail.cshtml phải là @model TeleBotServer
        }


        [HttpPost]
        public async Task<IActionResult> AddBot(string data)
        {
            int stt_code = (int)ResponseType.FAILED;
            string msg = "Error On Execution";

            try
            {
                var model = JsonConvert.DeserializeObject<TeleBotServer>(data);

                // ❗ Model mới: validate theo Name (và các field bắt buộc khác nếu cần)
                if (model != null && !string.IsNullOrWhiteSpace(model.Name))
                {
                    var result = await _telegramRepository.AddTelegram(model);
                    if (result == 0)
                    {
                        // Cập nhật lại cache Redis
                        try
                        {
                            var cache_name = CacheName.CACHE_TELEGRAM_LIST;
                            var dbIndex = Convert.ToInt32(_configuration["DataBaseConfig:Redis:Database:db_common"]);

                            _redisConn.clear(cache_name, dbIndex);
                            var list = _telegramRepository.GetAllcodeTelegram();
                            _redisConn.Set(cache_name, JsonConvert.SerializeObject(list), dbIndex);
                        }
                        catch (Exception ex)
                        {
                            LogHelper.InsertLogTelegram("AddBot - SeverTelegramController - Set Redis: " + ex.ToString());
                        }

                        stt_code = (int)ResponseType.SUCCESS;
                        msg = "Thêm mới/Cập nhật thông tin thành công";
                    }
                    else
                    {
                        stt_code = (int)ResponseType.FAILED;
                        msg = "Không thể lưu dữ liệu, vui lòng thử lại";
                    }
                }
                else
                {
                    stt_code = (int)ResponseType.FAILED;
                    msg = "Dữ liệu gửi lên không chính xác, vui lòng kiểm tra lại";
                }

            }
            catch (Exception ex)
            {
                LogHelper.InsertLogTelegram("AddBot - SeverTelegramController: " + ex);
                stt_code = (int)ResponseType.ERROR;
                msg = "Lỗi kỹ thuật vui lòng liên hệ bộ phận IT";
            }

            return Ok(new
            {
                stt_code = stt_code,
                msg = msg,
                data = data
            });
        }

        // Cái này vẫn là gọi API Telegram thật, để nguyên logic (vì nó không dính DB model mới)
        [HttpPost]
        //public async Task<string> GetGrouplogname(string dataapi)
        //{
        //    TelegramviewModel list = new TelegramviewModel();
        //    try
        //    {
        //        var conten = JsonConvert.DeserializeObject<Telegramapi>(dataapi);
        //        HttpClient httpClient = new HttpClient();
        //        var dommain = _configuration["BotSetting:domain"];
        //        var apiPrefix = dommain + conten.token + "/getChat?chat_id=" + conten.groupid + "";
        //        var rs = await httpClient.GetAsync(apiPrefix);
        //        var rs_content = JsonConvert.DeserializeObject<TelegramviewModel>(rs.Content.ReadAsStringAsync().Result);
        //        list = rs_content;
        //        if (list.result != null)
        //        {
        //            string groupname = list.result.title;
        //            return groupname;
        //        }
        //    }
        //    catch (Exception ex)
        //    {
        //        LogHelper.InsertLogTelegram("GetGrouplogname - SeverTelegramController: " + ex);
        //    }
        //    return null;
        //}

        // Search với model mới:
        // - TokenName  -> name
        // - Projectmodel bỏ luôn
        // - statusmodel -> status
        [HttpPost]
        public IActionResult Search(string name, int status = -1, int currentPage = 1, int pageSize = 20)
        {
            var model = new GenericViewModel<TeleBotServer>();
            try
            {
                model = _telegramRepository.GetTelegramPagingList(name, status, currentPage, pageSize);
            }
            catch (Exception ex)
            {
                LogHelper.InsertLogTelegram("Search - SeverTelegramController: " + ex);
            }
            return PartialView(model);
        }
    }
}
