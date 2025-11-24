using Entities.Models;
using Entities.ViewModels;
using Entities.ViewModels.OtherBooking;
using Entities.ViewModels.SetServices;
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
        Task<GenericViewModel<OtherBookingSearchViewModel>> GetPagingList(SearchFlyBookingViewModel searchModel, int currentPage, int pageSize);
        Task<List<OtherBookingViewModel>> GetAllOtherBookingByOrderId(int OrderId);
        Task<int> SetUpOtherBooking(OtherBookingSubmitModel model);
        Task<OtherBookingViewModel> GetOtherBookingById(int? Id);

        Task<long> UpdateServiceStatus(int status, long booking_id, int user_id);
        public Task<OtherBooking> GetOtherBookingById2(long booking_id);
        Task<List<OtherBooking>> ServiceCodeSuggesstion(string txt_search = "");
        public Task<List<OtherBookingPackages>> GetOtherBookingPackagesByBookingId(long booking_id);
        Task<List<OtherBookingPackagesOptional>> GetOtherBookingPackagesOptionalByBookingId(long booking_id);
        Task<long> UpdateServiceOperator(long booking_id, int user_id, int user_commit);
        Task<long> UpdateServiceOperator2(long booking_id, int user_id);


    }
}
