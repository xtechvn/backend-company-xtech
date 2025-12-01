using DAL;
using Entities.ConfigModels;
using Entities.Models;
using Entities.ViewModels;
using Entities.ViewModels.OtherBooking;
using Entities.ViewModels.SetServices;
using Microsoft.Extensions.Options;
using Repositories.IRepositories;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Utilities;

namespace Repositories.Repositories
{
    public class OtherBookingRepository : IOtherBookingRepository
    {
        private readonly OtherBookingDAL _OtherBookingDal;
        private readonly OtherBookingPackagesOptionalDAL otherBookingPackagesOptionalDAL;
        public OtherBookingRepository(IOptions<DataBaseConfig> dataBaseConfig)
        {
            _OtherBookingDal = new OtherBookingDAL(dataBaseConfig.Value.SqlServer.ConnectionString);
            otherBookingPackagesOptionalDAL = new OtherBookingPackagesOptionalDAL(dataBaseConfig.Value.SqlServer.ConnectionString);
        }
        public async Task<List<OtherBookingViewModel>> GetAllOtherBookingByOrderId(int OrderId)
        {
            var model = new List<OtherBookingViewModel>();
            try
            {
                var dt = await _OtherBookingDal.GetAllOtherBookingByOrderId(OrderId);
                if (dt != null && dt.Rows.Count > 0) 
                {
                    model = dt.ToList<OtherBookingViewModel>();
                }
                return model;
            }
            catch (Exception ex)
            {
                LogHelper.InsertLogTelegram("GetAllOtherBookingByOrderId - OtherBookingRepository: " + ex);
            }
            return model;
        }

        public async Task<OtherBookingViewModel> GetOtherBookingById(int? Id)
        {
            var model = new List<OtherBookingViewModel>();
            try
            {
                DataTable dt = await _OtherBookingDal.GetOtherBookingById(Id);
                if (dt != null && dt.Rows.Count > 0)
                {
                    model = dt.ToList<OtherBookingViewModel>();
                }
                return model[0];
            }
            catch (Exception ex)
            {
                LogHelper.InsertLogTelegram("GetAllOtherBookingByOrderId - OtherBookingRepository: " + ex);
            }
            return null;
        }
        public async Task<long> UpdateOtherBookingOptional(List<OtherBookingPackagesOptional> data, long booking_id, int user_summit)
        {
            try
            {
                double price = 0;
                if (data != null && data.Count > 0)
                {
                    List<long> remain_list = new List<long>();
                    foreach (var item in data)
                    {
                        if (item.Note != null && item.Note.Trim() != "")
                        {
                            item.Note = CommonHelper.RemoveSpecialCharacterExceptVietnameseCharacter(item.Note);
                        }
                        if (item.Status != 1)
                            price += item.Amount > 0 ? item.Amount : 0;
                        item.CreatedBy = user_summit;
                        item.UpdatedBy = user_summit;
                        var id = await otherBookingPackagesOptionalDAL.CreateOrUpdatePackageOptional(item);
                        remain_list.Add(item.Id);
                    }
                    await _OtherBookingDal.UpdateOtherBookingPrice(booking_id, price, user_summit);
                    await otherBookingPackagesOptionalDAL.RemoveNonExistsBookingOptional(remain_list, booking_id);
                    return data[0].Id;
                }
            }
            catch (Exception ex)
            {
                LogHelper.InsertLogTelegram("UpdateFlyBookingOptional - FlyBookingDetailRepository: " + ex);

            }
            return 0;

        }
        public async Task<OtherBooking> GetOtherBookingById2(long booking_id)
        {
            return _OtherBookingDal.GetOtherBookingById2(booking_id);
        }
        public async Task<List<OtherBooking>> ServiceCodeSuggesstion(string txt_search = "")
        {
            try
            {
                return await _OtherBookingDal.ServiceCodeSuggesstion(txt_search);
            }
            catch (Exception ex)
            {
                LogHelper.InsertLogTelegram("CancelTourByID - TourRepository: " + ex);

            }
            return new List<OtherBooking>();
        }
        public async Task<List<OtherBookingPackages>> GetOtherBookingPackagesByBookingId(long booking_id)
        {
            return _OtherBookingDal.GetOtherBookingPackagesByBookingId(booking_id);
        }
        public async Task<List<OtherBookingPackagesOptional>> GetOtherBookingPackagesOptionalByBookingId(long booking_id)
        {
            return _OtherBookingDal.GetOtherBookingPackagesOptionalByBookingId(booking_id);
        }
        public async Task<long> UpdateServiceOperator(long booking_id, int user_id, int user_commit)
        {
            return await _OtherBookingDal.UpdateServiceOperator(booking_id, user_id, user_commit);
        }
        public async Task<long> UpdateServiceOperator2(long booking_id, int user_id)
        {
            return await _OtherBookingDal.UpdateServiceOperator2(booking_id, user_id);
        }
        public async Task<long> UpdateServiceStatus(int status, long booking_id, int user_id)
        {
            return await _OtherBookingDal.UpdateServiceStatus(status, booking_id, user_id);
        }
        public async Task<int> SetUpOtherBooking(OtherBookingSubmitModel model)
        {
            try
            {
                return await _OtherBookingDal.SetUpOtherBooking(model);
            }
            catch (Exception ex)
            {
                LogHelper.InsertLogTelegram("CreateOrder - OtherBookingRepository: " + ex);
                return -1;
            }
        }
        public async Task<GenericViewModel<OtherBookingSearchViewModel>> GetPagingList(SearchFlyBookingViewModel searchModel, int currentPage, int pageSize)
        {
            var model = new GenericViewModel<OtherBookingSearchViewModel>();
            try
            {

                DataTable dt = _OtherBookingDal.GetPagingList(searchModel, currentPage, pageSize);
                if (dt != null && dt.Rows.Count > 0)
                {
                    model.ListData = dt.ToList<OtherBookingSearchViewModel>();
                    model.CurrentPage = currentPage;
                    model.PageSize = pageSize;
                    model.TotalRecord = Convert.ToInt32(dt.Rows[0]["TotalRow"]);
                    model.TotalPage = (int)Math.Ceiling((double)model.TotalRecord / model.PageSize);
                    return model;
                }
                return model;
            }
            catch (Exception ex)
            {
                LogHelper.InsertLogTelegram("GetPagingList - FlyBookingDetailRepository: " + ex);
                return null;
            }
        }
    }
}
