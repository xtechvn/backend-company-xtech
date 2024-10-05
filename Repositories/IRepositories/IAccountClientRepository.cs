using Entities.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Repositories.IRepositories
{
    public interface IAccountClientRepository
    {
        long GetMainAccountClientByClientId(long client_id);
        AccountClient AccountClientByClientId(long client_id);
        Task<int> InsertAccountClient(AccountClient model);
    }
}
