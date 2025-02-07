using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Entities.ViewModels
{
    public class TenantViewModel
    {
        public int TenantId { get; set; }
        public string UserName { get; set; }
        public string SurrogateName { get; set; }
        public string SurrogatePhone { get; set; }
        public string SurrogateEmail { get; set; }
        public string Address { get; set; }
        public string Password { get; set; }
        public DateTime? CreatedDate { get; set; }
        public int Status { get; set; }
    }
    public class TenantSearchModel
    {

        public string UserName { get; set; }
        public string SurrogateName { get; set; }
        public int status { get; set; } = -1;
        public int PageIndex { get; set; }
        public int PageSize { get; set; }
    }
}
