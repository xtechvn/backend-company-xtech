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
        public DataTable GetListPaymentRequestByServiceId(long serviceId, int type, string proc, int requestType = 0)
        {
            try
            {
                SqlParameter[] objParam = new SqlParameter[3];
                objParam[0] = new SqlParameter("@ServiceId", serviceId);
                objParam[1] = new SqlParameter("@Type", type);
                if (requestType != 0)
                {
                    objParam[2] = new SqlParameter("@RequestType", requestType);
                }
                else
                {
                    objParam[2] = new SqlParameter("@RequestType", (int)PAYMENT_VOUCHER_TYPE.THANH_TOAN_DICH_VU + "," + (int)PAYMENT_VOUCHER_TYPE.THANH_TOAN_KHAC);
                }
                return _DbWorker.GetDataTable(proc, objParam);
            }
            catch (Exception ex)
            {
                LogHelper.InsertLogTelegram("GetServiceListBySupplierId - PaymentRequestDAL: " + ex);
            }
            return null;
        }
    }
}
