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
    public class ProjectTaskDAL : GenericService<ProjectTask>
    {
        public ProjectTaskDAL(string connection) : base(connection)
        {
        }

        public async System.Threading.Tasks.Task<List<ProjectTask>> GetTasksBySprint(long? sprintId, long? projectId = null)
        {
            try
            {
                using (var _DbContext = new EntityDataContext(_connection))
                {
                    var query = _DbContext.ProjectTasks.AsQueryable();
                    if (projectId != null) query = query.Where(t => t.ProjectId == projectId);
                    query = query.Where(t => t.SprintId == sprintId);
                    
                    return await query.OrderBy(t => t.TaskOrder).ToListAsync();
                }
            }
            catch (Exception ex)
            {
                LogHelper.InsertLogTelegram("GetTasksBySprint - ProjectTaskDAL: " + ex);
                return new List<ProjectTask>();
            }
        }

        public async System.Threading.Tasks.Task<List<ProjectTask>> GetTasksBySprints(List<long> sprintIds, long? projectId = null)
        {
            try
            {
                using (var _DbContext = new EntityDataContext(_connection))
                {
                    var query = _DbContext.ProjectTasks.AsQueryable();
                    if (projectId != null) query = query.Where(t => t.ProjectId == projectId);
                    query = query.Where(t => t.SprintId.HasValue && sprintIds.Contains(t.SprintId.Value));
                    
                    return await query.OrderBy(t => t.TaskOrder).ToListAsync();
                }
            }
            catch (Exception ex)
            {
                LogHelper.InsertLogTelegram("GetTasksBySprints - ProjectTaskDAL: " + ex);
                return new List<ProjectTask>();
            }
        }

        public async System.Threading.Tasks.Task<long> Upsert(ProjectTask model)
        {
            try
            {
                using (var _DbContext = new EntityDataContext(_connection))
                {
                    if (model.Id > 0)
                    {
                        var success = await UpdateAsync(model);
                        return success ? model.Id : -1;
                    }
                    else
                    {
                        model.CreatedDate = DateTime.Now;
                   
                        await _DbContext.ProjectTasks.AddAsync(model);
                        await _DbContext.SaveChangesAsync();
                        return model.Id;
                    }
                }
            }
            catch (Exception ex)
            {
                LogHelper.InsertLogTelegram("Upsert - ProjectTaskDAL: " + ex);
                return -1;
            }
        }

        public async System.Threading.Tasks.Task<bool> UpdateAsync(ProjectTask model)
        {
            try
            {
                using (var _DbContext = new EntityDataContext(_connection))
                {
                    var exist = await _DbContext.ProjectTasks.FindAsync(model.Id);
                    if (exist != null)
                    {
                        exist.Title = model.Title;
                        exist.Description = model.Description;
                        exist.SprintId = model.SprintId;
                        exist.StatusId = model.StatusId;
                        exist.TaskOrder = model.TaskOrder;
                        exist.AssigneeId = model.AssigneeId;
                        exist.ReporterId = model.ReporterId;
                        exist.PriorityId = model.PriorityId;
                        exist.StoryPoint = model.StoryPoint;
                        exist.DueDate = model.DueDate;
                        exist.Label = model.Label;
                        exist.Attachment = model.Attachment;
                        exist.ProjectId = model.ProjectId;
                        exist.TaskType = model.TaskType;
                        exist.ModifiedDate = DateTime.Now;
                        exist.ModifiedBy = model.ModifiedBy;
                        
                        _DbContext.ProjectTasks.Update(exist);
                        await _DbContext.SaveChangesAsync();
                        return true;
                    }
                }
            }
            catch (Exception ex)
            {
                LogHelper.InsertLogTelegram("UpdateAsync - ProjectTaskDAL: " + ex);
            }
            return false;
        }

        public async System.Threading.Tasks.Task<bool> UpdateSprint(long taskId, long? sprintId)
        {
            try
            {
                using (var _DbContext = new EntityDataContext(_connection))
                {
                    var exist = await _DbContext.ProjectTasks.FindAsync(taskId);
                    if (exist != null)
                    {
                        exist.SprintId = sprintId;
                        await _DbContext.SaveChangesAsync();
                        return true;
                    }
                }
            }
            catch (Exception ex)
            {
                LogHelper.InsertLogTelegram("UpdateSprint - ProjectTaskDAL: " + ex);
            }
            return false;
        }

        public async System.Threading.Tasks.Task<bool> UpdateSprints(List<long> taskIds, long? sprintId)
        {
            try
            {
                using (var _DbContext = new EntityDataContext(_connection))
                {
                    var tasks = await _DbContext.ProjectTasks.Where(t => taskIds.Contains(t.Id)).ToListAsync();
                    if (tasks.Any())
                    {
                        foreach (var task in tasks)
                        {
                            task.SprintId = sprintId;
                        }
                        await _DbContext.SaveChangesAsync();
                        return true;
                    }
                }
            }
            catch (Exception ex)
            {
                LogHelper.InsertLogTelegram("UpdateSprints - ProjectTaskDAL: " + ex);
            }
            return false;
        }

        public async System.Threading.Tasks.Task<bool> UpdateStatus(long taskId, int statusId)
        {
            try
            {
                using (var _DbContext = new EntityDataContext(_connection))
                {
                    var exist = await _DbContext.ProjectTasks.FindAsync(taskId);
                    if (exist != null)
                    {
                        exist.StatusId = statusId;
                        await _DbContext.SaveChangesAsync();
                        return true;
                    }
                }
            }
            catch (Exception ex)
            {
                LogHelper.InsertLogTelegram("UpdateStatus - ProjectTaskDAL: " + ex);
            }
            return false;
        }
    }
}
