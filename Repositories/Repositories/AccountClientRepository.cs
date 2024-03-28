using DAL;
using Entities.ConfigModels;
using Microsoft.Extensions.Options;

namespace Repositories.IRepositories
{
    public class AccountClientRepository : IAccountClientRepository
    {
        private readonly AccountClientDAL accountClientDAL;
        private readonly IOptions<DataBaseConfig> dataBaseConfig;

        public AccountClientRepository(IOptions<DataBaseConfig> _dataBaseConfig)
        {
            accountClientDAL = new AccountClientDAL(_dataBaseConfig.Value.SqlServer.ConnectionString);
            dataBaseConfig = _dataBaseConfig;
        }

        public long GetMainAccountClientByClientId(long client_id)
        {
            return  accountClientDAL.GetMainAccountClientByClientId(client_id);
        }
        
    }
}
