using System;
using System.Collections.Generic;
using System.Text;

namespace Utilities.Contants
{
    public enum ServicesType
    {
        VINHotelRent=1,
        OthersHotelRent=2,
        FlyingTicket=3,
        VehicleRent=4,
        Tourist=5,
        VinWonder = 6,
        Other = 9,
        WaterSport=10
    }
    public static class ServicesTypeCode
    {
        public static readonly Dictionary<Int16, string> service = new Dictionary<Int16, string>
        {
            {Convert.ToInt16(ServicesType.VINHotelRent), "HOTEL" },
            {Convert.ToInt16(ServicesType.OthersHotelRent), "HOTEL" },
            {Convert.ToInt16(ServicesType.FlyingTicket), "FLIGHT" },
            {Convert.ToInt16(ServicesType.VehicleRent), "VEHICLE" },
            {Convert.ToInt16(ServicesType.Tourist), "TOUR" },
            {Convert.ToInt16(ServicesType.VinWonder), "VINWONDER" },
            {Convert.ToInt16(ServicesType.Other), "OTHER" }

        };
    }
}
