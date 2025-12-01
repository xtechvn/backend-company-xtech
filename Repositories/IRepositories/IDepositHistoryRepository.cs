using Entities.Models;
using Entities.ViewModels;
using Entities.ViewModels.Funding;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Repositories.IRepositories
{
    public interface IDepositHistoryRepository
    {
       
        List<DepositFunding> GetByClientId(long clientId, int payId = 0);
    }
}
