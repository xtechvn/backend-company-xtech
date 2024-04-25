using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json.Linq;
using Repositories.IRepositories;
using Repositories.Repositories;
using Ultilities.Constants;
using Ultilities.RedisWorker;
using Utilities.Contants;
using Utilities;

using Newtonsoft.Json;
using Entities.Models;
using Entities.ViewModels.Galaxy;

namespace Xtech.CMS.APIControllers
{
    [Route("api/galaxy")]
    [ApiController]
    public class GalaxyController : ControllerBase
    {
 
        public IConfiguration configuration;
        public IBookingVPSRepository _bookingVPSRepository;
         
        public GalaxyController(IConfiguration config, IBookingVPSRepository bookingVPSRepository)
        {
            configuration = config;
            
            _bookingVPSRepository = bookingVPSRepository;
        }
        [HttpPost("booking-galaxy.json")]
        public async Task<ActionResult> BookingGalaxy([FromForm] string token)
        {
            try
            {
                //string j_param = "{'Memory':1,'CPU':1,'SSD':1,'net':1,'nip':1,'nMonth':1,'quantity':1,'Clientid':1,'Amount':1}";
                //token = CommonHelper.Encode(j_param, configuration["DataBaseConfig:key_api:b2c"]);
                JArray objParr = null;
                if (CommonHelper.GetParamWithKey(token, out objParr, configuration["DataBaseConfig:key_api:b2c"]))
                {
                    var data = new GalaxyViewModel();
                    data.CPU= Convert.ToInt32(objParr[0]["CPU"]);
                    data.Memory= Convert.ToInt32(objParr[0]["Memory"]);
                    data.SSD= Convert.ToInt32(objParr[0]["SSD"]);
                    data.net= Convert.ToInt32(objParr[0]["net"]);
                    data.nip= Convert.ToInt32(objParr[0]["nip"]);
                    data.nMonth= Convert.ToInt32(objParr[0]["nMonth"]);
                    data.quantity= Convert.ToInt32(objParr[0]["quantity"]);
                    data.Clientid= Convert.ToInt32(objParr[0]["Clientid"]);
                    data.Amount= Convert.ToInt32(objParr[0]["Amount"]);
                    if (data != null)
                    {
                        var booking = _bookingVPSRepository.bookingVPS(data);
                        if (booking > 0)
                        {
                            return Ok(new
                            {
                                status = (int)ResponseType.SUCCESS,
                                msg = "Đăng ký thành công ",
                            });
                        }
                    }
                    return Ok(new
                    {
                        status = (int)ResponseType.FAILED,
                        msg = "Đăng ký không thành công ",
                    });
                }
                else
                {
                    return Ok(new
                    {
                        status = (int)ResponseType.ERROR,
                        msg = "Key không hợp lệ" ,
                    });
                }

              
            }
            catch (Exception ex)
            {
                LogHelper.InsertLogTelegram("BookingGalaxy - GalaxyController: " + ex + "\n Token: " + token);
                return Ok(new
                {
                    status = (int)ResponseType.FAILED,
                    msg = "Error: " + ex.ToString(),
                });
            }
        }

    }
}
