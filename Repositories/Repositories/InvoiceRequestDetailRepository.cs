using DAL;
using DAL.StoreProcedure;
using Entities.ConfigModels;
using Entities.Models;
using Entities.ViewModels.InvoiceRequest;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;
using Repositories.IRepositories;
using StackExchange.Redis;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Utilities;
using Utilities.Contants;

namespace Repositories.Repositories
{
    public class InvoiceRequestDetailRepository : IInvoiceRequestDetailRepository
    {
        private readonly InvoiceRequestDetailDAL _invoiceRequestDetailDAL;
        private readonly IConfiguration _configuration;
        private readonly IOptions<DataBaseConfig> _dataBaseConfig;
        public InvoiceRequestDetailRepository(IConfiguration configuration, IOptions<DataBaseConfig> dataBaseConfig)
        {
            _dataBaseConfig = dataBaseConfig;
            _configuration = configuration;
            _invoiceRequestDetailDAL = new InvoiceRequestDetailDAL(_dataBaseConfig.Value.SqlServer.ConnectionString);
        }

        public async Task<int> DeleteInvoiceRequestDetailById(int Id)
        {
            try
            {
                return await _invoiceRequestDetailDAL.DeleteInvoiceRequestDetailById(Id);
            }
            catch (Exception ex)
            {
                LogHelper.InsertLogTelegram("GetListInvoiceRequestDetailByInvoiceRequestId - InvoiceRequestDetailRepository: " + ex.ToString());
                return -1;
            }
        }

        public async Task<List<InvoiceRequestDetail>> GetListInvoiceRequestDetailByInvoiceRequestId(int OrderId)
        {
            try
            {
                return await _invoiceRequestDetailDAL.GetListInvoiceRequestDetailByInvoiceRequestId(OrderId);
            }
            catch (Exception ex)
            {
                LogHelper.InsertLogTelegram("GetListInvoiceRequestDetailByInvoiceRequestId - InvoiceRequestDetailRepository: " + ex.ToString());
                return null;
            }
        }

        public async Task<int> SetUpInvoiceRequestDetail(InvoiceRequestDetail request)
        {
            try
            {
                return await _invoiceRequestDetailDAL.SetUpInvoiceRequestDetail(request);
            }
            catch (Exception ex)
            {
                LogHelper.InsertLogTelegram("SetUpInvoiceRequestDetail - InvoiceRequestDetailRepository: " + ex.ToString());
                return -1;
            }
        }
    }
}
