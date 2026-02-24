using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ultilities.Constants
{
    public enum PaymentStatus
    {
        [Description("Chưa thanh toán")]
        UNPAID = 0,

        [Description("Đã thanh toán")]
        PAID = 1,

        [Description("Thanh toán thiếu")]
        PAID_NOT_ENOUGH = 2

    }
    public enum PaymentRequetStatus
    {

        Luu_Nhap = 0,
        Tu_choi = 1,
        Cho_TBP_duyet = 2,
        Cho_KTT_duyet = 3,
        Cho_chi = 4,
        Da_chi = 5,

    }
}
