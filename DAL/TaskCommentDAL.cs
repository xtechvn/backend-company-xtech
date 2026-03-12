using DAL.Generic;
using Entities.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Utilities;

namespace DAL
{
    public class TaskCommentDAL : GenericService<TaskComment>
    {
        public TaskCommentDAL(string connection) : base(connection)
        {
        }

        public async Task<List<TaskComment>> GetCommentsByTaskId(long taskId)
        {
            try
            {
                using (var _DbContext = new EntityDataContext(_connection))
                {
                    return await _DbContext.TaskComments
                        .Where(c => c.TaskId == taskId)
                        .OrderBy(c => c.CreatedDate)
                        .ToListAsync();
                }
            }
            catch (Exception ex)
            {
                LogHelper.InsertLogTelegram("GetCommentsByTaskId - TaskCommentDAL: " + ex);
                return new List<TaskComment>();
            }
        }

        public async Task<long> CreateComment(TaskComment comment)
        {
            try
            {
                using (var _DbContext = new EntityDataContext(_connection))
                {
                    comment.CreatedDate = DateTime.Now;
                    _DbContext.TaskComments.Add(comment);
                    await _DbContext.SaveChangesAsync();
                    return comment.Id;
                }
            }
            catch (Exception ex)
            {
                LogHelper.InsertLogTelegram("CreateComment - TaskCommentDAL: " + ex);
                return 0;
            }
        }

        public async Task<bool> DeleteComment(long id)
        {
            try
            {
                using (var _DbContext = new EntityDataContext(_connection))
                {
                    var comment = await _DbContext.TaskComments.FindAsync(id);
                    if (comment == null) return false;

                    _DbContext.TaskComments.Remove(comment);
                    await _DbContext.SaveChangesAsync();
                    return true;
                }
            }
            catch (Exception ex)
            {
                LogHelper.InsertLogTelegram("DeleteComment - TaskCommentDAL: " + ex);
                return false;
            }
        }
    }
}
