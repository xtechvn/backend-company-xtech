using Entities.ViewModels.OtherBookingPackage;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Repositories.IRepositories
{
    public interface IOtherBookingPackageRepository
    {
        Task<List<OtherBookingPackageViewModel>> GetListOtherBookingPackageByBookingId(int? OrderId);
        Task<int> SetUpOtherBookingPackage(OtherBookingPackageSubmitModel model);
        Task<int> DeleteOtherBookingPackage(int Id);
    }
}
