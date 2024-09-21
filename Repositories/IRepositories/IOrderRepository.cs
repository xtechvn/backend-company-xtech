using Entities.ViewModels;

namespace Repositories.IRepositories
{
    public interface IOrderRepository
    {
        Task<GenericViewModel<OrderViewModel>> GetList(OrderViewSearchModel searchModel);
    }
}
