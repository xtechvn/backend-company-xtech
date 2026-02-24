using System;
using System.Collections.Generic;
using System.Text;

namespace Entities.ViewModels.SetServices
{
    public class SearchFlyBookingViewModel
    {
		public string ServiceCode { get; set; }
		public string OrderCode { get; set; }
		public string StatusBooking { get; set; }
		public DateTime? StartDateFrom { get; set; }
		public DateTime? StartDateTo { get; set; }
		public DateTime? EndDateFrom { get; set; }
		public DateTime? EndDateTo { get; set; }
		public string UserCreate { get; set; }
		public DateTime? CreateDateFrom { get; set; }
		public DateTime? CreateDateTo { get; set; }
		public int? SalerId { get; set; } 
		public int? OperatorId { get; set; }
		public int PageIndex { get; set; }
		public int pageSize { get; set; }
		public string SalerPermission { get; set; }
		public string BookingCode { get; set; }
		public List<int>? ServiceType { get; set; }

	}
	
}
