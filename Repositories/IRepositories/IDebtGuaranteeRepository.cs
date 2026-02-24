using Entities.ViewModels.DebtGuarantee;
using Entities.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Entities.Models;

namespace Repositories.IRepositories
{
    public interface IDebtGuaranteeRepository
    {
        
        Task<long> UpdateDebtGuarantee(int id, int Status, int CreatedBy);
       
       
        Task<DebtGuaranteeViewModel> DetailDebtGuaranteebyOrderid(int Orderid);
       
    }
}
