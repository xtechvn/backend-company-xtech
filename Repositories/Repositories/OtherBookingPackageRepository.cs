using DAL;
using Entities.ConfigModels;
using Entities.ViewModels.OtherBooking;
using Entities.ViewModels.OtherBookingPackage;
using Microsoft.Extensions.Options;
using Repositories.IRepositories;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Utilities;

namespace Repositories.Repositories
{
    public class OtherBookingPackageRepository : IOtherBookingPackageRepository
    {
        private readonly OtherBookingPackagesDAL _OtherBookingPackageDal;
        public OtherBookingPackageRepository(IOptions<DataBaseConfig> dataBaseConfig)
        {
            _OtherBookingPackageDal = new OtherBookingPackagesDAL(dataBaseConfig.Value.SqlServer.ConnectionString);
        }

        public async Task<int> DeleteOtherBookingPackage(int Id)
        {
            try
            {
                return await _OtherBookingPackageDal.DeleteOtherBookingPackage(Id);
            }
            catch (Exception ex)
            {
                LogHelper.InsertLogTelegram("SetUpOtherBookingPackage - OtherBookingPackageRepository: " + ex);
                return -1;
            }
        }

        public async Task<List<OtherBookingPackageViewModel>> GetListOtherBookingPackageByBookingId(int? OrderId)
        {
            var model = new List<OtherBookingPackageViewModel>();
            try
            {
                var dt = await _OtherBookingPackageDal.GetListPackageByBookingId(OrderId);
                if (dt != null && dt.Rows.Count > 0)
                {
                    model = dt.ToList<OtherBookingPackageViewModel>();
                }
                return model;
            }
            catch (Exception ex)
            {
                LogHelper.InsertLogTelegram("GetListOtherBookingPackageByBookingId - OtherBookingPackageRepository: " + ex);
            }
            return model;
        }

        public async Task<int> SetUpOtherBookingPackage(OtherBookingPackageSubmitModel model)
        {
            try
            {
                return await _OtherBookingPackageDal.SetUpOtherBookingPackages(model);
            }
            catch (Exception ex)
            {
                LogHelper.InsertLogTelegram("SetUpOtherBookingPackage - OtherBookingPackageRepository: " + ex);
                return -1;
            }
        }
    }
}
