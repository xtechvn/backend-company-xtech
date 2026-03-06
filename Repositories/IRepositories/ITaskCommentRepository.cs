using Entities.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Repositories.IRepositories
{
    public interface ITaskCommentRepository
    {
        Task<List<TaskComment>> GetCommentsByTaskId(long taskId);
        Task<long> CreateComment(TaskComment comment);
        Task<bool> DeleteComment(long id);
    }
}
