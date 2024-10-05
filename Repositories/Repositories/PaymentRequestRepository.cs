using DAL;
using Entities.ConfigModels;
using Entities.ViewModels;
using Microsoft.Extensions.Options;
using Repositories.IRepositories;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Utilities;

namespace Repositories.Repositories
{
    public class PaymentRequestRepository : IPaymentRequestRepository
    {
        private readonly PaymentRequestDAL paymentRequestDAL;
        public PaymentRequestRepository(IOptions<DataBaseConfig> dataBaseConfig)
        {
            paymentRequestDAL = new PaymentRequestDAL(dataBaseConfig.Value.SqlServer.ConnectionString);
        }
        public List<OrderPaymentRequest> GetListPaymentRequestByOrderId(int Orderid)
        {
            try
            {
                var dt = paymentRequestDAL.GetListPaymentRequestByOrderId(Orderid);
                if (dt != null && dt.Rows.Count > 0)
                {
                    var data = dt.ToList<OrderPaymentRequest>();
                    return data;
                }
            }
            catch (Exception ex)
            {
                LogHelper.InsertLogTelegram("GetListPaymentRequestByOrderId - PaymentRequestRepository: " + ex);
            }
            return null;
        }
    }
}
