using Entities.ViewModels;
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
        List<PaymentRequestViewModel> GetByServiceId(long serviceId, int type, int requestType = 0);
    }
}
