using DAL;
using Entities.ConfigModels;
using Entities.Models;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;
using Repositories.IRepositories;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Repositories.Repositories
{
    public class SprintRepository : ISprintRepository
    {
        private readonly SprintDAL _sprintDAL;

        public SprintRepository(IConfiguration configuration,  IOptions<DataBaseConfig> dataBaseConfig)
        {
            _sprintDAL = new SprintDAL(dataBaseConfig.Value.SqlServer.ConnectionString);
        }

        public async Task<List<Sprint>> GetAllSprints(long? projectId = null)
        {
            return await _sprintDAL.GetAllSprints(projectId);
        }

        public async Task<long> Upsert(Sprint model)
        {
            return await _sprintDAL.Upsert(model);
        }

        public async Task<bool> UpdateStatus(long sprintId, int status)
        {
            return await _sprintDAL.UpdateStatus(sprintId, status);
        }

        public async Task<Sprint> GetById(long id)
        {
            return await _sprintDAL.FindAsync(id);
        }

        public async Task<bool> Delete(long id)
        {
            try
            {
                await _sprintDAL.DeleteAsync(id);
                return true;
            }
            catch
            {
                return false;
            }
        }
    }
}
