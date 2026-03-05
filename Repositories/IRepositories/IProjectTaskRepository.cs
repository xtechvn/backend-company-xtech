using Entities.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Repositories.IRepositories
{
    public interface IProjectTaskRepository
    {
        Task<List<ProjectTask>> GetTasksBySprint(long? sprintId, long? projectId = null);
        Task<long> Upsert(ProjectTask model);
        Task<bool> UpdateSprint(long taskId, long? sprintId);
        Task<bool> UpdateSprints(List<long> taskIds, long? sprintId);
        Task<bool> UpdateStatus(long taskId, int status);
        Task<ProjectTask> GetById(long id);
        Task<bool> Delete(long id);
    }
}
