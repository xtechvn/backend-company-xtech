using DAL.Generic;
using DAL.StoreProcedure;
using Entities.Models;
using Microsoft.Data.SqlClient;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Utilities;
using Utilities.Contants;

namespace DAL
{
    public class ContractPayDAL
    {
        private static DbWorker _DbWorker;
        private static string _connection;
        public ContractPayDAL(string connection)
        {
            _DbWorker = new DbWorker(connection);
            _connection = connection;
        }

        public async Task<long> CountInvoiceRequest()
        {
            try
            {
                using (var _DbContext = new EntityDataContext(_connection))
                {
                    return _DbContext.InvoiceRequests.Count();
                    
                }
            }
            catch (Exception ex)
            {
                LogHelper.InsertLogTelegram("CountInvoiceRequest - ContractPayDAL. " + ex);
                return 0;
            }
        }

        public async Task<DataTable> GetContractPayByOrderId(long OrderId)
        {
            try
            {

                SqlParameter[] objParam_contractPay = new SqlParameter[1];
                objParam_contractPay[0] = new SqlParameter("@OrderId", OrderId);

                return _DbWorker.GetDataTable(StoreProcedureConstant.SP_GetContractPayByOrderId, objParam_contractPay);
            }
            catch (Exception ex)
            {
                LogHelper.InsertLogTelegram("GetContractPayByOrderId - ContractPayDAL. " + ex);
                return null;
            }
        }

    }
}
