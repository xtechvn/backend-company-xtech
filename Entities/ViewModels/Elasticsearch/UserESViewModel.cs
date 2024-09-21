using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Entities.ViewModels.Elasticsearch
{
    public class UserESViewModel
    {
        public long _id { get; set; } // ID ElasticSearch
        public long id { get; set; } // ID customer
        public string username { get; set; }
        public string fullname { get; set; }
        public string email { get; set; }
        public string phone { get; set; }
    }
}
