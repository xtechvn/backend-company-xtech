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
    public class SprintDAL : GenericService<Sprint>
    {
        public SprintDAL(string connection) : base(connection)
        {
        }

        public async Task<List<Sprint>> GetAllSprints(long? projectId = null)
        {
            try
            {
                using (var _DbContext = new EntityDataContext(_connection))
                {
                    return await _DbContext.Sprints
                        .Where(s => s.ProjectId == projectId)
                        .OrderByDescending(s => s.CreatedDate)
                        .ToListAsync();
                }
            }
            catch (Exception ex)
            {
                LogHelper.InsertLogTelegram("GetAllSprints - SprintDAL: " + ex);
                return new List<Sprint>();
            }
        }

        public async Task<long> Upsert(Sprint model)
        {
            try
            {
                using (var _DbContext = new EntityDataContext(_connection))
                {
                    if (model.Id > 0)
                    {
                        var exist = await _DbContext.Sprints.FindAsync(model.Id);
                        if (exist != null)
                        {
                            exist.SprintName = model.SprintName;
                            exist.StartDate = model.StartDate;
                            exist.EndDate = model.EndDate;
                            exist.Goal = model.Goal;
                            exist.Status = model.Status;
                            exist.ProjectId = model.ProjectId;
                            exist.ModifiedDate = DateTime.Now;
                            exist.ModifiedBy = model.ModifiedBy;
                            _DbContext.Sprints.Update(exist);
                        }
                    }
                    else
                    {
                        model.CreatedDate = DateTime.Now;
                        await _DbContext.Sprints.AddAsync(model);
                    }
                    await _DbContext.SaveChangesAsync();
                    return model.Id;
                }
            }
            catch (Exception ex)
            {
                LogHelper.InsertLogTelegram("Upsert - SprintDAL: " + ex);
                return -1;
            }
        }

        public async Task<bool> UpdateStatus(long sprintId, int status)
        {
            try
            {
                using (var _DbContext = new EntityDataContext(_connection))
                {
                    var exist = await _DbContext.Sprints.FindAsync(sprintId);
                    if (exist != null)
                    {
                        exist.Status = status;
                        await _DbContext.SaveChangesAsync();
                        return true;
                    }
                }
            }
            catch (Exception ex)
            {
                LogHelper.InsertLogTelegram("UpdateStatus - SprintDAL: " + ex);
            }
            return false;
        }
    }
}
