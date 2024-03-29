using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Entities.ViewModels.Static
{
    //Class thông tin ảnh gửi lên.
    [System.Serializable]
    public class ImageDetail
    {
        public string data_file;
        public string extend;
    }
    //Class thông tin ảnh gửi lên.
    [System.Serializable]
    public class PaymentImageDetail
    {
        public string data_file;
        public string extend;

    }
    [System.Serializable]
    public class TicketImageDetail : ImageDetail
    {
        public string file_name;
    }
}
