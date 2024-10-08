using Entities.Models;
using Entities.ViewModels;
using Entities.ViewModels.CustomerManager;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Repositories.IRepositories
{
    public interface ICustomerManagerRepository
    {
        Task<CustomerManagerViewModel> GetDetailClient(long ClientId);
        int SetUpClient(CustomerManagerView model);
        Task<int> UpdateApproachStatus(Client client);
        Task<GenericViewModel<CustomerManagerViewModel>> GetPagingList(CustomerManagerViewSearchModel searchModel, int currentPage, int pageSize);
        int ResetStatusAc(long clientId, long Status, int type);
        Task<List<AllCode>> getApproachType();
        Task<AllCode> getApproachTypeCodeValue(int Id);
        Task<DataClientReturnViewModel> CheckExistAccount(AccountModel model);
        Task<AmountRemainView> GetAmountRemainOfContractByClientId(long ClientId);
        Task<string> ExportDeposit(CustomerManagerViewSearchModel searchModel, string FilePath, field field);
    }
}
