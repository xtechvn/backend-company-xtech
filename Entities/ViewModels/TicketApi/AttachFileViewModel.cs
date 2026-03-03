using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Entities.ViewModels.TicketApi
{
    public class AttachFileViewModel
    {
        public string Url { get; set; }
        public string Name { get; set; }
    }
    public class AttachFileRow
    {
        public long Id { get; set; }
        public long DataId { get; set; }
        public string Path { get; set; }
        public string? Ext { get; set; }
        public DateTime? CreateDate { get; set; }
        public string? FileName { get; set; } // optional nếu có cột Name
    }
}
