using Entities.ConfigModels;
using Entities.ViewModels;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Repositories.IRepositories;
using System.Security.Claims;
using Ultilities.RedisWorker;
using Utilities;
using Utilities.Contants;

namespace Repositories.Repositories.BaseRepos
{
    public class BaseRepository
    {
        protected static string _SqlServerConnectString;
        protected static SysUserModel _SysUserModel;
        private RedisConn _redisConn;

        public BaseRepository(IHttpContextAccessor context, IOptions<DataBaseConfig> dataBaseConfig, IConfiguration configuration,IUserRepository _UserRepository,
            RedisConn redisConn)
        {
            _SqlServerConnectString = dataBaseConfig.Value.SqlServer.ConnectionString;
            _redisConn = redisConn;
            Claim ClaimDepartmentId = context.HttpContext.User.FindFirst("DepartmentId");
            IEnumerable<PermissionData> permissions = null;


            int user_id = int.Parse(context.HttpContext.User.FindFirst(ClaimTypes.NameIdentifier).Value);
            string role = context.HttpContext.User.FindFirst(ClaimTypes.Role) != null ? context.HttpContext.User.FindFirst(ClaimTypes.Role).Value : "";
            string ClaimUserUnderList = "";

            try
            {
               
                //-- Get From Cache
                permissions = Enumerable.Empty<PermissionData>();
                UserRoleCacheModel user_role_cache = new UserRoleCacheModel();
                string data_json = _redisConn.Get(CacheName.USER_ROLE + user_id + "_" + configuration["CompanyType"], Convert.ToInt32(configuration["Redis:Database:db_common"]));
                if (data_json != null && data_json.Trim() != "")
                {
                    JArray objParr = null;
                    if (CommonHelper.GetParamWithKey(data_json, out objParr, configuration["DataBaseConfig:key_api:api_manual"]))
                    {
                        user_role_cache = JsonConvert.DeserializeObject<UserRoleCacheModel>(objParr[0].ToString());
                        ClaimUserUnderList = user_role_cache.UserUnderList;
                        permissions = user_role_cache.Permission;
                    }
                    //user_role_cache = JsonConvert.DeserializeObject<UserRoleCacheModel>(data_json);
                    //ClaimUserUnderList = user_role_cache.UserUnderList;
                    //permissions = user_role_cache.Permission;
                }
                else
                {
                    user_role_cache = new UserRoleCacheModel();
                    var permission = _UserRepository.GetUserPermissionById(user_id).Result;
                    if (permission != null && permission.Any())
                    {
                        user_role_cache.Permission = permission.Select(s => new PermissionData
                        {
                            MenuId = s.MenuId,
                            RoleId = s.RoleId,
                            PermissionId = s.PermissionId
                        });
                    }
                    else
                    {
                        user_role_cache.Permission = Enumerable.Empty<PermissionData>();
                    }
                    user_role_cache.UserUnderList = _UserRepository.GetListUserByUserId(user_id);
                    var data_encode = JsonConvert.SerializeObject(user_role_cache);
                    string token = CommonHelper.Encode(data_encode, configuration["DataBaseConfig:key_api:api_manual"]);
                    //string token = data_encode;
                    _redisConn.Set(CacheName.USER_ROLE + user_id + "_" + configuration["CompanyType"], token, Convert.ToInt32(configuration["Redis:Database:db_common"]));
                }
            }
            catch
            {
              
            }
            _SysUserModel = new SysUserModel
            {
                Id = user_id,
                Name = context.HttpContext.User.FindFirst(ClaimTypes.Name).Value,
                Email = context.HttpContext.User.FindFirst(ClaimTypes.Email).Value,
                Role = role,
                DepartmentId = ClaimDepartmentId != null ? int.Parse(ClaimDepartmentId.Value) : 0,
                Permissions = permissions,
                UserUnderList = ClaimUserUnderList != null ? ClaimUserUnderList : String.Empty
            };
        }
    }
}
