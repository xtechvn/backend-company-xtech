using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Entities.ViewModels.Funding
{
    public class CountStatus
    {
        public int Status { get; set; }
        public int Count { get; set; }
        public int Total { get; set; }
        public long DataId { get; set; }
        public string DataIdFly { get; set; }
        public string DataNo { get; set; }
        public string StatusName { get; set; }
        public int ServiceType { get; set; }
        public string PaymentRequestStatus { get; set; }
    }
}
