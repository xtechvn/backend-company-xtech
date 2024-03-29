using Entities.Models;

namespace Repositories.IRepositories
{
    public interface IDepartmentRepository
    {
        Task<long> Create(Department model);
        Task<long> Update(Department model);
        Task<Department> GetById(int id);
        Task<IEnumerable<Department>> GetAll(string name);
        Task<long> Delete(int id);
   
    }
}
