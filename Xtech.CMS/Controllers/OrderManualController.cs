using Catching.Elasticsearch;
using Entities.ViewModels.Elasticsearch;
using Microsoft.AspNetCore.Mvc;
using Repositories.IRepositories;
using System.Security.Claims;
using Ultilities.Constants;
using Utilities;

namespace Xtech.CMS.Controllers
{
    public class OrderManualController : Controller
    {
        private UserESRepository _userESRepository;
        private readonly IConfiguration _configuration;
        private readonly IUserRepository _userRepository;
        public OrderManualController(IConfiguration configuration,IUserRepository userRepository)
        {
            _configuration = configuration;
            _userRepository = userRepository;
            _userESRepository = new UserESRepository(_configuration["DataBaseConfig:Elastic:Host"], configuration);
        }

        [HttpPost]
        public async Task<IActionResult> UserSuggestion(string txt_search, int service_type = 0)
        {

            try
            {
                long _UserId = 0;
                if (HttpContext.User.FindFirst(ClaimTypes.NameIdentifier) != null)
                {
                    _UserId = Convert.ToInt64(HttpContext.User.FindFirst(ClaimTypes.NameIdentifier).Value);
                }
                if (txt_search == null) txt_search = "";
                var data = await _userESRepository.GetUserSuggesstion(txt_search);
                if (data == null || data.Count <= 0)
                {
                    var data_sql = await _userRepository.GetUserSuggesstion(txt_search);
                    data = new List<UserESViewModel>();
                    if (data_sql != null && data_sql.Count > 0)
                    {
                        data.AddRange(data_sql.Select(x => new UserESViewModel() { email = x.Email, fullname = x.FullName, id = x.Id, phone = x.Phone, username = x.UserName, _id = x.Id }));
                    }
                }

                return Ok(new
                {
                    status = (int)ResponseType.SUCCESS,
                    data = data,
                    selected = _UserId
                });

            }
            catch (Exception ex)
            {
                LogHelper.InsertLogTelegram("UserSuggestion - OrderManualController: " + ex.ToString());
                return Ok(new
                {
                    status = (int)ResponseType.SUCCESS,
                    data = new List<CustomerESViewModel>()
                });
            }

        }
    }
}
