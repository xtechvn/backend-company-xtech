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
    public interface IContractPayRepository
    {
        Task<List<ContractPayDetaiByOrderIdlViewModel>> GetContractPayByOrderId(long OrderId);
        double GetTotalAmountContractPayByServiceId(string ServiceId, long ServiceType, long ContractPayType);
        List<ContractPayViewModel> GetListContractPay(ContractPaySearchModel searchModel, out long total, int currentPage = 1, int pageSize = 20);
        ContractPayViewModel GetByContractPayId(int contractPayId);
        ContractPayViewModel GetByPayId(int contractPayId);
        int CreateContractPay(ContractPayViewModel model);
        long CountPaymentRequest();

        int UpdateContractPay(ContractPayViewModel model);
    }
}
