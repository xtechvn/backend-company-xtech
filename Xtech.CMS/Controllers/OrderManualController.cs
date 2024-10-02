using Catching.Elasticsearch;
using Entities.Models;
using Entities.ViewModels.Elasticsearch;
using Microsoft.AspNetCore.Mvc;
using Repositories.IRepositories;
using Repositories.Repositories;
using System.Security.Claims;
using Ultilities.Constants;
using Utilities;
using Utilities.Contants;

namespace Xtech.CMS.Controllers
{
    public class OrderManualController : Controller
    {
        private UserESRepository _userESRepository;
        private readonly IConfiguration _configuration;
        private readonly IUserRepository _userRepository;
        private readonly IAllCodeRepository _allCodeRepository;
        private readonly IOrderRepository _orderRepository;
        private OrderESRepository _orderESRepository;
        public OrderManualController(IConfiguration configuration,IUserRepository userRepository, IAllCodeRepository allCodeRepository, IOrderRepository orderRepository)
        {
            _configuration = configuration;
            _userRepository = userRepository;
            _orderESRepository = new OrderESRepository(_configuration["DataBaseConfig:Elastic:Host"], configuration);
            _userESRepository = new UserESRepository(_configuration["DataBaseConfig:Elastic:Host"], configuration);
            _allCodeRepository = allCodeRepository;
            _orderRepository = orderRepository;

        }
        [HttpPost]
        public IActionResult CreateOrderManual()
        {
            ViewBag.Branch = _allCodeRepository.GetListByType(AllCodeType.BRANCH_CODE);
            return View();
        }
        [HttpPost]
        public async Task<IActionResult> CreateOrder(Order order) 
        {
            try
            {
                order.CreatedBy = Convert.ToInt32(HttpContext.User.FindFirst(ClaimTypes.NameIdentifier).Value);
                var id = await _orderRepository.CreateOrder(order);
                return Ok(new
                {
                    status = (int)ResponseType.SUCCESS,
                    order_id = id
                });

            }
            catch (Exception ex)
            {
                LogHelper.InsertLogTelegram("CreateOrder - OrderManualController: " + ex.ToString());
                return Ok(new
                {
                    status = (int)ResponseType.SUCCESS,
                });
            }
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

        public async Task<IActionResult> OrderNoSuggestion(string txt_search)
        {

            try
            {
                long _UserId = 0;
                var data = new List<OrderElasticsearchViewModel>();
                if (HttpContext.User.FindFirst(ClaimTypes.NameIdentifier) != null)
                {
                    _UserId = Convert.ToInt64(HttpContext.User.FindFirst(ClaimTypes.NameIdentifier).Value);
                }
                if (txt_search != null)
                {
                    data = await _orderESRepository.GetOrderNoSuggesstion(txt_search);
                    return Ok(new
                    {
                        status = (int)ResponseType.SUCCESS,
                        data = data,
                        selected = _UserId
                    });
                }
                else
                {
                    return Ok(new
                    {
                        status = (int)ResponseType.SUCCESS,
                        data = new List<OrderElasticsearchViewModel>()
                    });
                }
            }
            catch (Exception ex)
            {
                LogHelper.InsertLogTelegram("OrderNoSuggestion - OrderManualController: " + ex.ToString());
                return Ok(new
                {
                    status = (int)ResponseType.SUCCESS,
                    data = new List<OrderElasticsearchViewModel>()
                });
            }

        }
    }
}
