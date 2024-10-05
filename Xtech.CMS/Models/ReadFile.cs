using Newtonsoft.Json;
using Utilities;

namespace Xtech.CMS.Models
{
    public class ReadFile
    {
        private static AppSettings _appconfig { get; set; }
        public static AppSettings LoadConfig()
        {
            if (_appconfig != null)
            {
                return _appconfig;
            }

            using (StreamReader r = new StreamReader("config.json"))
            {
                string json = r.ReadToEnd();
                _appconfig = JsonConvert.DeserializeObject<AppSettings>(json);
                return _appconfig;
            }
        }
    }
}
