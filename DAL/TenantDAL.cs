using DAL.Generic;
using DAL.StoreProcedure;
using Entities.Models;
using Entities.ViewModels;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore.Storage;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Utilities;
using Utilities.Contants;

namespace DAL
{
    public class TenantDAL : GenericService<User>
    {
        private static DbWorker _DbWorker;
        public TenantDAL(string connection) : base(connection)
        {
            _DbWorker = new DbWorker(connection);
        }
        public async Task<int> InsertTenant(UserViewModel model)
        {
            try
            {
                SqlParameter[] sqlParameter = new SqlParameter[]
                {
                    new SqlParameter("@Email", model.Email),
                    new SqlParameter("@Phone", model.Phone),
                    new SqlParameter("@Address", model.Address),
                    new SqlParameter("@SurrogateName", model.FullName),
                    new SqlParameter("@SurrogatePhone", model.Phone),
                    new SqlParameter("@SurrogateEmail", model.Email),
                    new SqlParameter("@Status", model.Status),
                    new SqlParameter("@CreatedBy", model.Id),
                    new SqlParameter("@CreatedDate", DBNull.Value),
                    new SqlParameter("@UserName", model.UserName),
                    new SqlParameter("@Password", EncodeHelpers.MD5Hash(model.Password)),
                };
                return _DbWorker.ExecuteNonQuery(StoreProcedureConstant.sp_InsertTenant, sqlParameter);
            }
            catch (Exception ex)
            {
                LogHelper.InsertLogTelegram("InsertTenant - TenantDAL: " + ex.ToString());
                return -1;
            }
        }
        public int UpdateTenant(UserViewModel model)
        {
            try
            {

                var parameters = new SqlParameter[]
                {
                  new SqlParameter("@TenantId", model.TenantId),
                  new SqlParameter("@Email", model.Email),
                    new SqlParameter("@Phone", model.Phone),
                    new SqlParameter("@Address", model.Address),
                    new SqlParameter("@SurrogateName", model.FullName),
                    new SqlParameter("@SurrogatePhone", model.Phone),
                    new SqlParameter("@SurrogateEmail", model.Email),
                    new SqlParameter("@Status", model.Status),
                    new SqlParameter("@UpdatedBy", model.CreatedBy),
                    new SqlParameter("@IsDelete", model.IsDelete==null?DBNull.Value:model.IsDelete),
                   
                };
                var id = _DbWorker.ExecuteNonQuery(StoreProcedureConstant.sp_UpdateTenant, parameters);
                model.Id = id;
                return id;
            }
            catch (Exception ex)
            {
                LogHelper.InsertLogTelegram("UpdateTenant - TenantDAL: " + ex);
                return -1;
            }
        }
        public async Task<DataTable> GetListTenant(TenantSearchModel model)
        {
            try
            {

                var parameters = new SqlParameter[]
                {
                  new SqlParameter("@UserName", model.UserName),
                  new SqlParameter("@SurrogateName", model.SurrogateName),
                  new SqlParameter("@status", model.status),
                  new SqlParameter("@PageIndex", model.PageIndex),
                  new SqlParameter("@PageSize", model.PageSize),
                };
                return  _DbWorker.GetDataTable(StoreProcedureConstant.SP_GetListTenant, parameters);
              
            }
            catch (Exception ex)
            {
                LogHelper.InsertLogTelegram("GetListTenant - TenantDAL: " + ex);
                return null;
            }
        }   
        public async Task<TenantViewModel> GetDetailTenantByTenantId(int TenantId)
        {
            try
            {

                var parameters = new SqlParameter[]
                {
                  new SqlParameter("@TenantId",TenantId),
          
                };
                var dt=  _DbWorker.GetDataTable(StoreProcedureConstant.SP_GetDetailTenantByTenantId, parameters);
                if (dt != null && dt.Rows.Count > 0)
                {
                    var data = dt.ToList<TenantViewModel>();
                    return data[0];
                }
                return null;
            }
            catch (Exception ex)
            {
                LogHelper.InsertLogTelegram("GetListTenant - TenantDAL: " + ex);
                return null;
            }
        }
    }
}
