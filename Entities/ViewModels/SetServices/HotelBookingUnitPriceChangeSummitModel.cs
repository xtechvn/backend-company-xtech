 using Entities.Models;
using System.Collections.Generic;
using System.Text;

namespace Entities.ViewModels.HotelBookingRoom
{
  public class HotelBookingUnitPriceChangeSummitModel 
  {
       public List<HotelBookingUnitPriceChangeSummitRoom> rooms { get; set; }
       public List<HotelBookingUnitPriceChangeSummitExtra> extra_packages { get; set; }
  }

    public class HotelBookingUnitPriceChangeSummitExtra
    {
        public long id { get; set; }
        public double operator_price { get; set; }
        public double unit_price { get; set; }
        public int supplier_id { get; set; }
    }

    public class HotelBookingUnitPriceChangeSummitRoom
    {
        public long id { get; set; }
        public long room_id { get; set; }
        public bool is_room_fund { get; set; }
        public int number_of_room { get; set; }
        public int suplier_id { get; set; }
        public List<HotelBookingUnitPriceChangeSummitRoomRate> rates { get; set; }
        public string package_name { get; set; }

    }

    public class HotelBookingUnitPriceChangeSummitRoomRate
    {
        public long id { get; set; }
        public long rate_id { get; set; }
        public double operator_price { get; set; }
        public double sale_price { get; set; }
        public double profit { get; set; }
        public int nights { get; set; }
        public double operator_amount { get; set; }
        public double sale_amount{ get; set; }
    }
}
