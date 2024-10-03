using Entities.ConfigModels;
using Entities.Models;
using Entities.ViewModels;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json.Linq;
using Repositories.IRepositories;
using Repositories.Repositories;
using System.Security.Claims;
using Ultilities.RedisWorker;
using Utilities;
using Utilities.Contants;
using Newtonsoft.Json;
using Entities.ViewModels.Login;
using Ultilities.Constants;
using Entities.ViewModels.CustomerManager;

namespace Xtech.CMS.APIControllers
{
    [Route("api/login")]
    [ApiController]
    public class LoginController : ControllerBase
    {
        private readonly IClientRepository _clientRepository;
        private readonly ICustomerManagerRepository _customerManagerRepository;
        public IConfiguration _configuration;
        private RedisConn _redisConn;
        public LoginController(IConfiguration config, IClientRepository clientRepository, ICustomerManagerRepository customerManagerRepository, RedisConn redisConn)
        {
            _clientRepository = clientRepository;
            _configuration = config;
            _redisConn = redisConn;
            _customerManagerRepository = customerManagerRepository;

        }


        [HttpPost("ConfirmLogin.json")]
        [AllowAnonymous]
        public async Task<IActionResult> ConfirmLogin(AccountModel model)
        {
            try
            {
                //-- Validate Input
                if (model == null || model.UserName == null || model.UserName.Trim() == "" || model.Password == null || model.Password.Trim() == "")
                {
                    return Ok(new
                    {
                        status = (int)ResponseType.FAILED,
                        msg = "Tài khoản / Mật khẩu không được để trống, vui lòng thử lại"
                    });
                }
                //-- Bỏ ký tự đặc biệt
                model.ReturnUrl = CommonHelper.RemoveAllSpecialCharacterinURL(model.ReturnUrl);
                model.UserName = CommonHelper.RemoveAllSpecialCharacterLogin(model.UserName);
                model.UserName = model.UserName.Replace("+", "").Replace("//", "").Replace("=", "");
                model.Password = CommonHelper.RemoveAllSpecialCharacterLogin(model.Password);
                //-- Kiểm tra user/pass
                var client = await _customerManagerRepository.CheckExistAccount(model);
                DataClientReturnViewModel datareturn = new DataClientReturnViewModel() 
                {
                    Id = (int)client.Id,
                    ClientName = client.ClientName,
                    Email = client.Email,
                    ReturnUrl = model.ReturnUrl ?? "/",

                };
                if (client == null || client.Id <= 0)
                {
                    //-- Nếu tài khoản bị khóa
                    if (client.Status != 0)
                    {
                        return Ok(new
                        {
                            status = (int)ResponseType.FAILED,
                            msg = "Tài khoản của bạn đã bị khóa, vui lòng liên hệ IT",
                        });
                    }
                    return Ok(new
                    {
                        status = (int)ResponseType.SUCCESS,
                        msg = "Đăng nhập thành công",
                        data = datareturn
                    });
                }
            }
            catch (Exception ex)
            {
                LogHelper.InsertLogTelegram("ConfirmLogin - LoginController" + ex);
            }
            return Ok(new
            {
                status = (int)ResponseType.FAILED,
                msg = "Có lỗi xảy ra trong quá trình đăng nhập, vui lòng liên hệ IT"
            });
        }
    }
}
