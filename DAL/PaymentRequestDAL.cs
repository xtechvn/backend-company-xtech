using DAL.StoreProcedure;
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
    public class PaymentRequestDAL
    {
        private static DbWorker _DbWorker;
        public PaymentRequestDAL(string connection)
        {
            _DbWorker = new DbWorker(connection);
        }
        public DataTable GetListPaymentRequestByOrderId(int orderId)
        {
            try
            {
                SqlParameter[] objParam = new SqlParameter[1];
                objParam[0] = new SqlParameter("@OrderId", orderId);
                return _DbWorker.GetDataTable(StoreProcedureConstant.SP_GetListPaymentRequestByOrderId, objParam);
            }
            catch (Exception ex)
            {
                LogHelper.InsertLogTelegram("GetListPaymentRequestByOrderId - PaymentRequestDAL: " + ex);
            }
            return null;
        }
    }
}
