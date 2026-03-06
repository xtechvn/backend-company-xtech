using DAL;
using Entities.ConfigModels;
using Entities.Models;
using Microsoft.Extensions.Options;
using Repositories.IRepositories;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Repositories.Repositories
{
    public class TaskCommentRepository : ITaskCommentRepository
    {
        private readonly TaskCommentDAL _taskCommentDAL;

        public TaskCommentRepository(IOptions<DataBaseConfig> dataBaseConfig)
        {
            _taskCommentDAL = new TaskCommentDAL(dataBaseConfig.Value.SqlServer.ConnectionString);
        }

        public async Task<List<TaskComment>> GetCommentsByTaskId(long taskId)
        {
            return await _taskCommentDAL.GetCommentsByTaskId(taskId);
        }

        public async Task<long> CreateComment(TaskComment comment)
        {
            return await _taskCommentDAL.CreateComment(comment);
        }

        public async Task<bool> DeleteComment(long id)
        {
            return await _taskCommentDAL.DeleteComment(id);
        }
    }
}
