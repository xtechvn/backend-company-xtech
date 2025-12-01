using DAL.Generic;
using DAL.StoreProcedure;
using Entities.Models;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Utilities;
using Utilities.Contants;

namespace DAL
{
    public class OtherBookingPackagesOptionalDAL : GenericService<OtherBookingPackagesOptional>
    {
        private static DbWorker _DbWorker;

        public OtherBookingPackagesOptionalDAL(string connection) : base(connection)
        {
            _DbWorker = new DbWorker(connection);

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
        public async Task<long> CreateOrUpdatePackageOptional(OtherBookingPackagesOptional packages)
        {
            try
            {
                using (var _DbContext = new EntityDataContext(_connection))
                {
                    var exists = _DbContext.FlyBookingPackagesOptional.AsNoTracking().FirstOrDefault(s => s.Id == packages.Id);
                    if (exists != null && exists.Id > 0)
                    {
                        return UpdateOtherBookingPackagesOptional(packages);
                    }
                    else
                    {
                        return CreateOtherBookingPackagesOptional(packages);

                    }
                }

            }
            catch (Exception ex)
            {
                LogHelper.InsertLogTelegram("CreateFlyBookingPackagesOptional - HotelBookingDAL. " + ex);
                return -1;
            }
        }
        public async Task<int> RemoveNonExistsBookingOptional(List<long> remain_list,long booking_id)
        {
            try
            {
                using (var _DbContext = new EntityDataContext(_connection))
                {
                    var delete_list= await _DbContext.OtherBookingPackagesOptional.Where(x => x.BookingId == booking_id && !remain_list.Contains(x.Id)).ToListAsync();
                    if(delete_list!=null && delete_list.Count > 0)
                    {
                        _DbContext.OtherBookingPackagesOptional.RemoveRange(delete_list);
                        await _DbContext.SaveChangesAsync();
                    }
                    return 1;
                }
            }
            catch (Exception ex)
            {
                LogHelper.InsertLogTelegram("OtherBookingPackagesOptional - OtherBookingDAL: " + ex);
                return -1;
            }
        }
        public int CreateOtherBookingPackagesOptional(OtherBookingPackagesOptional packages)
        {
            try
            {

                SqlParameter[] objParam_order = new SqlParameter[9];
                objParam_order[0] = new SqlParameter("@BookingId", packages.BookingId);
                objParam_order[1] = new SqlParameter("@SuplierID", packages.SuplierId);
                objParam_order[2] = new SqlParameter("@Amount", packages.Amount);
                if (packages.Note != null)
                {
                    objParam_order[3] = new SqlParameter("@Note", packages.Note);
                }
                else
                {
                    objParam_order[3] = new SqlParameter("@Note", DBNull.Value);

                }
                objParam_order[4] = new SqlParameter("@CreatedBy", packages.CreatedBy);
                objParam_order[5] = new SqlParameter("@PackageName", packages.PackageName);
                objParam_order[6] = new SqlParameter("@BasePrice", packages.BasePrice);
                objParam_order[7] = new SqlParameter("@Quantity", packages.Quantity);
                objParam_order[8] = new SqlParameter("@Status", packages.Status);

                var id = _DbWorker.ExecuteNonQuery(StoreProcedureConstant.InsertOtherBookingPackagesOptional, objParam_order);
                packages.Id = id;
                return id;
            }
            catch (Exception ex)
            {
                LogHelper.InsertLogTelegram("CreateFlyBookingPackagesOptional - HotelBookingDAL. " + ex);
                return -1;
            }
        }
        public int UpdateOtherBookingPackagesOptional(OtherBookingPackagesOptional packages)
        {
            try
            {

                SqlParameter[] objParam_order = new SqlParameter[10];
                objParam_order[0] = new SqlParameter("@BookingId", packages.BookingId);
                objParam_order[1] = new SqlParameter("@SuplierID", packages.SuplierId);
                objParam_order[2] = new SqlParameter("@Amount", packages.Amount);
                if (packages.Note != null)
                {
                    objParam_order[3] = new SqlParameter("@Note", packages.Note);
                }
                else
                {
                    objParam_order[3] = new SqlParameter("@Note", DBNull.Value);

                }
                objParam_order[4] = new SqlParameter("@UpdatedBy", packages.UpdatedBy);
                objParam_order[5] = new SqlParameter("@Id", packages.Id);
                objParam_order[6] = new SqlParameter("@PackageName", packages.PackageName);
                objParam_order[7] = new SqlParameter("@BasePrice", packages.BasePrice);
                objParam_order[8] = new SqlParameter("@Quantity", packages.Quantity);
                objParam_order[9] = new SqlParameter("@Status", packages.Status);
                _DbWorker.ExecuteNonQueryNoIdentity(StoreProcedureConstant.UpdateOtherBookingPackagesOptional, objParam_order);
                return 1;
            }
            catch (Exception ex)
            {
                LogHelper.InsertLogTelegram("UpdateOtherBookingPackagesOptional - HotelBookingDAL. " + ex);
                return -1;
            }
        }
    }
}
