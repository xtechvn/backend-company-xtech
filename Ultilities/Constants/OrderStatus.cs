using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ultilities.Constants
{
    // Trạng thái đơn
    public enum OrderStatus
    {
        /// <summary>
        /// Mặc định trạng thái đơn khi được khởi tạo
        /// </summary>
        [Description("Tạo mới")]
        New = 0,

        [Description("Nhận triển khai")]
        Received_for_implementation = 1,

        [Description("Điều hành duyệt")]
        Approved_by_management = 2,

        [Description("Điều hành từ chối")]
        Rejected_by_management = 3,

        [Description("Kế toán duyệt")]
        Approved_by_accounting = 4,

        [Description("Kế toán từ chối")]
        Rejected_by_accounting = 5,

        [Description("Hoàn thành")]
        Completed = 6,

        [Description("Hủy")]
        Canceled = 7,

        [Description("Đơn rác")]
        Draft = 8
    }

    // Trạng thái đơn
    public enum OrderĐebtStatus
    {
        /// <summary>
        /// Đã gạch nợ đủ cho đơn hàng
        /// </summary>
        [Description("Gạch nợ đủ")]
        PAID_ENOUGH = 1,
        /// <summary>
        /// Chưa đã gạch nợ đủ cho đơn hàng
        /// </summary>
        [Description("Gạch nợ chưa đủ")]
        PAID_NOT_ENOUGH = 2,

    }
}
