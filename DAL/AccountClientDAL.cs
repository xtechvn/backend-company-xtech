using DAL.StoreProcedure;
using Entities.Models;
using Microsoft.Data.SqlClient;
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
    public class AccountClientDAL
    {
        private static DbWorker _DbWorker;
        public AccountClientDAL(string connection)
        {
            _DbWorker = new DbWorker(connection);
        }
        public AccountClient AccountClientByClientId(long client_id)
        {
            try
            {
                SqlParameter[] sqlParameter = new SqlParameter[]
                {
                    new SqlParameter("@client_id", client_id)
                };
                DataTable dataTable = _DbWorker.GetDataTable(StoreProcedureConstant.SP_GetAccountClientByClientId, sqlParameter);
                if (dataTable != null && dataTable.Rows.Count > 0)
                {
                    var data = dataTable.ToList<AccountClient>();
                    return data[0];
                }
                return null;
            }
            catch (Exception ex)
            {
                LogHelper.InsertLogTelegram("AccountClientByClientId - AccountClientDAL: " + ex.ToString());

            }
            return null;

        }

        public long GetMainAccountClientByClientId(long client_id)
        {
            try
            {
                SqlParameter[] sqlParameter = new SqlParameter[]
                {
                    new SqlParameter("@client_id", client_id)
                };
                DataTable dataTable = _DbWorker.GetDataTable(StoreProcedureConstant.SP_GetAccountClientByClientId, sqlParameter);
                if (dataTable != null && dataTable.Rows.Count > 0)
                {
                    var data = dataTable.ToList<AccountClient>();
                    return data[0].Id;
                }
                return 0;
            }
            catch (Exception ex)
            {
                LogHelper.InsertLogTelegram("GetMainAccountClientByClientId - AccountClientDAL: " + ex.ToString());

            }
            return -1;

        }

        public int CreateAccountClient(AccountClient model)
        {
            try
            {
                SqlParameter[] objParam = new SqlParameter[7];
                objParam[0] = new SqlParameter("@ClientId ", model.ClientId);
                objParam[1] = new SqlParameter("@ClientType", model.ClientType);
                objParam[2] = new SqlParameter("@UserName", model.UserName);
                objParam[3] = new SqlParameter("@Password", model.Password);
                objParam[4] = new SqlParameter("@PasswordBackup", model.PasswordBackup);
                objParam[5] = new SqlParameter("@ForgotPasswordToken", model.ForgotPasswordToken);
                objParam[6] = new SqlParameter("@Status", model.Status);

                return _DbWorker.ExecuteNonQuery(StoreProcedureConstant.SP_InsertAccountClient, objParam);
            }
            catch (Exception ex)
            {
                LogHelper.InsertLogTelegram("CreateAccountClient - AccountClientDAL: " + ex);
                return 0;
            }
        }

        public AccountClient GetAccountClientByClientId(long client_id)
        {
            try
            {
                SqlParameter[] sqlParameters = new SqlParameter[]
                {
                    new SqlParameter("@ClientId",client_id)
                };
                var rs = _DbWorker.GetDataTable(StoreProcedureConstant.SP_GetAccountClientByClientId, sqlParameters);
                if (rs != null && rs.Rows.Count > 0)
                {
                    var data = rs.ToList<AccountClient>();
                    return data[0];
                }
                return null;
            }
            catch (Exception ex)
            {
                LogHelper.InsertLogTelegram("AccountClientByClientId - AccountClientDAL: " + ex.ToString());

            }
            return null;
        }

        public async Task<int> UpdataAccountClient(AccountClient model)
        {
            try
            {
                SqlParameter[] objParam = new SqlParameter[10];
                objParam[0] = new SqlParameter("@Id ", model.Id);
                objParam[1] = new SqlParameter("@ClientId ", model.ClientId);
                objParam[2] = new SqlParameter("@ClientType", model.ClientType);
                objParam[3] = new SqlParameter("@UserName", model.UserName);
                objParam[4] = new SqlParameter("@Password", model.Password);
                objParam[5] = new SqlParameter("@PasswordBackup", model.PasswordBackup);
                objParam[6] = new SqlParameter("@ForgotPasswordToken", model.ForgotPasswordToken);
                objParam[7] = new SqlParameter("@Status", model.Status);

                return _DbWorker.ExecuteNonQuery(StoreProcedureConstant.SP_UpdateAccountClient, objParam);
            }
            catch (Exception ex)
            {
                LogHelper.InsertLogTelegram("UpdataAccountClient - AccountClientDAL: " + ex.ToString());

            }
            return 0;

        }
    }
}
