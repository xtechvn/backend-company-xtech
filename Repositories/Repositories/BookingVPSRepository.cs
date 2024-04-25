using DAL;
using Entities.ConfigModels;
using Entities.Models;
using Entities.ViewModels.Galaxy;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Repositories.IRepositories;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Repositories.Repositories
{
    public class BookingVPSRepository: IBookingVPSRepository
    {

    
        private readonly BookingVPSDAL _bookingVPSDAL;

        public BookingVPSRepository(IOptions<DataBaseConfig> dataBaseConfig)
        {
            _bookingVPSDAL = new BookingVPSDAL(dataBaseConfig.Value.SqlServer.ConnectionString);
        }
        public int bookingVPS(GalaxyViewModel data)
        {
            return _bookingVPSDAL.InsertBookingvps(data);
        }
    }
}
