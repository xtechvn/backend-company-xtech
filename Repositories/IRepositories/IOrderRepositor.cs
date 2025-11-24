using Entities.Models;
using Entities.ViewModels;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Repositories.IRepositories
{
    public interface IOrderRepositor
    {
        Order GetByOrderId(long OrderId);
        Task<List<ProductServiceName>> ProductServiceName(string OrderId);
        Task<int> UpdateOrderStatus(long OrderId, long Status, long UpdatedBy, long UserVerify);
        Task<string> FindByVoucherid(int voucherId);
        Task<GenericViewModel<OrderViewModel>> GetByClientId(long ClientId, int currentPage, int pageSize);
        Task<List<OrderDetailViewModel>> GetDetailOrderByOrderId(int OrderId);
    }
}
