using Entities.Models;
using Entities.ViewModels.Galaxy;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Repositories.IRepositories
{
    public interface IBookingVPSRepository
    {
        int bookingVPS(GalaxyViewModel data);
    }
}
