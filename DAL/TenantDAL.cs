using DAL.Generic;
using DAL.StoreProcedure;
using Entities.Models;
using Entities.ViewModels;
using Microsoft.Data.SqlClient;
using System;
using System.Collections.Generic;
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
        public int UpsertUser(UserViewModel user)
        {
            try
            {

                var parameters = new SqlParameter[]
                {
                    new SqlParameter("@UserMapId", user.Id),
                    new SqlParameter("@UserName", user.UserName),
                    new SqlParameter("@FullName", user.FullName),
                    new SqlParameter("@Password", EncodeHelpers.MD5Hash(user.Password)) ,
                    new SqlParameter("@ResetPassword",EncodeHelpers.MD5Hash(user.Password)),
                    new SqlParameter("@Phone", user.Phone),
                    new SqlParameter("@BirthDay", user.BirthDay),
                    new SqlParameter("@Gender", user.Gender),
                    new SqlParameter("@Email", user.Email),
                    new SqlParameter("@Avata", user.Avata),
                    new SqlParameter("@Address",user.Address),
                    new SqlParameter("@Status", user.Status),
                    new SqlParameter("@Note",user.Note==null?"":user.Note),
                    new SqlParameter("@Manager",user.CreatedBy),
                    new SqlParameter("@DepartmentId", user.DepartmentId),
                    new SqlParameter("@Level", user.Level),
                    new SqlParameter("@UserPositionId",  user.UserPositionId),
                    new SqlParameter("@CompanyType",  user.CompanyType),
                    new SqlParameter("@NickName",  user.UserName),
                    new SqlParameter("@TenantId",  user.TenantId),
                    new SqlParameter("@Type",  user.Type),
                    new SqlParameter("@CreatedBy", user.CreatedBy)
                };
                var id = _DbWorker.ExecuteNonQuery(StoreProcedureConstant.sp_InsertUser, parameters);
                user.Id = id;
                return id;
            }
            catch (Exception ex)
            {
                LogHelper.InsertLogTelegram("UpsertUser - UserDAL: " + ex);
                return -1;
            }
        }
    }
}
