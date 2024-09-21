using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Repositories.IRepositories;
using Ultilities.Constants;
using Utilities;
using Xtech.CMS.Models;

namespace Xtech.CMS.Services
{
    public class APIService
    {
        private readonly IConfiguration _configuration;
        private readonly IUserRepository _userRepository;
        private HttpClient _HttpClient;
        private const string CONST_TOKEN_PARAM = "token";
        private readonly string _ApiSecretKey;
        private string USER_NAME = "test";
        private string PASSWORD = "password";
        private string API_GET_TOKEN = "/api/auth/login";
        private string TOKEN = "";
        public APIService(IConfiguration configuration, IUserRepository userRepository)
        {
            _configuration = configuration;
            _userRepository = userRepository;
            _HttpClient = new HttpClient(new HttpClientHandler
            {
                ServerCertificateCustomValidationCallback = (message, certificate2, arg3, arg4) => true
            })
            {
                BaseAddress = new Uri(configuration["API:Domain"])
            };
            _ApiSecretKey = configuration["API:SecretKey"];
            API_GET_TOKEN = configuration["API:GetToken"];
            USER_NAME = configuration["API:username"];
            PASSWORD = configuration["API:password"];

        }
        public async Task<int> SendMailResetPassword(string email)
        {
            try
            {
                HttpClient httpClient = new HttpClient();
                var j_param = new Dictionary<string, string>()
                {
                {"template_type","2" },
                {"email", email},
                };
                var data = JsonConvert.SerializeObject(j_param);
                var a = _configuration["DataBaseConfig:key_api:api_manual"];
                var token = EncodeHelpers.Encode(data, _configuration["DataBaseConfig:key_api:api_manual"]);
                var request = new FormUrlEncodedContent(new[]
                    {
                    new KeyValuePair<string, string>("token",token)
                });
                var url = ReadFile.LoadConfig().API_URL + ReadFile.LoadConfig().API_Send_Email_Reset_Password;
                var response = await httpClient.PostAsync(url, request);


                if (response.IsSuccessStatusCode)
                {

                    return 0;
                }

                return 1;
            }
            catch (Exception ex)
            {
                LogHelper.InsertLogTelegram("apisever:" + ex.ToString());
                return 1;
            }
        } 

        public async Task<string> GetToken()
        {
            try
            {
                var request = new UserLoginAPIModel()
                {
                    Username = USER_NAME,
                    Password = PASSWORD
                };
                var request_message = new HttpRequestMessage(HttpMethod.Post, API_GET_TOKEN);
                var content = new StringContent(JsonConvert.SerializeObject(request), null, "application/json");
                request_message.Content = content;
                var response = await _HttpClient.SendAsync(request_message);
                response.EnsureSuccessStatusCode();
                if (response.IsSuccessStatusCode)
                {
                    var json = JObject.Parse(await response.Content.ReadAsStringAsync());
                    var status = int.Parse(json["status"].ToString());
                    if (status != (int)ResponseType.SUCCESS)
                    {
                        LogHelper.InsertLogTelegram("GetToken - APIService:" + json["msg"].ToString());
                    }
                    else
                    {
                        return json["token"].ToString();
                    }

                }
            }
            catch (Exception ex)
            {
                LogHelper.InsertLogTelegram("GetToken - APIService:" + ex.ToString());

            }
            return null;

        }
        
       
        public class UserLoginAPIModel
        {
            public string Username { get; set; }
            public string Password { get; set; }
        }
    }
}
