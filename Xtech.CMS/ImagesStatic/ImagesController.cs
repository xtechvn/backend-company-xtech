using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Utilities;
using System.Diagnostics;
using ENTITIES.ViewModels.Static;
using Ultilities.Constants;
using Repositories;
using Entities.ViewModels.Static;

namespace Utilities
{

    [Route("static/[controller]")]
    [ApiController]
    public class ImagesController : ControllerBase
    {
        private IConfiguration configuration;
        private readonly IImagesConvertRepository imagesConvertRepository;
        private readonly string[] stringArray = { "PNG", "JPG", "JPEG", "GIF", "BMP" };
        public ImagesController(IConfiguration _configuration, IImagesConvertRepository _imagesConvertRepository)
        {
            configuration = _configuration;
            imagesConvertRepository = _imagesConvertRepository;
        
        }
        [HttpPost("convert-image-url")]
        public async Task<IActionResult> ConvertImageURL([FromForm]string token)
        {
            try
            {
                //#region Test
                //var j_param = new Dictionary<string, object>
                //{
                //    {"urls", "https://statics.product.cloudhms.io/customer-files/38643830-6332-3635-2d36-6439382d6463/2020/08/13/043825-14014d8b-5755-49ce-80e1-926abf993996.jpg"},
                //                    };
                //var data_product = JsonConvert.SerializeObject(j_param);
                //token = CommonHelper.Encode(data_product, configuration["key_api:b2b"]);
                //#endregion
                Stopwatch sw = new Stopwatch();
                sw.Restart();
                string imgPath_base = "\\" + configuration["File:MainFolder"] + "\\" + configuration["File:Images"];
                string imgPath_url_base = configuration["URLs:Images"]; // @ConfigurationManager.AppSettings["img_url_base"];
                //Max Size file có thể upload: mặc định là 3MB
                int max_file_size = 3 * 1024 * 1024;

                JArray objParr = null;
                bool response_queue = false;
                if (CommonHelper.GetParamWithKey(token, out objParr, configuration["DataBaseConfig:key_api:b2b"]))
                {
                    string urls = objParr[0]["urls"].ToString();
                    if(urls==null|| urls.Trim() == "")
                    {
                        return Ok(new
                        {
                            status = (int)Ultilities.Constants.ResponseType.FAILED,
                            message = "Data invalid!"
                        });
                    }
                    var images_url = urls.Split(",");
                    List<ImagesConvertMongoDbModel> converted_url = new List<ImagesConvertMongoDbModel>();
                    if (images_url.Count() > 0)
                    {
                        foreach(var img in images_url)
                        {
                            if (img.Trim() == "" || img == null) continue;
                            var exists = imagesConvertRepository.GetImageByURL(img);
                           // ImagesConvertMongoDbModel exists =null;
                            if (exists == null)
                            {
                                using (var httClient = new HttpClient())
                                {
                                    var imageBytes = await httClient.GetByteArrayAsync(img);
                                    var part = img.Split(".");
                                    string file_type = "";
                                    ImageDetail img_detail = new ImageDetail()
                                    {
                                        data_file = Convert.ToBase64String(imageBytes),
                                        extend = part[part.Length - 1]
                                    };
                                    var data_file = ImageUploadHelper.ResizeBase64ImageToWidth(Convert.ToBase64String(imageBytes), out file_type,250);
                                   
                                    if (data_file != null)
                                    {
                                        img_detail = new ImageDetail()
                                        {
                                            data_file = data_file,
                                            extend = file_type
                                        };
                                    }
                                   

                                    string c_url = ImageUploadHelper.SaveFile(img_detail, imgPath_base, imgPath_url_base, max_file_size);
                                    ImagesConvertMongoDbModel mongo_model = new ImagesConvertMongoDbModel()
                                    {
                                        converted_url = c_url,
                                        orginal_url = img,
                                    };
                                    mongo_model.GenID();
                                    await imagesConvertRepository.InsertImage(mongo_model);
                                    converted_url.Add(mongo_model);
                                }
                            }
                            else
                            {
                                converted_url.Add(exists);
                            }
                           
                        }
                    }
                    sw.Stop();
                    return Ok(new
                    {
                        status = (int)ResponseType.SUCCESS,
                        message = "Success",
                        data= converted_url.Select(x=>x.converted_url).ToList(),
                        time = sw.ElapsedMilliseconds + " ms"
                    });
                }
                else
                {
                    return Ok(new
                    {
                        status = (int)ResponseType.ERROR,
                        message = "Key invalid!"
                    });
                }
            }
            catch (Exception e)
            {
                //Lỗi trên API
                return BadRequest(new { status = (int)ResponseType.ERROR, message = "On Execution", url_path = "" });
            }
        }

        // GET: MediaController

        [HttpPost("upload")]
        public IActionResult Upload([FromBody] object body)
        {
            try
            {
                var Jobject = JObject.Parse(body.ToString());
                string token = Jobject["token"].ToString();

                string[] stringArray = { "PNG", "JPG", "JPEG", "GIF", "BMP" };
                //Max Size file có thể upload: mặc định là 3MB
                int max_file_size = 3 * 1024 * 1024;

                try
                {
                    if (Convert.ToInt32(configuration["ConfigSize:General"]) > 0)
                    {
                        max_file_size = Convert.ToInt32(configuration["ConfigSize:General"]);
                    }
                }
                catch (FormatException)
                {

                }

                //Decode token để lấy JSON:
                string param = CommonHelper.Decode(token, configuration["DataBaseConfig:key_api:Static"]);

                //Model hóa JSON:
                ImageDetail img_detail = JsonConvert.DeserializeObject<ImageDetail>(param);

                // Nếu extend thuộc ảnh
                if (!stringArray.Contains(img_detail.extend.ToUpper()))
                {
                    //Trả kết quả sai file type:
                    return BadRequest(new { status = (int)ResponseType.ERROR, message = "Invalid File Type", url_path = "" });
                }

                //Nếu lấy ra được thông tin:
                if (ImageUploadHelper.IsBase64String(img_detail.data_file))
                {
                    string root =  configuration["File:MainFolder"] ;
                    List<string> child = new List<string>();
                    child.Add(configuration["File:Images"]);
                   
                    //Lấy thông tin thời gian hiện tại
                    DateTime time = DateTime.Now;
                    string year = time.Year.ToString();
                    string month = time.Month.ToString();
                    string day = time.Day.ToString();
                    //Thông tin file và build đường dẫn local:
                    child.Add(year);
                    child.Add(month);
                    child.Add(day);

                    string file_name = Guid.NewGuid() + "." + img_detail.extend;
                    string folder = FileService.CheckAndCreateFolder(root, child);



                    string imgPath_full = folder+"\\" + file_name;
                    byte[] bytes = System.Convert.FromBase64String(img_detail.data_file);
                    //Kiểm tra nếu file vượt quá max size:
                    if (bytes.Length > max_file_size)
                    {
                        return BadRequest(new { status = (int)ResponseType.ERROR, message = "The file image exceeds the maximum allowed size: " + max_file_size + " bytes", file_path = file_name, url_path = "" });
                    }
                    else
                    {
                        //Ghi byte[] vào file đã tạo:
                        using (var fs = new FileStream(imgPath_full, FileMode.Create, FileAccess.Write))
                        {
                            fs.Write(bytes, 0, bytes.Length);
                        }
                    }

                    //Build đường link local:
                    string urlPath_full = FileService.BuildURLFromPath(imgPath_full);

                    //Trả kết quả
                    return Ok(new { status = (int)ResponseType.SUCCESS, message = "Images Received", url_path = urlPath_full});
                }
                //Thông tin không được encode với key trong file .config hoặc thông tin convert ra null:
                else
                {
                    return BadRequest(new { status = (int)ResponseType.ERROR, message = "Invalid Format", url_path = "" });
                }
            }
            catch (Exception e)
            {
                //Log Telegram:
                //LogHelper.InsertLogTelegram("ServiceReceiverMedia - Upload Error: " + e.ToString());

                //Lỗi trên API
                return BadRequest(new { status = (int)ResponseType.ERROR, message = "On Execution", url_path = "" });
            }
        }
        [HttpPost("upload-payment")]
        public IActionResult UploadPaymentImage([FromBody] object body)
        {
            try
            {
                var Jobject = JObject.Parse(body.ToString());
                string token = Jobject["token"].ToString();
                /*
                var object_input= new PaymentImageDetail()
                {
                    data_file = "iVBORw0KGgoAAAANSUhEUgAAALwAAABSCAIAAABhZSkOAAAAAXNSR0IArs4c6QAAAARnQU1BAACxjwv8YQUAAAAJcEhZcwAADsMAAA7DAcdvqGQAAAPOSURBVHhe7ZpNjuIwEEbnICw5DSvOwoKzIPVREEdpbtALxAIhITF2lePYIZB8zEzTTN6TFyRx/PtSdiJ+XQFEkAZkkAZkkAZkkAZkkAZkkAZkkAZkkAZkkAZknpTmvJp/zZbHzT4dw5T4g0jz+XGYLU+f6Qimw7PSbNch0hxWu0s6hgnxrDQhzCw+MGaafPdG+LJZfs3mx206hHfkOWn2p0XYCM+/9GCDNCVhNA6z9TkdvQ1PSRO3wPPjKm5r1I0w0pTYS+g0pLGJD13dHWfzg/jWjTQl05HG1qbVLv2oVyhzIoSf/XmzDtHIlrB1GY2yNJc7GQZpqnhYwufutAqR364Gsxfrc85gYfJ2YfUvT7Ecj6PbcCY21XLuQ2mxnNjrTNHHmxfJgXGI755+YzepD+FL0KWxMfW+5fnLuBMH3/G0qX2Y/JbDImbrzTBIU0J5e0hlM5otV5VyFX61avbVoqY9CU0H2xYuj26P/c53mWR1KkQcGIepSVOJUgjk+GDFzqcnb++Dm9ejwQyD9JTgM9Q2IwSG9WnbHnarsDmrpsfOpAweijxDM7vhkteb8vj5NrqEKoqro7tpJ///5amzJHVXqDRYZRivZ2gwwyBeQp3f4sTNitNSOhHp5o+Tlw9dGj90OYrfXkjfZBexanQ3pyHNTWixbrdB2werep7qwRrMMEhPCTfuhj3NsbsCVrdYIXfiZXlYtq2Vpnf5s9S0YWQ3JyGNj8VtymMxOFgjR/MBPSV0pGnWl06qbinMsAKLmUOaISRp/sJg/RNp3JJmLbCZCAXmDUc535nsmf0o15FhaayK7GgfijSdLfkboEhTz02m7Pk3StPkjytRtCSX6dLkDJftRydDwiXYhE7V0zZCGm9D3OTeme+R3UzllH6/A4o0xaiVND2PwzE4WCNH8wFeQjcVKvdn6Gm5bV1D6sSMEdLcC7q5irHdtJNlGj8OL0SQxp7gvgW4iEAvkGaxPN48qe13P//sto0trCo1PCZ1qx4lTSC+Zje1pJSvju/mZbsuP+eMH4cXom2EfwI98/Esdx8DeMhkpUmf497iyf5pTE8aWyZSevgGBPeYrDT8V/V53k8aeDlIAzJIAzJIAzJIAzJIAzJIAzJIAzJIAzJIAzJIAzJIAzJIAzJIAzJIAzJIAzJIAzJIAzJIAzJIAzJIAzJIAzJIAzJIAzJIAzJIAzJIAzJIAzJIAzJIAzJIAzJIAzJIAzJIAzJIAzJIAzJIAzJIAzJIAzJIAzJIAzJIAzJIAzJIAzJIAzJIAzJIAzJIAzJIAzJIAyLX629jb4GkrwldmwAAAABJRU5ErkJggg==",
                    extend="jpg"
                };
                token = CommonHelper.Encode(JsonConvert.SerializeObject(object_input), configuration["key_api:b2b"]);
                */

                //Array kiểm tra nếu file name là file ảnh thông thường.
                string[] stringArray = { "PNG", "JPG", "JPEG", "GIF", "BMP" };
                //Max Size file có thể upload: mặc định là 6MB
                int max_file_size = 6 * 1024 * 1024;

                try
                {
                    if (Convert.ToInt32(configuration["ConfigSize:General"]) > 0)
                    {
                        max_file_size = Convert.ToInt32(configuration["ConfigSize:General"]);
                    }
                }
                catch (FormatException)
                {

                }

                //Decode token để lấy JSON:
                string param = CommonHelper.Decode(token, configuration["DataBaseConfig:key_api:b2b"]);

                //Model hóa JSON:
                PaymentImageDetail img_detail = JsonConvert.DeserializeObject<PaymentImageDetail>(param);

                // Nếu extend thuộc video
                if (!stringArray.Contains(img_detail.extend.ToUpper()))
                {
                    //Trả kết quả sai file type:
                    return BadRequest(new { status = (int)ResponseType.ERROR, message = "Invalid File Type", url_path = "" });
                }

                //Nếu lấy ra được thông tin:
                if (ImageUploadHelper.IsBase64String(img_detail.data_file))
                {
                    string root = configuration["File:MainFolder"];
                    List<string> child = new List<string>();
                    child.Add(configuration["File:Images"]);

                    //Lấy thông tin thời gian hiện tại
                    DateTime time = DateTime.Now;
                    string year = time.Year.ToString();
                    string month = time.Month.ToString();
                    string day = time.Day.ToString();
                    //Thông tin file và build đường dẫn local:
                    child.Add(year);
                    child.Add(month);
                    child.Add(day);

                    string file_name = Guid.NewGuid() + "." + img_detail.extend;
                    string folder = FileService.CheckAndCreateFolder(root, child);



                    string imgPath_full = folder + "\\" + file_name;

                    byte[] bytes = System.Convert.FromBase64String(img_detail.data_file);
                    //Kiểm tra nếu file vượt quá max size:
                    if (bytes.Length > max_file_size)
                    {
                        return BadRequest(new { status = (int)ResponseType.ERROR, message = "The file image exceeds the maximum allowed size: " + max_file_size + " bytes", file_path = file_name, url_path = "" });
                    }
                    else
                    {
                        //Ghi byte[] vào file đã tạo:
                        using (var fs = new FileStream(imgPath_full, FileMode.Create, FileAccess.Write))
                        {
                            fs.Write(bytes, 0, bytes.Length);
                        }
                    }

                    //Build đường link local:
                    string urlPath_full = FileService.BuildURLFromPath(imgPath_full);

                    //Trả kết quả
                    return Ok(new { status = (int)ResponseType.SUCCESS, message = "Images Received", url_path = urlPath_full});
                }
                //Thông tin không được encode với key trong file .config hoặc thông tin convert ra null:
                else
                {
                    return BadRequest(new { status = (int)ResponseType.FAILED, message = "Invalid Format", url_path = "" });
                }
            }
            catch (Exception e)
            {
                //Log Telegram:
               // LogHelper.InsertLogTelegram("ServiceReceiverMedia - Upload Error: " + e.ToString());

                //Lỗi trên API
                return BadRequest(new { status = (int)ResponseType.ERROR, message = "On Execution", url_path = "" });
            }
        }

        [HttpPost("upload-with-name")]
        public IActionResult UploadAndKeepFileName([FromBody] object body)
        {
            try
            {
                var Jobject = JObject.Parse(body.ToString());
                string token = Jobject["token"].ToString();

                string[] stringArray = { "PNG", "JPG", "JPEG", "GIF", "BMP" };
                //Max Size file có thể upload: mặc định là 3MB
                int max_file_size = 3 * 1024 * 1024;

                try
                {
                    if (Convert.ToInt32(configuration["ConfigSize:General"]) > 0)
                    {
                        max_file_size = Convert.ToInt32(configuration["ConfigSize:General"]);
                    }
                }
                catch (FormatException)
                {

                }

                //Decode token để lấy JSON:
                string param = CommonHelper.Decode(token, configuration["DataBaseConfig:key_api:Static"]);

                //Model hóa JSON:
                TicketImageDetail img_detail = JsonConvert.DeserializeObject<TicketImageDetail>(param);

                // Nếu extend thuộc ảnh
                if (!stringArray.Contains(img_detail.extend.ToUpper()))
                {
                    //Trả kết quả sai file type:
                    return BadRequest(new { status = (int)ResponseType.ERROR, message = "Invalid File Type", url_path = "" });
                }

                //Nếu lấy ra được thông tin:
                if (ImageUploadHelper.IsBase64String(img_detail.data_file))
                {
                    string root = configuration["File:MainFolder"];
                    List<string> child = new List<string>();
                    child.Add(configuration["File:Images"]);

                    //Lấy thông tin thời gian hiện tại
                    DateTime time = DateTime.Now;
                    string year = time.Year.ToString();
                    string month = time.Month.ToString();
                    string day = time.Day.ToString();
                    //Thông tin file và build đường dẫn local:
                    child.Add(year);
                    child.Add(month);
                    child.Add(day);

                    string file_name = Guid.NewGuid() + "." + img_detail.extend;
                    string folder = FileService.CheckAndCreateFolder(root, child);



                    string imgPath_full = folder + "\\" + file_name;

                    byte[] bytes = System.Convert.FromBase64String(img_detail.data_file);
                    //Kiểm tra nếu file vượt quá max size:
                    if (bytes.Length > max_file_size)
                    {
                        return BadRequest(new { status = (int)ResponseType.ERROR, message = "The file image exceeds the maximum allowed size: " + max_file_size + " bytes", file_path = file_name, url_path = "" });
                    }
                    else
                    {
                        //Ghi byte[] vào file đã tạo:
                        using (var fs = new FileStream(imgPath_full, FileMode.Create, FileAccess.Write))
                        {
                            fs.Write(bytes, 0, bytes.Length);
                        }
                    }

                    //Build đường link local:
                    string urlPath_full = FileService.BuildURLFromPath(imgPath_full);

                    //Trả kết quả
                    return Ok(new { status = (int)ResponseType.SUCCESS, message = "Images Received", url_path = urlPath_full});
                }
                //Thông tin không được encode với key trong file .config hoặc thông tin convert ra null:
                else
                {
                    return BadRequest(new { status = (int)ResponseType.ERROR, message = "Invalid Format", url_path = "" });
                }
            }
            catch (Exception e)
            {
                //Log Telegram:
                //LogHelper.InsertLogTelegram("ServiceReceiverMedia - Upload Error: " + e.ToString());

                //Lỗi trên API
                return BadRequest(new { status = (int)ResponseType.ERROR, message = "On Execution", url_path = "" });
            }
        }
     
    }
}
