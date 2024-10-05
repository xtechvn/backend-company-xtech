using Entities.Models;
using Entities.ViewModels.CustomerManager;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Repositories.IRepositories
{
    public interface IClientRepository
    {
        Task<Client> GetClientDetailByClientId(int clientId);
        List<Client> GetAllClient();
        Task<int> SetUpClient(Client client);
        Client GetClientByEmail(string email);
    }
}
