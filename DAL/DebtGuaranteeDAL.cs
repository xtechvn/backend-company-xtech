using DAL.Generic;
using DAL.StoreProcedure;
using Entities.Models;
using Entities.ViewModels.DebtGuarantee;
using Microsoft.Data.SqlClient;

using System;
using System.Collections.Generic;
using System.Data;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Utilities;
using Utilities.Contants;

namespace DAL
{
    public class DebtGuaranteeDAL : GenericService<DebtGuarantee> 
    {
        private DbWorker _dbWorker;
        public DebtGuaranteeDAL(string connection) : base(connection)
        {
            _dbWorker = new DbWorker(connection);
        }
        //public async Task<DataTable> GetListDebtGuarantee(SearchDebtGuarantee model)
        //{
        //    try
        //    {
        //        SqlParameter[] objParam = new SqlParameter[10];
        //        objParam[0] = new SqlParameter("@Code", model.Code);
        //        objParam[1] = new SqlParameter("@Status", model.Status);
        //        objParam[2] = new SqlParameter("@OrderId", model.OrderId);
        //        objParam[3] = new SqlParameter("@PageIndex", model.PageIndex);
        //        objParam[4] = new SqlParameter("@PageSize", model.PageSize);
        //        objParam[5] = new SqlParameter("@SalerPermission", model.SalerPermission);
        //        objParam[6] = (CheckDate(model.CreateTime) == DateTime.MinValue) ? new SqlParameter("@CreateTime", DBNull.Value) : new SqlParameter("@CreateTime", CheckDate(model.CreateTime));
        //        objParam[7] = (CheckDate(model.ToDateTime) == DateTime.MinValue) ? new SqlParameter("@ToDateTime", DBNull.Value) : new SqlParameter("@ToDateTime", CheckDate(model.ToDateTime).AddDays(1));
        //        objParam[8] = new SqlParameter("@ClientId", model.ClientId); 
        //        objParam[9] = new SqlParameter("@DepartmentId", model.DepartmentId); 
        //        return _dbWorker.GetDataTable(StoreProcedureConstant.SP_GetListDebtGuarantee, objParam);

        //    }
        //    catch (Exception ex)
        //    {
        //        LogHelper.InsertLogTelegram("DebtGuaranteeDAL GetListDebtGuarantee" + ex);
        //        return null;
        //    }
        //}
        //public async Task<long> InsertDebtGuarantee(DebtGuarantee model)
        //{
        //    try
        //    {
        //        SqlParameter[] objParam = new SqlParameter[6];
        //        objParam[0] = new SqlParameter("@Code", model.Code);
        //        objParam[1] = new SqlParameter("@Orderid", model.Orderid);
        //        objParam[2] = new SqlParameter("@ClientId", model.ClientId);
        //        objParam[3] = new SqlParameter("@Status", model.Status);
        //        objParam[4] = new SqlParameter("@CreatedBy", model.CreatedBy);
        //        objParam[5] = new SqlParameter("@CreatedDate", DBNull.Value);
        //        return  _dbWorker.ExecuteNonQuery(StoreProcedureConstant.sp_InsertDebtGuarantee, objParam);

        //    }
        //    catch (Exception ex)
        //    {
        //        LogHelper.InsertLogTelegram("DebtGuaranteeDAL InsertDebtGuarantee" + ex);
        //        return -1;
        //    }
        //}
        public async Task<long> UpdateDebtGuarantee(DebtGuarantee model)
        {
            try
            {
                SqlParameter[] objParam = new SqlParameter[6];
                objParam[0] = new SqlParameter("@Id", model.Id);
                objParam[1] = new SqlParameter("@Code", model.Code == null ? DBNull.Value : model.Code);
                objParam[2] = new SqlParameter("@Orderid", model.Orderid == null ? DBNull.Value : model.Orderid);
                objParam[3] = new SqlParameter("@ClientId", model.ClientId==null?DBNull.Value: model.ClientId);
                objParam[4] = new SqlParameter("@Status", model.Status);
                objParam[5] = new SqlParameter("@UpdatedBy", model.UpdatedBy);
                return _dbWorker.ExecuteNonQuery(StoreProcedureConstant.sp_UpdateDebtGuarantee, objParam);

            }
            catch (Exception ex)
            {
                LogHelper.InsertLogTelegram("DebtGuaranteeDAL UpdateDebtGuarantee" + ex);
                return -1;
            }
        }
        //public async Task<DataTable> DetailDebtGuarantee(int Id)
        //{
        //    try
        //    {
        //        SqlParameter[] objParam = new SqlParameter[1];
        //        objParam[0] = new SqlParameter("@Id", Id);
     
        //        return _dbWorker.GetDataTable(StoreProcedureConstant.SP_GeDetailDebtGuarantee, objParam);

        //    }
        //    catch (Exception ex)
        //    {
        //        LogHelper.InsertLogTelegram("DebtGuaranteeDAL DetailDebtGuarantee" + ex);
        //        return null;
        //    }
        //}   
        public async Task<DataTable> DetailDebtGuaranteebyOrderid(int Orderid)
        {
            try
            {
                SqlParameter[] objParam = new SqlParameter[1];
                objParam[0] = new SqlParameter("@Orderid", Orderid);
     
                return _dbWorker.GetDataTable(StoreProcedureConstant.SP_GetDetailDebtGuaranteeByOrderid, objParam);

            }
            catch (Exception ex)
            {
                LogHelper.InsertLogTelegram("DebtGuaranteeDAL DetailDebtGuarantee" + ex);
                return null;
            }
        }
        private DateTime CheckDate(string dateTime)
        {
            DateTime _date = DateTime.MinValue;
            if (!string.IsNullOrEmpty(dateTime))
            {
                _date = DateTime.ParseExact(dateTime, "d/M/yyyy", CultureInfo.InvariantCulture);
            }

            return _date != DateTime.MinValue ? _date : DateTime.MinValue;
        }
        //public async Task<DataTable>SumTotalGetListDebtGuarantee(SearchDebtGuarantee model)
        //{
        //    try
        //    {
        //        SqlParameter[] objParam = new SqlParameter[8];
        //        objParam[0] = new SqlParameter("@Code", model.Code);
        //        objParam[1] = new SqlParameter("@Status", model.Status);
        //        objParam[2] = new SqlParameter("@OrderId", model.OrderId);
        //        objParam[3] = new SqlParameter("@SalerPermission", model.SalerPermission);
        //        objParam[4] = (CheckDate(model.CreateTime) == DateTime.MinValue) ? new SqlParameter("@CreateTime", DBNull.Value) : new SqlParameter("@CreateTime", CheckDate(model.CreateTime));
        //        objParam[5] = (CheckDate(model.ToDateTime) == DateTime.MinValue) ? new SqlParameter("@ToDateTime", DBNull.Value) : new SqlParameter("@ToDateTime", CheckDate(model.ToDateTime).AddDays(1));
        //        objParam[6] = new SqlParameter("@ClientId", model.ClientId);
        //        objParam[7] = new SqlParameter("@DepartmentId", model.DepartmentId);
        //        return _dbWorker.GetDataTable(StoreProcedureConstant.SP_SumTotalGetListDebtGuarantee, objParam);

        //    }
        //    catch (Exception ex)
        //    {
        //        LogHelper.InsertLogTelegram("SumTotalGetListDebtGuarantee GetListDebtGuarantee" + ex);
        //        return null;
        //    }
        //}
    }
}
