using Entities.ViewModels.Article;
using Newtonsoft.Json;
using System.Drawing;
using System.Text;

namespace Utilities
{
    public class UpLoadHelper
    {
        static string apiPrefix = "/static/images/upload";
        static string apiUploadVideo = "/static/Video/upload-video";
        /// <summary>
        /// UploadImageBase64
        /// </summary>
        /// <param name="ImageBase64">src of image</param>
        /// <returns></returns>
        public static async Task<string> UploadImageBase64(ImageBase64 modelImage,string domain,string key)
        {
            string ImagePath = string.Empty;
            string tokenData = string.Empty;
            try
            {
               
                var j_param = new Dictionary<string, string> {
                    { "data_file", modelImage.ImageData },
                    { "extend", modelImage.ImageExtension }};

                using (HttpClient httpClient = new HttpClient())
                {
                    tokenData = CommonHelper.Encode(JsonConvert.SerializeObject(j_param), key);
                    //var contentObj = new { token = tokenData };
                    //var content = new StringContent(JsonConvert.SerializeObject(contentObj), Encoding.UTF8, "application/json");
                    //var result = await httpClient.PostAsync(domain+apiPrefix, content);
                    var client = new HttpClient();
                    var request = new HttpRequestMessage(HttpMethod.Post, domain+apiPrefix);
                    var content = new StringContent("{\"token\":\""+tokenData+"\"}", null, "application/json");
                    //request.Content = content;
                    //var response = await client.SendAsync(request);
                    //response.EnsureSuccessStatusCode();
                    request.Content = content;
                    var response = await client.SendAsync(request);
                    response.EnsureSuccessStatusCode();
                 
                    dynamic resultContent = Newtonsoft.Json.Linq.JObject.Parse(await response.Content.ReadAsStringAsync());
                    if (resultContent.status == 0)
                    {
                        return resultContent.url_path;
                    }
                    else
                    {
                        LogHelper.InsertLogTelegram("UploadImageBase64. Result: " + resultContent.status + ". Message: " + resultContent.msg);
                    }
                }
            }
            catch (Exception ex)
            {
                LogHelper.InsertLogTelegram("UploadImageBase64 - " + ex.Message.ToString() + " Token:" + tokenData);
            }
            return ImagePath;
        }

        public static async Task<string> UploadBase64Src(string ImageSrc, string domain, string key)
        {
            try
            {
                var objimage = StringHelpers.GetImageSrcBase64Object(ImageSrc);
                if (objimage != null)
                {
                    objimage.ImageData = ResizeBase64Image(objimage.ImageData, out string FileType);
                    if (!string.IsNullOrEmpty(FileType)) objimage.ImageExtension = FileType;

                    return await UploadImageBase64(objimage, domain,key);
                }
                else
                {
                    if (ImageSrc.StartsWith(domain))
                        return ImageSrc.Replace(domain, string.Empty);
                    else
                        return ImageSrc;
                }
            }
            catch (Exception ex)
            {
                LogHelper.InsertLogTelegram("UploadImageBase64 - " + ex.Message.ToString());
            }
            return string.Empty;
        }

        /// <summary>
        /// Resize image with maximum 1000px width
        /// </summary>
        /// <param name="ImageBase64"></param>
        /// <returns></returns>
        public static string ResizeBase64Image(string ImageBase64, out string FileType)
        {
            FileType = null;
            try
            {
                var IsValid = StringHelpers.TryGetFromBase64String(ImageBase64, out byte[] ImageByte);
                if (IsValid)
                {
                    using (var memoryStream = new MemoryStream(ImageByte))
                    {
                        var RootImage = Image.FromStream(memoryStream);
                        if (RootImage.Width > 1000)
                        {
                            int width = 1000;
                            int height = (int)(width / ((double)RootImage.Width / RootImage.Height));
                            var ResizeImage = (System.Drawing.Image)(new Bitmap(RootImage, new Size(width, height)));
                            using (var stream = new MemoryStream())
                            {
                                ResizeImage.Save(stream, System.Drawing.Imaging.ImageFormat.Jpeg);
                                ImageByte = stream.ToArray();
                            }
                            FileType = "jpg";
                            return Convert.ToBase64String(ImageByte);
                        }
                        else
                        {
                            return ImageBase64;
                        }
                    }
                }
                else
                {
                    return null;
                }
            }
            catch
            {
                return null;
            }
        }
       
    }
}
