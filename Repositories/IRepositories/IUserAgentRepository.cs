using Entities.Models;
using Entities.ViewModels.UserAgent;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Repositories.IRepositories
{
    public interface IUserAgentRepository
    {
        List<UserAgentViewModel> UserAgentByClient(int ClientId, long id);
        int UpdataUserAgent(int Id, int UserId, int create_id, long ClientId);
    }
}
