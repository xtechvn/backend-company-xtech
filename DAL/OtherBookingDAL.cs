using DAL.StoreProcedure;
using Entities.ViewModels.OtherBooking;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore.Infrastructure;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Utilities;

namespace DAL
{
    public class OtherBookingDAL
    {
        private static DbWorker _DbWorker;
        public OtherBookingDAL(string connection)
        {
            _DbWorker = new DbWorker(connection);
        }

        public async Task<DataTable> GetAllOtherBookingByOrderId(int OrderId) 
        {
            try 
            {
                SqlParameter[] sqlParameter = new SqlParameter[]
                {
                    new SqlParameter("@OrderId",OrderId != null ? OrderId : DBNull.Value)
                };

                return _DbWorker.GetDataTable(ProcedureConstants.SP_GetListOtherBookingByOrderId, sqlParameter);
            }
            catch(Exception ex) 
            {
                LogHelper.InsertLogTelegram("GetAllOtherBookingByOrderId - OtherBookingDAL: " + ex);
            }
            return null;
        }

        public async Task<DataTable> GetOtherBookingById(int? Id)
        {
            try
            {
                SqlParameter[] sqlParameter = new SqlParameter[]
                {
                    new SqlParameter("@Id",Id != null ? Id : DBNull.Value)
                };

                return _DbWorker.GetDataTable(ProcedureConstants.sp_GetOtherBookingById, sqlParameter);
            }
            catch (Exception ex)
            {
                LogHelper.InsertLogTelegram("GetAllOtherBookingByOrderId - OtherBookingDAL: " + ex);
            }
            return null;
        }

        public async Task<int> SetUpOtherBooking(OtherBookingSubmitModel model)
        {
            try
            {
                if (model.Id == null)
                {
                    int defaultStatus = 0;
                    SqlParameter[] sqlParameter = new SqlParameter[]
                    {
                         new SqlParameter("@OrderId",model.OrderId != null? model.OrderId : DBNull.Value),
                         new SqlParameter("@ServiceType",model.ServiceType != null? model.ServiceType : DBNull.Value),
                         new SqlParameter("@Status",defaultStatus),
                         new SqlParameter("@ServiceCode","SERVICE_TYPE"),
                         new SqlParameter("@Amount",model.Amount != null ? model.Amount : DBNull.Value),
                         new SqlParameter("@Note",model.Note != null ? model.Note : DBNull.Value),
                         new SqlParameter("@OperatorID",model.OperatorID != null ? model.OperatorID : DBNull.Value),
                         new SqlParameter("@Profit",model.Profit != null ? model.Profit : DBNull.Value),
                         new SqlParameter("@OthersAmount",model.OthersAmount != null ? model.OthersAmount : DBNull.Value),
                         new SqlParameter("@StartDate",model.StartDate != null ? model.StartDate : DBNull.Value),
                         new SqlParameter("@EndDate",model.EndDate != null ? model.EndDate : DBNull.Value)
                    };

                    return _DbWorker.ExecuteNonQuery(ProcedureConstants.sp_InsertOtherBooking, sqlParameter);
                }
                else 
                {
                    SqlParameter[] sqlParameter = new SqlParameter[]
                    {
                         new SqlParameter("@Id",model.Id != null? model.Id : DBNull.Value),
                         new SqlParameter("@OrderId",model.OrderId != null? model.OrderId : DBNull.Value),
                         new SqlParameter("@ServiceType",model.ServiceType != null? model.ServiceType : DBNull.Value),
                         new SqlParameter("@Status",model.Status != null ? model.Status : model.Status),
                         new SqlParameter("@ServiceCode",model.ServiceCode!= null? model.ServiceCode : DBNull.Value),
                         new SqlParameter("@OperatorID",model.OperatorID != null ? model.OperatorID : DBNull.Value),
                         new SqlParameter("@Amount",model.Amount != null ? model.Amount : DBNull.Value),
                         new SqlParameter("@Note",model.Note != null ? model.Note : DBNull.Value),
                         new SqlParameter("@Profit",model.Profit != null ? model.Profit : DBNull.Value),
                         new SqlParameter("@OthersAmount",model.OthersAmount != null ? model.OthersAmount : DBNull.Value),
                         new SqlParameter("@StartDate",model.StartDate != null ? model.StartDate : DBNull.Value),
                         new SqlParameter("@EndDate",model.EndDate != null ? model.EndDate : DBNull.Value)
                    };

                    return _DbWorker.ExecuteNonQuery(ProcedureConstants.sp_UpdateOtherBooking, sqlParameter);
                }
            }
            catch (Exception ex)
            {
                LogHelper.InsertLogTelegram("GetAllOtherBookingByOrderId - OtherBookingDAL: " + ex);
            }
            return -1;
        }
    }
}
