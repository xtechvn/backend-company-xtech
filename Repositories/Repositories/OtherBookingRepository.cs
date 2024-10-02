using DAL;
using Entities.ConfigModels;
using Entities.ViewModels;
using Entities.ViewModels.OtherBooking;
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
        public OtherBookingRepository(IOptions<DataBaseConfig> dataBaseConfig)
        {
            _OtherBookingDal = new OtherBookingDAL(dataBaseConfig.Value.SqlServer.ConnectionString);
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
    }
}
