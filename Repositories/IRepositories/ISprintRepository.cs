using Entities.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Repositories.IRepositories
{
    public interface ISprintRepository
    {
        Task<List<Sprint>> GetAllSprints(long? projectId = null);
        Task<long> Upsert(Sprint model);
        Task<bool> UpdateStatus(long sprintId, int status);
        Task<Sprint> GetById(long id);
        Task<bool> Delete(long id);
    }
}
