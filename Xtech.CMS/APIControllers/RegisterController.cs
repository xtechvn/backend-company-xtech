using Entities.Models;
using Entities.ViewModels.CustomerManager;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json.Linq;
using Repositories.IRepositories;
using Repositories.Repositories;
using Ultilities.Constants;
using Utilities;

namespace Xtech.CMS.APIControllers
{
    [Route("api/Register")]
    [ApiController]
    public class RegisterController : ControllerBase
    {
        private readonly IClientRepository _clientRepository;
        public IConfiguration configuration;
        private IIdentifierServiceRepository _identifierServiceRepository;
        private readonly IAccountClientRepository _accountClientRepository;
        public RegisterController(IConfiguration config,IIdentifierServiceRepository identifierServiceRepository,IClientRepository clientRepository,IAccountClientRepository accountClientRepository)
        {
            configuration = config;
            _accountClientRepository = accountClientRepository;
            _clientRepository = clientRepository;
            _identifierServiceRepository = identifierServiceRepository;
        }

        [HttpPost("Register.json")]
        public async Task<IActionResult> Register([FromForm] string token) 
        {
            try
            {
                JArray objParr = null;
                if (CommonHelper.GetParamWithKey(token, out objParr, configuration["DataBaseConfig:key_api:b2c"]))
                {
                    DataClientReturnViewModel RS = new DataClientReturnViewModel();
                    var email = _clientRepository.GetClientByEmail((objParr[0]["Email"]).ToString().Trim());
                    if (email == null) 
                    {
                        Client client = new Client()
                        {
                            ClientName = (objParr[0]["ClientName"]).ToString().Trim(),
                            Email = (objParr[0]["Email"]).ToString().Trim(),
                            ClientCode = await _identifierServiceRepository.buildClientNo(Convert.ToInt32(ClientType.KL)),
                            ClientType = ClientType.KL,
                            JoinDate = DateTime.Now,
                            Phone = (objParr[0]["Phone"]).ToString().Trim(),
                            Status = (int)ClientStatus.active
                        };
                        var ClientId = await _clientRepository.SetUpClient(client);
                        AccountClient accountClient = new AccountClient()
                        {
                            ClientId = ClientId > 0 ? ClientId : null,
                            ClientType = ClientType.KL,
                            UserName = (objParr[0]["Email"]).ToString().Trim(),
                            Password = EncodeHelpers.MD5Hash((objParr[0]["Password"]).ToString().Trim()),
                            PasswordBackup = EncodeHelpers.MD5Hash((objParr[0]["ConfirmPassword"]).ToString().Trim())
                        };
                        var AccountId = await _accountClientRepository.InsertAccountClient(accountClient);
                        RS.IdAccount = AccountId;
                        RS.IdClient = ClientId;
                        RS.UserName = accountClient.UserName;
                        RS.Email = client.Email;
                        RS.ReturnUrl = "/";
                        RS.status = (int)ClientStatus.active;
                        if (RS.IdAccount > 0 && RS.IdClient > 0)
                        {
                            return Ok(new
                            {
                                status = (int)ResponseType.SUCCESS,
                                msg = "Đăng ký thành công",
                                data = RS
                            });
                        }
                    }

                    return Ok(new
                    {
                        status = (int)ResponseType.FAILED,
                        msg = "Email đã tồn tại",
                        data = RS
                    });
                }
                return Ok(new
                {
                    status = (int)ResponseType.FAILED,
                    msg = "Register.json = Lỗi khi chuyển đổi dữ liệu",
                });
            }
            catch (Exception ex)
            {

                LogHelper.InsertLogTelegram("Register.json - NewsController " + ex);
                return Ok(new
                {
                    status = (int)ResponseType.FAILED,
                    msg = "Register.json = " + ex.ToString(),
                    _token = token
                });
            }
        }
    }
}
