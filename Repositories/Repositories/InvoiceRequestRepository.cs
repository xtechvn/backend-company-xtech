using DAL;
using Entities.ConfigModels;
using Entities.Models;
using Entities.ViewModels;
using Entities.ViewModels.InvoiceRequest;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;
using Repositories.IRepositories;
using StackExchange.Redis;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Utilities;

namespace Repositories.Repositories
{
    public class InvoiceRequestRepository : IInvoiceRequestRepository
    {
        private readonly InvoiceRequestDAL _invoiceRequestDAL;
        private readonly ContractPayDAL _contractPayDAL;
        private readonly IConfiguration _configuration;
        private readonly IOptions<DataBaseConfig> _dataBaseConfig;
        public InvoiceRequestRepository(IConfiguration configuration, IOptions<DataBaseConfig> dataBaseConfig)
        {
            _dataBaseConfig = dataBaseConfig;
            _configuration = configuration;
            _invoiceRequestDAL = new InvoiceRequestDAL(_dataBaseConfig.Value.SqlServer.ConnectionString);
            _contractPayDAL = new ContractPayDAL(_dataBaseConfig.Value.SqlServer.ConnectionString);

        }

        public async Task<List<InvoiceRequestViewModel>> GetInvoiceRequestById(int OrderId)
        {
            try 
            {
                return await _invoiceRequestDAL.GetInvoiceRequestById(OrderId);
            }
            catch (Exception ex) 
            {
                LogHelper.InsertLogTelegram("GetInvoiceRequestById - InvoiceRequestRepository: " + ex.ToString());
                return null;
            }
        }

        public async Task<List<InvoiceRequestViewModel>> GetListInvoiceRequestByOrderId(int OrderId)
        {
            try
            {
                return await _invoiceRequestDAL.GetListInvoiceRequestByOrderId(OrderId);
            }
            catch (Exception ex)
            {
                LogHelper.InsertLogTelegram("GetListInvoiceRequestByOrderId - InvoiceRequestRepository: " + ex.ToString());
                return null;
            }
        }
        public async Task<string> BuildExportBillNo()
        {
            string bill_no = string.Empty;
            try
            {
                var months = new Dictionary<int, string> { { 1, "A" }, { 2, "B" }, { 3, "C" }, { 4, "D" }, { 5, "E" }, { 6, "F" }, { 7, "G" }, { 8, "H" }, { 9, "K" }, { 10, "L" }, { 11, "M" }, { 12, "N" } };

                var current_date = DateTime.Now;
                bill_no = "YCXHD";

                // 2 số cuối của năm
                bill_no += current_date.Year.ToString().Substring(current_date.Year.ToString().Length - 2, 2);

                //Tháng hiện tại
                bill_no += months[current_date.Month];

                //2. Số thứ tự đã dùng.
                long bill_count = await _contractPayDAL.CountInvoiceRequest();

                //format numb
                string s_bill_new = string.Format(String.Format("{0,5:00000}", bill_count + 1));

                bill_no += s_bill_new;

                return bill_no;
            }
            catch (Exception ex)
            {
                LogHelper.InsertLogTelegram("BuildExportBillNo - IdentifierServiceRepository" + ex.ToString());
                //Trả mã random
                var rd = new Random();
                var contract_pay_default = rd.Next(DateTime.Now.Day, DateTime.Now.Year) + rd.Next(1, 999);
                bill_no = "PYCXHD-" + contract_pay_default;
                return bill_no;
            }
        }


        public async Task<int> SetUpInvoiceRequest(InvoiceRequest request)
        {
            try
            {
                if (request.Id <= 0) 
                {
                    request.InvoiceRequestNo = await BuildExportBillNo();
                }
                return await _invoiceRequestDAL.SetUpInvoiceRequest(request);
            }
            catch (Exception ex)
            {
                LogHelper.InsertLogTelegram("SetUpInvoiceRequest - InvoiceRequestRepository: " + ex.ToString());
                return -1;
            }
        }

        public async Task<GenericViewModel<InvoiceRequestViewModel>> GetListInvoiceRequest(InvoiceSearchViewModel request)
        {
            try
            {
               return await _invoiceRequestDAL.GetListInvoiceRequest(request);
            }
            catch (Exception ex)
            {
                LogHelper.InsertLogTelegram("GetListInvoiceRequest - InvoiceRequestRepository: " + ex.ToString());
                return null;
            }
        }
    }
}
