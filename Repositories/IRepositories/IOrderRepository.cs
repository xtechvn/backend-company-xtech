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
        Task<long> UpdateOrder(Order model);
        Task<List<ProductServiceName>> ProductServiceName(string OrderId);
        Task<double> UpdateOrderDetail(long OrderId, long user_id);
        Task<int> UpdateOrderStatus(long OrderId, long Status, long UpdatedBy, long UserVerify);
    }
}
