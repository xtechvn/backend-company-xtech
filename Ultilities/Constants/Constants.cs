using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace Utilities.Contants
{
    public class Constants
    {
        public const string Success = "SUCCESS";
        public const string Fail = "FAIL";
        public const string Error = "ERROR";

        public enum SortOrder
        {

            TRUY_CAP = 1,//Truy cập
            THEM = 2,//Thêm
            SUA = 3,//Sửa
            XOA = 4,//Xóa
            XUAT_BC = 5,//Xuất báo cáo
            VIEW_ALL = 6,//View All Data
            DUYET = 7,//Duyệt
        }
        public enum MenuId
        {
            HOP_DONG = 54,//Hợp đồng
            PHIEU_YEU_CAU_CHI = 64,//yêu cầu chi
            PHIEU_CHI = 62,//phiếu chi
            NAP_QUY = 46,//NẠP QUỸ
            QL_KHACH_HANG = 1008,//Quản lý khách hàng
            PHIEU_THU = 57,//quản lý phiếu thu
            YEU_CAU_XUAT_HOA_DON = 2008,//quản lý phiếu thu
            CHUONG_TRINH = 90,//quản chương trình qc
                              //CHUONG_TRINH = 82,//quản chương trình pro

        }
        public enum Payment_Request_Status 
        {
            CHUA_DUYET = 0,
            DA_DUYET = 1,
            BI_TU_CHOI = 2,
            LUU_NHAP = 3
        }
        public enum NoteType
        {
            ORDER = 1,
            ORDER_ITEM = 2
        }
       
        public enum Chart_Revenu_Type
        {
            Week = 1,
            Month = 2,
        }
        public enum Chart_Label_Type
        {
            Today = 1,
            Yesterday = 2,
            Week = 3,
            Month = 4,
        }
        public enum Chart_Type_Label
        {
            Revenu = 1,
            Quantity = 2,
        }
        public enum Month
        {
            Monday = DayOfWeek.Monday,
        }
    }
}
