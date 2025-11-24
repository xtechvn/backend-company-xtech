using Entities.Models;
using Entities.ViewModels.HotelBookingCode;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Repositories.IRepositories
{
   public interface IHotelBookingCodeRepository
    {
        //Task<List<HotelBookingCodeModel>> GetListlBookingCodeByHotelBookingId(long HotelBookingId,int Type);
       
        Task DeleteBookingCodeByIdandNote(long data_id, string note);
    }
}
