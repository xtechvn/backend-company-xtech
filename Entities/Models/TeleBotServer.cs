using System;
using System.Collections.Generic;

namespace Entities.Models
{
    public partial class TeleBotServer
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public DateTime? CreatedAt { get; set; }
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public decimal Price { get; set; }
        public int Status { get; set; }   // 👈 thêm dòng này
    }
}
