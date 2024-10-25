using Catching.Elasticsearch.Generic;
using Elasticsearch.Net;
using Entities.ViewModels.Elasticsearch;
using Microsoft.Extensions.Configuration;
using Nest;
using Utilities;

namespace Catching.Elasticsearch
{
    public class OrderESRepository : ESRepository<OrderElasticsearchViewModel>
    {
        public string index_name = "xtech_order_store";
        private readonly IConfiguration configuration;
        public OrderESRepository(string Host, IConfiguration _configuration) : base(Host)
        {
            configuration = _configuration;
            index_name = configuration["DataBaseConfig:Elastic:index_Orders"];
        }
        public async Task<List<OrderElasticsearchViewModel>> GetOrderNoSuggesstion(string txt_search)
        {
            List<OrderElasticsearchViewModel> result = new List<OrderElasticsearchViewModel>();
            try
            {
                int top = 30;
                var nodes = new Uri[] { new Uri(_ElasticHost) };
                var connectionPool = new StaticConnectionPool(nodes);
                var connectionSettings = new ConnectionSettings(connectionPool).DisableDirectStreaming().DefaultIndex(index_name);
                var elasticClient = new ElasticClient(connectionSettings);

                var search_response = elasticClient.Search<OrderElasticsearchViewModel>(s => s
                          .Index(index_name + (_company_type.Trim() == "0" ? "" : "_" + _company_type.Trim()))
                          .Size(top)
                          .Query(q =>
                             q.QueryString(qs => qs
                               .Fields(new[] { "orderno" })
                               .Query("*" + txt_search.ToUpper() + "*")
                               .Analyzer("standard")
                           )
                          ));

                if (!search_response.IsValid)
                {
                    return result;
                }
                else
                {
                    result = search_response.Documents as List<OrderElasticsearchViewModel>;
                    return result;
                }
            }
            catch (Exception ex)
            {
                LogHelper.InsertLogTelegram("GetOrderNoSuggesstion - OrderESRepository. " + ex);
                return null;
            }

        }
   /*     public async Task<List<OrderElasticsearchViewModel>> GetOrderNoSuggesstion2(string txt_search)
        {
            List<OrderElasticsearchViewModel> result = new List<OrderElasticsearchViewModel>();
            try
            {
                int top = 30;
                var nodes = new Uri[] { new Uri(_ElasticHost) };
                var connectionPool = new StaticConnectionPool(nodes);
                var connectionSettings = new ConnectionSettings(connectionPool).DisableDirectStreaming().DefaultIndex(index_name);
                var elasticClient = new ElasticClient(connectionSettings);

                var search_response = elasticClient.Search<OrderElasticsearchViewModel>(s => s
                          .Index(index_name + (_company_type.Trim() == "0" ? "" : "_" + _company_type.Trim()))
                          .Size(top)
                          .Query(q =>
                           q.Bool(
                               qb => qb.Must(
                                   sh => sh.QueryString(qs => qs
                                   .Fields(new[] { "orderno" })
                                   .Query("*" + txt_search.ToUpper() + "*")
                                   .Analyzer("standard")

                            )
                           )
                          )));

                if (!search_response.IsValid)
                {
                    return result;
                }
                else
                {
                    result = search_response.Documents as List<OrderElasticsearchViewModel>;
                    return result;
                }
            }
            catch (Exception ex)
            {
                LogHelper.InsertLogTelegram("GetOrderNoSuggesstion - OrderESRepository. " + ex);
                return null;
            }

        }*/
    }
}
