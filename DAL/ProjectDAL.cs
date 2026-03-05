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
    public class ProjectDAL : GenericService<Projects>
    {
        public ProjectDAL(string connection) : base(connection)
        {
        }

        public async Task<List<Projects>> GetAllProjects()
        {
            try
            {
                using (var _DbContext = new EntityDataContext(_connection))
                {
                    return await _DbContext.Projects
                        .OrderByDescending(p => p.CreatedDate)
                        .ToListAsync();
                }
            }
            catch (Exception ex)
            {
                LogHelper.InsertLogTelegram("GetAllProjects - ProjectDAL: " + ex);
                return new List<Projects>();
            }
        }

        public async Task<long> Upsert(Projects model)
        {
            try
            {
                using (var _DbContext = new EntityDataContext(_connection))
                {
                    if (model.Id > 0)
                    {
                        var exist = await _DbContext.Projects.FindAsync(model.Id);
                        if (exist != null)
                        {
                            exist.ProjectName = model.ProjectName;
                            exist.Description = model.Description;
                            _DbContext.Projects.Update(exist);
                        }

                    }
                    else
                    {
                        model.CreatedDate = DateTime.Now;
                        await _DbContext.Projects.AddAsync(model);
                    }
                    await _DbContext.SaveChangesAsync();
                    return model.Id;
                }
            }
            catch (Exception ex)
            {
                LogHelper.InsertLogTelegram("Upsert - ProjectDAL: " + ex);
                return -1;
            }
        }
    }
}
