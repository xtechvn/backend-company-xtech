using Catching.Elasticsearch.Generic;
using Elasticsearch.Net;
using Entities.Models;
using Entities.ViewModels.Elasticsearch;
using Microsoft.Extensions.Configuration;
using Nest;
using SixLabors.ImageSharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Catching.Elasticsearch
{
    public class InvoiceRequestESRepository : ESRepository<InvoiceRequestESViewModel>
    {
        public string index_name = "xtech_invoice_request";
        private readonly IConfiguration configuration;
        public InvoiceRequestESRepository(string Host, IConfiguration _configuration) : base(Host)
        {
            configuration = _configuration;
            index_name = configuration["DataBaseConfig:Elastic:index_Invoice_request"];
        }

        public async Task<List<InvoiceRequestESViewModel>> GetInvoiceRequestByNO(string txt_search)
        {
            List<InvoiceRequestESViewModel> result = new List<InvoiceRequestESViewModel>();
            try
            {
                int top = 4000;
                var nodes = new Uri[] { new Uri(_ElasticHost) };
                var connectionPool = new StaticConnectionPool(nodes);
                var connectionSettings = new ConnectionSettings(connectionPool).DisableDirectStreaming().DefaultIndex(index_name);
                var elasticClient = new ElasticClient(connectionSettings);

                var search_response = elasticClient.Search<InvoiceRequestESViewModel>(s => s
                          .Index(index_name + (_company_type.Trim() == "0" ? "" : "_" + _company_type.Trim()))
                          .Size(top)
                          .Query(q =>
                             q.QueryString(qs => qs
                               .Fields(new[] { "invoicerequestno" })
                               .Query("*" + txt_search.ToUpper() + "*")
                               .Analyzer("standard")
                           )
                          ));

                if (!search_response.IsValid)
                {
                    return null;
                }
                else
                {
                    result = search_response.Documents as List<InvoiceRequestESViewModel>;
                    return result.Count > 0 ? result : null;
                }
            }
            catch (Exception ex)
            {
                return null;
            }

        }
    }
}
