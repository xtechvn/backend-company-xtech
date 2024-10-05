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

namespace Repositories.Repositories
{
    public class AccountClientRepository : IAccountClientRepository
    {
        private readonly AccountClientDAL accountClientDAL;
        private readonly IOptions<DataBaseConfig> dataBaseConfig;

        public AccountClientRepository(IOptions<DataBaseConfig> _dataBaseConfig)
        {
            dataBaseConfig = _dataBaseConfig;
            accountClientDAL = new AccountClientDAL(_dataBaseConfig.Value.SqlServer.ConnectionString);
        }

        public long GetMainAccountClientByClientId(long client_id)
        {
            return accountClientDAL.GetMainAccountClientByClientId(client_id);
        }
        public AccountClient AccountClientByClientId(long client_id)
        {
            return accountClientDAL.AccountClientByClientId(client_id);
        }

        public async Task<int> InsertAccountClient(AccountClient model)
        {
            return accountClientDAL.CreateAccountClient(model);
        }
    }
}
