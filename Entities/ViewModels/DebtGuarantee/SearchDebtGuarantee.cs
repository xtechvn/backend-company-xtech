using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Entities.ViewModels.DebtGuarantee
{
    public class SearchDebtGuarantee
    {
        public string Code { get; set; }
        public string Status { get; set; }
        public string ClientId { get; set; }
        public string DepartmentId { get; set; }
        public string OrderId { get; set; }
        public string CreateTime { get; set; }
        public string ToDateTime { get; set; }
        public string SalerPermission { get; set; }
        public int PageIndex { get; set; } = -1;
        public int PageSize { get; set; } = -1;
    }
}
