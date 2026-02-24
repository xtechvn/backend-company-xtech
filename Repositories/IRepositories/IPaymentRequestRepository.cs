using Entities.Models;
using Entities.ViewModels;
using Entities.ViewModels.Funding;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Repositories.IRepositories
{
    public interface IPaymentRequestRepository
    {
        List<OrderPaymentRequest> GetListPaymentRequestByOrderId(int Orderid);
        int UpdatePaymentRequest(PaymentRequestViewModel model);
        List<PaymentRequestViewModel> GetServiceListBySupplierId(long supplierId, int requestId = 0, int serviceId = 0);
        List<PaymentRequestViewModel> GetByServiceId(long serviceId, int type, int requestType = 0);
        List<PaymentRequestViewModel> GetBySupplierId(long supplierId, int paymentVoucherId = 0, string requestType = "1,2");
        List<PaymentRequestViewModel> GetByClientId(long clientId, long Type, int paymentVoucherId = 0);
        List<PaymentRequestViewModel> GetPaymentRequests(PaymentRequestSearchModel searchModel, out long total, int currentPage = 1, int pageSize = 20);
        List<CountStatus> GetCountStatus(PaymentRequestSearchModel searchModel);
        List<PaymentRequestViewModel> GetRequestByClientId(long clientId, long orderid = 0);
        int CreatePaymentRequest(PaymentRequestViewModel model);
        PaymentRequestViewModel GetById(int paymentRequestId);
        int ApprovePaymentRequest(string paymentRequestNo, int userId, int status);
        PaymentRequest GetByRequestNo(string paymentRequestNo);
    }
}
