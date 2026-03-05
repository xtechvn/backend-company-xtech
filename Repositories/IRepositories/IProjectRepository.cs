using Entities.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Repositories.IRepositories
{
    public interface IProjectRepository
    {
        Task<List<Projects>> GetAllProjects();
        Task<long> Upsert(Projects model);
        Task<Projects> GetById(long id);
    }
}
