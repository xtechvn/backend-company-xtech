using Entities.Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace Entities.ViewModels.HotelBookingCode
{
    public class HotelBookingCodeViewModel
    {
        public long Id { get; set; }
        public int Type { get; set; }
        public long HotelBookingId { get; set; }
        public string BookingCode { get; set; }
        public string Description { get; set; }
        public string Note { get; set; }
        public string AttactFile { get; set; }
        public int IsDelete { get; set; }
        public int CreatedBy { get; set; }
        public int UpdatedBy { get; set; }
        public string CreatedDate { get; set; }
        public string UpdatedDate { get; set; }
        public long OrderId { get; set; }

    }
    public class SendEmailViewModel
    {
        public string Subject { get; set; }
        public long ServiceId { get; set; }
        public long Orderid { get; set; }
        public string To_Email { get; set; }
        public string CC_Email { get; set; }
        public string BCC_Email { get; set; }
        public string Email { get; set; }
        public string OrderNote { get; set; }
        public string PaymentNotification { get; set; }
        public int ServiceType { get; set; }
        public string group_booking_id { get; set; }

        public long type { get; set; }
        public long OrderAmount { get; set; }
        public string OrderNo { get; set; }
        public string saler_Email { get; set; }
        public string saler_Name { get; set; }
        public string saler_Phone { get; set; }
        public string user_Email { get; set; }
        public string user_Phone { get; set; }
        public string user_Name { get; set; }
        public string TTChuyenKhoan { get; set; }
        public string TTNote { get; set; }
        public string NDChuyenKhoan { get; set; }
        public string TileEmail { get; set; }

        public string go_numberOfChild { get; set; }
        public string go_numberOfInfant { get; set; }
        public string go_numberOfAdult { get; set; }
        public string go_startpoint { get; set; }
        public string back_numberOfChild { get; set; }
        public string back_numberOfAdult { get; set; }
        public string back_numberOfInfant { get; set; }
        public string back_startdate { get; set; }
        public string back_enddate { get; set; }
        public string back_startpoint { get; set; }
        public string back_endpoint { get; set; }
        public string back_airline { get; set; }

        public string go_endpoint { get; set; }
        public string DKHuy { get; set; }
        public string go_airline { get; set; }

        public string go_startdate { get; set; }
        public string go_enddate { get; set; }

        public string datatable { get; set; }
        public string datatableCode { get; set; }
        public long totalAmount { get; set; }
        public string totalToday { get; set; }
        public string NumberOfRoom { get; set; }

        public string code_1_code { get; set; }
        public string code_1_description { get; set; }
        public string code_2_code { get; set; }
        public string code_2_description { get; set; }



        public string fyAmount { get; set; }
        public string fyAdultNumber { get; set; }
        public string fyChildNumber { get; set; }
        public string fyInfantNumber { get; set; }
        public string fyBookingCode { get; set; }
        public string fyAirlineName_Vi { get; set; }
        public string fyBookingCode2 { get; set; }
        public string fyAirlineName_Vi2 { get; set; }
        public string fyEndDistrict2 { get; set; }
        public string fyStartDistrict2 { get; set; }
        public string fyEndDate { get; set; }
        public string fyStartDate { get; set; }
        public string fyNote { get; set; }
        public string fyStartDistrict { get; set; }
        public string fyEndDistrict { get; set; }

        public string hotelNote { get; set; }
        public string hotelAmount { get; set; }
        public string hotelTotalDays { get; set; }
        public string hotelNumberOfAdult { get; set; }
        public string hotelNumberOfChild { get; set; }
        public string hotelNumberOfInfant { get; set; }
        public string hotelNumberOfRoom { get; set; }
        public string hotelDepartureDate { get; set; }
        public string hotelArrivalDate { get; set; }

        public string tourNote { get; set; }
        public string tourAmount { get; set; }
        public string tourTotalAdult { get; set; }
        public string tourTotalChildren { get; set; }
        public string tourTotalBaby { get; set; }
        public string tourORGANIZINGName { get; set; }
        public string tourEndDate { get; set; }
        public string tourStartDate { get; set; }
        public string tourGroupEndPoint3 { get; set; }
        public string tourStartPoint3 { get; set; }
        public string tourGroupEndPoint2 { get; set; }
        public string tourStartPoint2 { get; set; }
        public string tourGroupEndPoint1 { get; set; }
        public string tourStartPoint1 { get; set; }

        public string OtherStartDate { get; set; }
        public string OtherEndDate { get; set; }
        public string OtherAmount { get; set; }
        public string OtherNote { get; set; }

        public long SupplierId { get; set; }
        public List<OtherEmail> OtherEmail { get; set; }
        public List<TourEmail> TourEmail { get; set; }
        public List<HotelEmail> HotelEmail { get; set; }
        public List<FyEmail> FyEmail { get; set; }
        public List<VinWonderEmail> VinWonderEmail { get; set; }
        public List<PackagesEmail> Packagesemail { get; set; }

    }
    public class PackagesEmail
    {
        public string Packages_Type { get; set; }
        public string date { get; set; }
        public string Packagesdetail { get; set; }
    }
    public class OtherEmail
    {
        public string OtherServiceName { get; set; }
        public string OtherStartDate { get; set; }
        public string OtherEndDate { get; set; }
        public string OtherAmount { get; set; }
        public string OtherNote { get; set; }
    }
    public class TourEmail
    {
        public string TourProductName { get; set; }
        public string tourNote { get; set; }
        public string tourAmount { get; set; }
        public string tourTotalAdult { get; set; }
        public string tourTotalChildren { get; set; }
        public string tourTotalBaby { get; set; }
        public string tourORGANIZINGName { get; set; }
        public string tourEndDate { get; set; }
        public string tourStartDate { get; set; }
        public string tourGroupEndPoint3 { get; set; }
        public string tourStartPoint3 { get; set; }
        public string tourGroupEndPoint2 { get; set; }
        public string tourStartPoint2 { get; set; }
        public string tourGroupEndPoint1 { get; set; }
        public string tourStartPoint1 { get; set; }

    }
    public class HotelEmail
    {
        public string HotelId { get; set; }
        public string HotelName { get; set; }
        public string hotelNote { get; set; }
        public string hotelAmount { get; set; }
        public string hotelTotalDays { get; set; }
        public string hotelNumberOfAdult { get; set; }
        public string hotelNumberOfChild { get; set; }
        public string hotelNumberOfInfant { get; set; }
        public string hotelNumberOfRoom { get; set; }
        public string hotelDepartureDate { get; set; }
        public string hotelArrivalDate { get; set; }
    }
    public class FyEmail
    {
        public string fyAmount { get; set; }
        public string fyAdultNumber { get; set; }
        public string fyChildNumber { get; set; }
        public string fyInfantNumber { get; set; }
        public string fyBookingCode { get; set; }
        public string fyAirlineName_Vi { get; set; }
        public string fyBookingCode2 { get; set; }
        public string fyAirlineName_Vi2 { get; set; }
        public string fyEndDistrict2 { get; set; }
        public string fyStartDistrict2 { get; set; }
        public string fyEndDate { get; set; }
        public string fyStartDate { get; set; }
        public string fyNote { get; set; }
        public string fyStartDistrict { get; set; }
        public string fyEndDistrict { get; set; }
    }
    public class VinWonderEmail
    {
        public string VinWonderAmount { get; set; }
        public string VinWonderStartDate { get; set; }
        public string VinWonderName { get; set; }
    }
   
    public class EmailYCChiViewModel
    {
        public string GhiChu { get; set; }
        public string Swiftcode { get; set; }
    }
}
