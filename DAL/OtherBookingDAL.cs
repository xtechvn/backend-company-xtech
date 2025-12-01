using DAL.Generic;
using DAL.StoreProcedure;
using Entities.Models;
using Entities.ViewModels.OtherBooking;
using Entities.ViewModels.SetServices;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
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
    public class OtherBookingDAL : GenericService<OtherBooking>
    {
        private static DbWorker _DbWorker;
        public OtherBookingDAL(string connection) : base(connection)
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
        public async Task<long> UpdateOtherBookingPrice(long booking_id, double price, int user_summit)
        {
            try
            {
                using (var _DbContext = new EntityDataContext(_connection))
                {
                    var exists = _DbContext.OtherBooking.FirstOrDefault(x => x.Id == booking_id);
                    if (exists != null && exists.Id > 0)
                    {
                        exists.Price = price;
                        exists.UpdatedBy = user_summit;
                        exists.UpdatedDate = DateTime.Now;
                        _DbContext.OtherBooking.Update(exists);
                        await _DbContext.SaveChangesAsync();
                    }
                    return 1;
                }
            }
            catch (Exception ex)
            {
                LogHelper.InsertLogTelegram("UpdateOtherBookingPrice - OtherBookingDAL: " + ex);
                return -1;
            }
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
        public OtherBooking GetOtherBookingById2(long booking_id)
        {
            try
            {
                using (var _DbContext = new EntityDataContext(_connection))
                {
                    return _DbContext.OtherBooking.FirstOrDefault(x => x.Id == booking_id);
                }
            }
            catch (Exception ex)
            {
                LogHelper.InsertLogTelegram("GetOtherBookingById - OtherBookingDAL: " + ex);
                return null;
            }
        }
        public async Task<List<OtherBooking>> ServiceCodeSuggesstion(string txt_search)
        {
            try
            {
                using (var _DbContext = new EntityDataContext(_connection))
                {
                    return await _DbContext.OtherBooking.AsNoTracking().Where(x => x.ServiceCode.ToLower().Contains(txt_search.ToLower())).Skip(0).Take(30).ToListAsync();
                }
            }
            catch (Exception ex)
            {
                LogHelper.InsertLogTelegram("ServiceCodeSuggesstion - OtherBookingDAL: " + ex);
                return null;
            }
        }
        public List<OtherBookingPackages> GetOtherBookingPackagesByBookingId(long booking_id)
        {
            try
            {
                using (var _DbContext = new EntityDataContext(_connection))
                {
                    return _DbContext.OtherBookingPackages.Where(x => x.BookingId == booking_id).ToList();
                }
            }
            catch (Exception ex)
            {
                LogHelper.InsertLogTelegram("GetOtherBookingPackagesByBookingId - OtherBookingPackagesDAL: " + ex);
                return null;
            }
        }
        public List<OtherBookingPackagesOptional> GetOtherBookingPackagesOptionalByBookingId(long booking_id)
        {
            try
            {
                using (var _DbContext = new EntityDataContext(_connection))
                {
                    return _DbContext.OtherBookingPackagesOptional.Where(x => x.BookingId == booking_id).ToList();
                }
            }
            catch (Exception ex)
            {
                LogHelper.InsertLogTelegram("OtherBookingPackagesOptional - OtherBookingDAL: " + ex);
                return null;
            }
        }
        public async Task<long> UpdateServiceOperator(long booking_id, int user_id, int user_commit)
        {
            try
            {
                using (var _DbContext = new EntityDataContext(_connection))
                {
                    var exists = await _DbContext.OtherBooking.AsNoTracking().FirstOrDefaultAsync(s => s.Id == booking_id);
                    if (exists != null && exists.Id > 0)
                    {
                        exists.UpdatedDate = DateTime.Now;
                        exists.UpdatedBy = user_commit;
                        exists.OperatorId = user_id;
                        _DbContext.OtherBooking.Update(exists);
                        await _DbContext.SaveChangesAsync();
                    }
                    return 1;
                }
            }
            catch (Exception ex)
            {
                LogHelper.InsertLogTelegram("UpdateServiceOperator - OtherBookingDAL: " + ex);
                return -2;
            }
        }
        public async Task<long> UpdateServiceOperator2(long booking_id, int user_id)
        {
            try
            {
                using (var _DbContext = new EntityDataContext(_connection))
                {
                    var exists = await _DbContext.OtherBooking.AsNoTracking().Where(s => s.Id == booking_id).FirstOrDefaultAsync();
                    if (exists != null && exists.Id > 0 && exists.Status == (int)ServiceStatus.WaitingExcution)
                    {
                        exists.UpdatedDate = DateTime.Now;
                        exists.UpdatedBy = user_id;
                        exists.OperatorId = user_id;
                        exists.Status = (int)ServiceStatus.OnExcution;
                        _DbContext.OtherBooking.Update(exists);
                        await _DbContext.SaveChangesAsync();
                    }
                    return 1;
                }
            }
            catch (Exception ex)
            {
                LogHelper.InsertLogTelegram("UpdateServiceOperator - OtherBookingDAL: " + ex);
                return -2;
            }
        }
        public async Task<long> UpdateServiceStatus(int status, long booking_id, int user_id)
        {
            try
            {
                using (var _DbContext = new EntityDataContext(_connection))
                {
                    var exists = await _DbContext.OtherBooking.AsNoTracking().Where(s => s.Id == booking_id).FirstOrDefaultAsync();
                    if (exists != null && exists.Id > 0)
                    {
                        exists.StatusOld = exists.Status;
                        exists.UpdatedDate = DateTime.Now;
                        exists.UpdatedBy = user_id;
                        exists.Status = status;
                        _DbContext.OtherBooking.Update(exists);
                        await _DbContext.SaveChangesAsync();
                    }
                    return 1;
                }
            }
            catch (Exception ex)
            {
                LogHelper.InsertLogTelegram("UpdateServiceStatus - OtherBookingDAL: " + ex);
                return -2;
            }
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
        public DataTable GetPagingList(SearchFlyBookingViewModel searchModel, int currentPage, int pageSize)
        {
            try
            {

                SqlParameter[] objParam = new SqlParameter[17];
                if (searchModel.ServiceCode != null)
                {
                    objParam[0] = new SqlParameter("@ServiceCode", searchModel.ServiceCode);
                }
                else
                {
                    objParam[0] = new SqlParameter("@ServiceCode", DBNull.Value);

                }
                if (searchModel.OrderCode != null)
                {
                    objParam[1] = new SqlParameter("@OrderCode", searchModel.OrderCode);
                }
                else
                {
                    objParam[1] = new SqlParameter("@OrderCode", DBNull.Value);

                }
                objParam[2] = new SqlParameter("@StatusBooking", searchModel.StatusBooking);
                if (searchModel.StartDateFrom != null)
                {
                    objParam[3] = new SqlParameter("@StartDateFrom", ((DateTime)searchModel.StartDateFrom).Date);
                }
                else
                {
                    objParam[3] = new SqlParameter("@StartDateFrom", DBNull.Value);

                }
                if (searchModel.StartDateTo != null)
                {
                    objParam[4] = new SqlParameter("@StartDateTo", ((DateTime)searchModel.StartDateTo).Date);
                }
                else
                {
                    objParam[4] = new SqlParameter("@StartDateTo", DBNull.Value);

                }
                if (searchModel.EndDateFrom != null)
                {
                    objParam[5] = new SqlParameter("@EndDateFrom", ((DateTime)searchModel.EndDateFrom).Date);
                }
                else
                {
                    objParam[5] = new SqlParameter("@EndDateFrom", DBNull.Value);

                }
                if (searchModel.EndDateTo != null)
                {
                    objParam[6] = new SqlParameter("@EndDateTo", ((DateTime)searchModel.EndDateTo).Date);
                }
                else
                {
                    objParam[6] = new SqlParameter("@EndDateTo", DBNull.Value);

                }
                if (searchModel.UserCreate != null)
                {
                    objParam[7] = new SqlParameter("@UserCreate", searchModel.UserCreate);
                }
                else
                {
                    objParam[7] = new SqlParameter("@UserCreate", DBNull.Value);

                }
                if (searchModel.CreateDateFrom != null)
                {
                    objParam[8] = new SqlParameter("@CreateDateFrom", ((DateTime)searchModel.CreateDateFrom).Date);
                }
                else
                {
                    objParam[8] = new SqlParameter("@CreateDateFrom", DBNull.Value);

                }
                if (searchModel.CreateDateTo != null)
                {
                    objParam[9] = new SqlParameter("@CreateDateTo", ((DateTime)searchModel.CreateDateTo).Date);
                }
                else
                {
                    objParam[9] = new SqlParameter("@CreateDateTo", DBNull.Value);

                }
                if (searchModel.SalerId > 0)
                {
                    objParam[10] = new SqlParameter("@SalerId", searchModel.SalerId);
                }
                else
                {
                    objParam[10] = new SqlParameter("@SalerId", DBNull.Value);

                }
                if (searchModel.OperatorId > 0)
                {
                    objParam[11] = new SqlParameter("@OperatorId", searchModel.OperatorId);
                }
                else
                {
                    objParam[11] = new SqlParameter("@OperatorId", DBNull.Value);

                }
                objParam[12] = new SqlParameter("@PageIndex", currentPage);
                objParam[13] = new SqlParameter("@PageSize", pageSize);
                if (searchModel.SalerPermission != null)
                {
                    objParam[14] = new SqlParameter("@SalerPermission", searchModel.SalerPermission);
                }
                else
                {
                    objParam[14] = new SqlParameter("@SalerPermission", DBNull.Value);

                }
                if (searchModel.BookingCode != null)
                {
                    objParam[15] = new SqlParameter("@BookingCode", searchModel.BookingCode);
                }
                else
                {
                    objParam[15] = new SqlParameter("@BookingCode", DBNull.Value);

                }
                if (searchModel.ServiceType != null && searchModel.ServiceType.Count > 0)
                {
                    objParam[16] = new SqlParameter("@ServiceType", string.Join(",", searchModel.ServiceType));
                }
                else
                {
                    objParam[16] = new SqlParameter("@ServiceType", DBNull.Value);

                }
                string procedure = StoreProcedureConstant.GetListOtherBooking;

                return _DbWorker.GetDataTable(procedure, objParam);
            }
            catch (Exception ex)
            {
                LogHelper.InsertLogTelegram("GetPagingList - OtherBookingDAL: " + ex);
            }
            return null;
        }
    }
}
