using DAL;
using Entities.ConfigModels;
using Entities.Models;
using Microsoft.Extensions.Options;
using Repositories.IRepositories;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Utilities;

namespace Repositories.Repositories
{
    public class ClientRepository : IClientRepository
    {
        private readonly ClientDAL _ClientDAL;

        public ClientRepository(IOptions<DataBaseConfig> dataBaseConfig)
        {
            _ClientDAL = new ClientDAL(dataBaseConfig.Value.SqlServer.ConnectionString);
        }
        
        public async Task<Client> GetClientDetailByClientId(int clientId)
        {
            try
            {
                var data = await _ClientDAL.GetClientByID(clientId);
                return data;
            }
            catch (Exception ex)
            {
                LogHelper.InsertLogTelegram("GetClientDetailByClientId - ClientRepository: " + ex);
                return null;
            }
        }

       
        public List<Client> GetAllClient()
        {
            try
            {
                return _ClientDAL.GetAll();
            }
            catch (Exception ex)
            {
                LogHelper.InsertLogTelegram("GetAllClient - ClientRepository: " + ex);
                return null;
            }
        }
        public Client GetClientByEmail(string email)
        {
            try
            {
                return _ClientDAL.GetClientByEmail(email);
            }
            catch (Exception ex)
            {
                LogHelper.InsertLogTelegram("GetClientByEmail - ClientRepository: " + ex);
                return null;
            }
        }
    }
}
