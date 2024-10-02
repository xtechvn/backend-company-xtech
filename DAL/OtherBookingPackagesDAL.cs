using DAL.StoreProcedure;
using Entities.ViewModels.OtherBooking;
using Entities.ViewModels.OtherBookingPackage;
using Microsoft.Data.SqlClient;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Utilities;

namespace DAL
{
    public class OtherBookingPackagesDAL
    {
        private static DbWorker _DbWorker;
        public OtherBookingPackagesDAL(string connection)
        {
            _DbWorker = new DbWorker(connection);
        }
        public async Task<DataTable> GetListPackageByBookingId(int? BookingId) 
        {
            try
            {
                SqlParameter[] sqlParameter = new SqlParameter[]
                {
                    new SqlParameter("@OtherBookingId",BookingId != null ? BookingId : DBNull.Value)
                };

                return _DbWorker.GetDataTable(ProcedureConstants.SP_GetListOtherBookingPackagesByBookingId, sqlParameter);
            }
            catch (Exception ex)
            {
                LogHelper.InsertLogTelegram("GetListPackageByBookingId - OtherBookingPackagesDAL: " + ex);
            }
            return null;
        }

        public async Task<int> SetUpOtherBookingPackages(OtherBookingPackageSubmitModel model)
        {
            try
            {
                if (model.Id == 0)
                {
                    SqlParameter[] sqlParameter = new SqlParameter[]
                    {
                         new SqlParameter("@BookingId",model.BookingId != null? model.BookingId : DBNull.Value),
                         new SqlParameter("@Name",model.Name != null ? model.Name : model.Name),
                         new SqlParameter("@BasePrice",model.BasePrice!= null? model.BasePrice : DBNull.Value),
                         new SqlParameter("@Amount",model.Amount != null ? model.Amount : DBNull.Value),
                         new SqlParameter("@Quantity",model.Quantity != null ? model.Quantity : DBNull.Value),
                         new SqlParameter("@Profit",model.Profit != null ? model.Profit : DBNull.Value),
                         new SqlParameter("@SalePrice",model.SalePrice != null ? model.SalePrice : DBNull.Value),
                          new SqlParameter("@ServiceType",model.ServiceType != null ? model.ServiceType : DBNull.Value),
                         new SqlParameter("@Note",model.Note != null ? model.Note : DBNull.Value)
                    };

                    return _DbWorker.ExecuteNonQuery(ProcedureConstants.sp_InsertOtherBookingPackages, sqlParameter);
                }
                else
                {
                    SqlParameter[] sqlParameter = new SqlParameter[]
                     {
                         new SqlParameter("@Id",model.Id != null? model.Id : DBNull.Value),
                         new SqlParameter("@BookingId",model.BookingId != null? model.BookingId : DBNull.Value),
                         new SqlParameter("@Name",model.Name != null ? model.Name : model.Name),
                         new SqlParameter("@BasePrice",model.BasePrice!= null? model.BasePrice : DBNull.Value),
                         new SqlParameter("@Amount",model.Amount != null ? model.Amount : DBNull.Value),
                         new SqlParameter("@Quantity",model.Quantity != null ? model.Quantity : DBNull.Value),
                         new SqlParameter("@Profit",model.Profit != null ? model.Profit : DBNull.Value),
                         new SqlParameter("@SalePrice",model.SalePrice != null ? model.SalePrice : DBNull.Value),
                          new SqlParameter("@ServiceType",model.ServiceType != null ? model.ServiceType : DBNull.Value),
                         new SqlParameter("@Note",model.Note != null ? model.Note : DBNull.Value)
                     };

                    return _DbWorker.ExecuteNonQuery(ProcedureConstants.sp_UpdateOtherBookingPackages, sqlParameter);
                }
            }
            catch (Exception ex)
            {
                LogHelper.InsertLogTelegram("SetUpOtherBookingPackages - OtherBookingPackagesDAL: " + ex);
            }
            return -1;
        }

        public async Task<int> DeleteOtherBookingPackage(int Id) 
        {
            try 
            {
                SqlParameter[] sqlParameter = new SqlParameter[]
                {
                    new SqlParameter("@Id",Id),
                };
                return _DbWorker.ExecuteNonQuery(ProcedureConstants.SP_Delete_OtherBookingPackage, sqlParameter);
            }
            catch (Exception ex) 
            {
                LogHelper.InsertLogTelegram("DeleteOtherBookingPackages - OtherBookingPackagesDAL: " + ex);
            }
            return -1;
        }
    }
}
