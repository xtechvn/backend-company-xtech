using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Repositories.IRepositories
{
    public interface IIdentifierServiceRepository
    {
        Task<string> buildClientNo(int client_type);
    }
}
