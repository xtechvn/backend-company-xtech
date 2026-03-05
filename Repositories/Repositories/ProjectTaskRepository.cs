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
    public class ProjectTaskRepository : IProjectTaskRepository
    {
        private readonly ProjectTaskDAL _projectTaskDAL;

        public ProjectTaskRepository(IConfiguration configuration, IOptions<DataBaseConfig> dataBaseConfig)
        {
            _projectTaskDAL = new ProjectTaskDAL(dataBaseConfig.Value.SqlServer.ConnectionString); 
        }

        public async Task<List<ProjectTask>> GetTasksBySprint(long? sprintId, long? projectId = null)
        {
            return await _projectTaskDAL.GetTasksBySprint(sprintId, projectId);
        }

        public async Task<long> Upsert(ProjectTask model)
        {
            return await _projectTaskDAL.Upsert(model);
        }

        public async Task<bool> UpdateSprint(long taskId, long? sprintId)
        {
            return await _projectTaskDAL.UpdateSprint(taskId, sprintId);
        }

        public async Task<bool> UpdateSprints(List<long> taskIds, long? sprintId)
        {
            return await _projectTaskDAL.UpdateSprints(taskIds, sprintId);
        }

        public async Task<bool> UpdateStatus(long taskId, int status)
        {
            return await _projectTaskDAL.UpdateStatus(taskId, status);
        }

        public async Task<ProjectTask> GetById(long id)
        {
            return await _projectTaskDAL.FindAsync(id);
        }

        public async Task<bool> Delete(long id)
        {
            try
            {
                await _projectTaskDAL.DeleteAsync(id);
                return true;
            }
            catch
            {
                return false;
            }
        }
    }
}
