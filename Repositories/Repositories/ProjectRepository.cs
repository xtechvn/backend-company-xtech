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
    public class ProjectRepository : IProjectRepository
    {
        private readonly ProjectDAL _projectDAL;

        public ProjectRepository(IConfiguration configuration, IOptions<DataBaseConfig> dataBaseConfig)
        {
            _projectDAL = new ProjectDAL(dataBaseConfig.Value.SqlServer.ConnectionString);
        }

        public async Task<List<Projects>> GetAllProjects()
        {
            return await _projectDAL.GetAllProjects();
        }

        public async Task<long> Upsert(Projects model)
        {
            return await _projectDAL.Upsert(model);
        }

        public async Task<Projects> GetById(long id)
        {
            return await _projectDAL.FindAsync(id);
        }
    }
}
