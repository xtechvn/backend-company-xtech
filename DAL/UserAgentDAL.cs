using DAL.StoreProcedure;
using Entities.Models;
using Entities.ViewModels.UserAgent;
using Microsoft.Data.SqlClient;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Ultilities.Constants;
using Utilities;
using Utilities.Contants;

namespace DAL
{
    public class UserAgentDAL
    {
        private static DbWorker _DbWorker;
        public UserAgentDAL(string connection)
        {
            _DbWorker = new DbWorker(connection);
        }

        public List<UserAgentViewModel> GeListUserAgentByClient(int ClientId, long id)
        {
            try
            {

                SqlParameter[] objParam = new SqlParameter[1];
                objParam[0] = new SqlParameter("@ClientId", ClientId > 0 ? ClientId : DBNull.Value);


                DataTable dt = _DbWorker.GetDataTable(StoreProcedureConstant.SP_GetUserAgentByClientId, objParam);
                if (dt != null && dt.Rows.Count > 0)
                {
                    var data = dt.ToList<UserAgentViewModel>();
                    return data;
                }
            }
            catch (Exception ex)
            {
                LogHelper.InsertLogTelegram("UserAgentByClient - UserAgentDAL: " + ex);

            }
            return null;
        }

        public int CreateUserAgent(UserAgent model)
        {
            try
            {
                SqlParameter[] sqlParameters = new SqlParameter[] 
                {
                    new SqlParameter("@UserId",model.UserId),
                    new SqlParameter("@ClientId",model.ClientId),
                    new SqlParameter("@MainFollow",model.MainFollow),
                    new SqlParameter("@CreateDate",model.CreateDate),
                    new SqlParameter("@UpdateLast",model.UpdateLast),
                    new SqlParameter("@VerifyDate",model.VerifyDate),
                    new SqlParameter("@VerifyStatus",model.VerifyStatus),
                    new SqlParameter("@CreatedBy",model.CreatedBy)
                };
                var rs = _DbWorker.ExecuteNonQuery(StoreProcedureConstant.SP_InsertUserAgent, sqlParameters);
                return 1;
            }
            catch (Exception ex)
            {
                LogHelper.InsertLogTelegram("CreateUserAgent - UserAgentDAL: " + ex);
                return 0;
            }
        }

        public int UpdataUserAgent(int Id, int UserId, int create_id, long ClientId)
        {
            try
            {
                if (Id == 0)
                {
                    SqlParameter[] objParam = new SqlParameter[8];
                    objParam[0] = new SqlParameter("@UserId", UserId);
                    objParam[1] = new SqlParameter("@ClientId", ClientId);
                    objParam[2] = new SqlParameter("@MainFollow", DBNull.Value);
                    objParam[3] = new SqlParameter("@CreateDate", DateTime.Now);
                    objParam[4] = new SqlParameter("@UpdateLast", DateTime.Now);
                    objParam[5] = new SqlParameter("@VerifyDate", DateTime.Now);
                    objParam[6] = new SqlParameter("@VerifyStatus", VerifyStatus.DA_DUYET);
                    objParam[7] = new SqlParameter("@CreatedBy", create_id);

                    return _DbWorker.ExecuteNonQuery(StoreProcedureConstant.SP_InsertUserAgent, objParam);
                }
                else
                {
                    SqlParameter[] objParam = new SqlParameter[6];
                    objParam[0] = new SqlParameter("@Id", Id);
                    objParam[1] = new SqlParameter("@UserId", UserId);
                    objParam[2] = new SqlParameter("@MainFollow", DBNull.Value);
                    objParam[3] = new SqlParameter("@VerifyDate", DateTime.Now);
                    objParam[4] = new SqlParameter("@VerifyStatus", VerifyStatus.DA_DUYET);
                    objParam[5] = new SqlParameter("@UpdateBy", create_id);

                    return _DbWorker.ExecuteNonQuery(StoreProcedureConstant.SP_UpdateUserAgent, objParam);
                }


                return 1;
            }
            catch (Exception ex)
            {
                LogHelper.InsertLogTelegram("UpdataUserAgent - UserAgentDAL: " + ex);
                return 0;
            }
        }

        public UserAgent GetUserAgentByClientId(int ClientId)
        {
            try
            {
                SqlParameter[] sqlParameters = new SqlParameter[]
                 {
                    new SqlParameter("@ClientId",ClientId)
                 };
                var rs = _DbWorker.GetDataTable(StoreProcedureConstant.SP_GetUserAgentByClientId, sqlParameters);
                if (rs != null && rs.Rows.Count > 0) 
                {
                    var userAgents = rs.ToList<UserAgent>();
                    return userAgents[0];
                }
                return null;
            }
            catch (Exception ex)
            {
                LogHelper.InsertLogTelegram("GetUserAgentByClientId - UserAgentDAL: " + ex);
                return null;
            }
        }
    }
}
