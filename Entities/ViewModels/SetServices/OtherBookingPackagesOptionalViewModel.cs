using Entities.Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace Entities.ViewModels.SetServices
{
    public class OtherBookingPackagesOptionalViewModel : OtherBookingPackagesOptional
    {
        public Supplier supplier { get; set; }
        public decimal TotalAmountPay { get; set; }
        public string SupplierName { get; set; }
        public string Email { get; set; }
        public string Phone { get; set; }
        public double TotalContractPay { get; set; }
    }
}
