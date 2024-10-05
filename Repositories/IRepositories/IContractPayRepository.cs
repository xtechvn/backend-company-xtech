using Entities.Models;
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
    }
}
