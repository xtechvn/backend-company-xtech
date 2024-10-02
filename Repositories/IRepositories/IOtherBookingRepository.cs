using Entities.ViewModels;
using Entities.ViewModels.OtherBooking;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Repositories.IRepositories
{
    public interface IOtherBookingRepository
    {
        Task<List<OtherBookingViewModel>> GetAllOtherBookingByOrderId(int OrderId);
        Task<int> SetUpOtherBooking(OtherBookingSubmitModel model);
        Task<OtherBookingViewModel> GetOtherBookingById(int? Id);
    }
}
