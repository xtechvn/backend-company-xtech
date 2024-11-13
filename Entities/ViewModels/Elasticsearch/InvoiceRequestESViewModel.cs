using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Entities.ViewModels.Elasticsearch
{
    public class InvoiceRequestESViewModel
    {
        public long id { get; set; } // ID ElasticSearch
        public string invoicerequestno { get; set; }

    }
}
