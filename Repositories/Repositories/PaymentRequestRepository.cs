using DAL;
using Entities.ConfigModels;
using Entities.ViewModels;
using Entities.ViewModels.Funding;
using Microsoft.Extensions.Options;
using Repositories.IRepositories;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Utilities;
using Utilities.Contants;

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
        public List<PaymentRequestViewModel> GetByServiceId(long serviceId, int type, int requestType = 0)
        {
            try
            {
                var listServiceOutput = paymentRequestDAL.GetListPaymentRequestByServiceId(serviceId, type,
                    StoreProcedureConstant.sp_GetListPaymentRequestByServiceId, requestType).ToList<PaymentRequestViewModel>();
                //foreach (var item in listServiceOutput)
                //{
                //    item.ListServiceCodeAndType = new List<CountStatus>();
                //    if (!string.IsNullOrEmpty(item.PaymentVoucherCode))
                //    {
                //        var listPaymentVoucher = paymentVoucherDAL.GetByPaymentCodes(item.PaymentVoucherCode.Split(",").ToList());
                //        foreach (var paymentVoucher in listPaymentVoucher)
                //        {
                //            CountStatus countStatus = new CountStatus();
                //            countStatus.DataNo = paymentVoucher.PaymentCode;
                //            countStatus.DataId = paymentVoucher.Id;
                //            item.ListServiceCodeAndType.Add(countStatus);
                //        }
                //    }
                //}
                return listServiceOutput;
            }
            catch (Exception ex)
            {
                LogHelper.InsertLogTelegram("GetByServiceId - PaymentRequestRepository: " + ex);
            }
            return new List<PaymentRequestViewModel>();
        }
    }
}
