using Entities.Models;
using Entities.OrderDetail;
using Entities.ViewModels;

namespace Repositories.IRepositories
{
    public interface IOrderRepository
    {
        Task<GenericViewModel<OrderViewModel>> GetList(OrderViewSearchModel searchModel);
        Task<OrderDetailViewModel> GetOrderDetailByOrderId(long OrderId);
        Task<double> GetTotalAmountByPaymentStatus(int? paymentStatus);
        Task<int> CreateOrder(Order order);
        Task<int> UpdateAmountOrder(int OrderId);
        Task<string> GetOrderNo();
    }
}
