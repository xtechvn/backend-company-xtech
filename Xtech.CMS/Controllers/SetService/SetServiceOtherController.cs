
using Entities.Models;
using Entities.ViewModels;
using Entities.ViewModels.SetServices;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;

using Newtonsoft.Json;
using Repositories.IRepositories;
using Repositories.Repositories;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Ultilities.Constants;
using Utilities;
using Utilities.Contants;
using WEB.CMS.Customize;
using Xtech.CMS.Services;
using static Utilities.DepositHistoryConstant;

namespace WEB.Adavigo.CMS.Controllers.SetService.Other
{

    [CustomAuthorize]
    public class SetServiceController : Controller
    {
        private readonly IConfiguration _configuration;
        private readonly IOrderRepositor _orderRepository;
        private readonly IOrderRepository _orderRepository2;
        //private readonly ISupplierRepository _supplierRepository;
        private APIService apiService;

        private readonly IAllCodeRepository _allCodeRepository;
        private readonly IUserRepository _userRepository;

        private readonly IClientRepository _clientRepository;

        private readonly IPaymentRequestRepository _paymentRequestRepository;
        //private readonly ISupplierRepository _supplierRepository;
        private readonly IOtherBookingRepository _otherBookingRepository;
        private ManagementUser _ManagementUser;
        //private APIService apiService;
        private readonly IWebHostEnvironment _WebHostEnvironment;
        //private readonly IHotelBookingCodeRepository _hotelBookingCodeRepository;
        private readonly IContractPayRepository _contractPayRepository;

        public SetServiceController(IConfiguration configuration, IOrderRepositor orderRepository, IAllCodeRepository allcodeRepository,
           IUserRepository userRepository, IOtherBookingRepository otherBookingRepository,
           IPaymentRequestRepository paymentRequestRepository, IOrderRepository orderRepository2, IWebHostEnvironment WebHostEnvironment,
           ManagementUser managementUser, IClientRepository clientRepository, IContractPayRepository contractPayRepository)
        {

            _configuration = configuration;
            _orderRepository = orderRepository;

            _clientRepository = clientRepository;

            _allCodeRepository = allcodeRepository;
           
            _userRepository = userRepository;
          
            _paymentRequestRepository = paymentRequestRepository;
            //_supplierRepository = supplierRepository;
            _orderRepository2 = orderRepository2;
            _otherBookingRepository = otherBookingRepository;
            _ManagementUser = managementUser;
            //apiService = new APIService(configuration, userRepository);
            _WebHostEnvironment = WebHostEnvironment;
            //_hotelBookingCodeRepository = hotelBookingCodeRepository;
            _contractPayRepository = contractPayRepository;
            apiService = new APIService(configuration, userRepository);
        }
        public IActionResult Others()
        {
            try
            {
                var allcode_list = _allCodeRepository.GetListByType(AllCodeType.BOOKING_HOTEL_ROOM_STATUS);
                ViewBag.status = allcode_list.Where(x => x.CodeValue != (int)ServiceStatus.New).ToList();
            }
            catch (Exception ex)
            {
                LogHelper.InsertLogTelegram("Others - SetServiceOtherController: " + ex);

            }
            return View();
        }
        public async Task<IActionResult> OtherSearch(SearchFlyBookingViewModel searchModel)
        {
            ViewBag.Model = new GenericViewModel<OtherBookingSearchViewModel>();

            try
            {
                if (searchModel == null) searchModel = new SearchFlyBookingViewModel();
                if (searchModel.pageSize <= 0) searchModel.pageSize = 30;
                if (searchModel.PageIndex <= 0) searchModel.PageIndex = 1;
                if (searchModel.StatusBooking == null || searchModel.StatusBooking.Trim() == "")
                {
                    var s = _allCodeRepository.GetListByType(AllCodeType.BOOKING_HOTEL_ROOM_STATUS).Where(x => x.CodeValue != (int)ServiceStatus.New).Select(x => x.CodeValue);
                    searchModel.StatusBooking = string.Join(",", s);
                }
                if (searchModel.StartDateFrom != null && (searchModel.StartDateTo == null || searchModel.StartDateTo < searchModel.StartDateFrom)) searchModel.StartDateTo = searchModel.StartDateFrom;
                if (searchModel.EndDateFrom != null && (searchModel.EndDateTo == null || searchModel.EndDateTo < searchModel.EndDateFrom)) searchModel.EndDateTo = searchModel.EndDateFrom;
                if (searchModel.StartDateTo < searchModel.EndDateFrom && searchModel.StartDateTo < searchModel.EndDateTo)
                {
                    searchModel.StartDateTo = null;
                    searchModel.EndDateFrom = null;
                }
                var current_user = _ManagementUser.GetCurrentUser();
                if (current_user != null)
                {
                    if (current_user.Role != "")
                    {
                        var list = current_user.Role.Split(',');
                        foreach (var item in list)
                        {
                            bool is_admin_or_department = false;
                            switch (Convert.ToInt32(item))
                            {

                                case (int)RoleType.TPKS:
                                case (int)RoleType.TPTour:
                                case (int)RoleType.TPVe:
                                case (int)RoleType.DHPQ:
                                case (int)RoleType.DHTour:
                                case (int)RoleType.DHVe:
                                case (int)RoleType.DHKS:
                                case (int)RoleType.SaleKd:
                                case (int)RoleType.SaleTour:
                                    {
                                        searchModel.SalerPermission += current_user.UserUnderList;
                                    }
                                    break;
                                case (int)RoleType.Admin:
                                case (int)RoleType.KT:
                                case (int)RoleType.GDHN:
                                case (int)RoleType.GDHPQ:
                                case (int)RoleType.GD:
                                case (int)RoleType.TPDHTour:
                                case (int)RoleType.TPDHKS:
                                case (int)RoleType.TPDHVe:
                                    {
                                        searchModel.SalerPermission = null;
                                        is_admin_or_department = true;
                                    }
                                    break;
                            }
                            if (is_admin_or_department) break;
                        }

                        ViewBag.Model = await _otherBookingRepository.GetPagingList(searchModel, searchModel.PageIndex, searchModel.pageSize);
                    }
                }

            }
            catch (Exception ex)
            {
                LogHelper.InsertLogTelegram("OtherSearch - SetServiceOtherController: " + ex);
            }

            return View();
        }
        [HttpPost]
        public async Task<IActionResult> ListOrder(long id)
        {
            var data = new List<OrderDetailViewModel>();
            ViewBag.Client = new Client();
            try
            {
                data = await _orderRepository.GetDetailOrderByOrderId(Convert.ToInt32(id));
                {
                    ViewBag.Client = await _clientRepository.GetClientDetailByClientId(data[0].ClientId);

                }
            }
            catch (Exception ex)
            {
                LogHelper.InsertLogTelegram("ListOrder - SetServiceController: " + ex);
            }

            return PartialView(data);
        }
        public async Task<IActionResult> OtherDetail(long id)
         {
            ViewBag.user = new User();
            ViewBag.Booking = new OtherBooking();
            ViewBag.UserCreated = new User();
            ViewBag.UserUpdated = new User();
            ViewBag.Saler = new User();
            ViewBag.Operator = new User();
            ViewBag.OrderAmount = 0;
            ViewBag.ClientId = 0;
            ViewBag.RefundAmount = 0;
            try
            {
                var amount = _contractPayRepository.GetTotalAmountContractPayByServiceId(id.ToString(), (int)ServicesType.Other, (int)CONTRACT_PAY_TYPE.THU_TIEN_NCC_HOAN_TRA);
                ViewBag.amountNcc = amount;
                var allcode_list = _allCodeRepository.GetListByType(AllCodeType.BOOKING_HOTEL_ROOM_STATUS);
                var other_detail = await _otherBookingRepository.GetOtherBookingById2(id);
                if (other_detail != null && other_detail.Id > 0)
                {
                    ViewBag.StatusName = allcode_list.First(x => x.CodeValue == other_detail.Status).Description;
                    ViewBag.user = Convert.ToInt32(HttpContext.User.FindFirst(ClaimTypes.NameIdentifier).Value);
                    ViewBag.Booking = other_detail;
                    //ViewBag.UserCreated = await _userRepository.GetById((long)other_detail.CreatedBy);
                    var order = _orderRepository.GetByOrderId(other_detail.OrderId);
                    if (order != null && order.OrderId > 0)
                    {
                        //ViewBag.RefundAmount = (double)(order.Refund == null ? 0 : order.Refund);
                        ViewBag.OrderAmount = order.Amount;
                        ViewBag.Saler = await _userRepository.GetById((long)order.SalerId);
                        ViewBag.ClientId = order.ClientId ?? 0;

                    }
                    if (other_detail.UpdatedBy != null)
                    {
                        ViewBag.UserUpdated = await _userRepository.GetById((long)other_detail.UpdatedBy);

                    }
                    ViewBag.Operator = await _userRepository.GetById((long)other_detail.OperatorId);
                    ViewBag.AllowToFinishPayment = false;
                    if (other_detail != null && other_detail.Id > 0)
                    {
                        var max_date = other_detail.EndDate;
                        if (max_date < DateTime.Now)
                        {
                            ViewBag.AllowToFinishPayment = true;

                        }
                    }
                    //var data_refund = _paymentRequestRepository.GetRequestByClientId((long)order.ClientId, order.OrderId);
                    //if (data_refund != null && data_refund.Count > 0)
                    //{
                    //    ViewBag.RefundAmount = data_refund.Where(n => (n.Status == (int)(PAYMENT_REQUEST_STATUS.DA_CHI)) && !string.IsNullOrEmpty(n.PaymentVoucherCode)).Sum(n => n.Amount);

                    //}
                    return View();

                }
                else
                {
                    return RedirectToAction("Error", "Home");
                }
            }
            catch (Exception ex)
            {
                LogHelper.InsertLogTelegram("FlyDetail - SetServiceFlyBookingController: " + ex);
                return RedirectToAction("Error", "Home");
            }

        }
        public async Task<IActionResult> HotelPopupDetail(int id, int type, int orderid, string groupbookingid)
        {

            try
            {
                ViewBag.id = id;
                ViewBag.type = type;
                ViewBag.orderid = orderid;
                ViewBag.group_booking_id = groupbookingid;
                return View();
            }
            catch (Exception ex)
            {
                LogHelper.InsertLogTelegram("HotelPopupDetail - ContractController: " + ex);
                return View();
            }
        }
        [HttpPost]
        public async Task<IActionResult> OtherDetailBookingSale(long id)
        {
            try
            {
                var allcode_list = _allCodeRepository.GetListByType(AllCodeType.SERVICE_TYPE_OTHER);

                var other_detail = await _otherBookingRepository.GetOtherBookingById2(id);
                if (other_detail != null && other_detail.Id > 0)
                {
                    ViewBag.Service = allcode_list.FirstOrDefault(x => x.CodeValue == other_detail.ServiceType);
                    ViewBag.Booking = other_detail;
                    ViewBag.ExtraList = await _otherBookingRepository.GetOtherBookingPackagesByBookingId(other_detail.Id);
                    return View();
                }
                else
                {
                    return RedirectToAction("Error", "Home");
                }
            }
            catch (Exception ex)
            {
                LogHelper.InsertLogTelegram("OtherDetailBookingSale - SetServiceFlyBookingController: " + ex);
                return RedirectToAction("Error", "Home");
            }
        }
        [HttpPost]
        public async Task<IActionResult> OtherDetailBookingOperator(long id)
        {
            try
            {
                var allcode_list = _allCodeRepository.GetListByType(AllCodeType.SERVICE_TYPE_OTHER);

                var other_detail = await _otherBookingRepository.GetOtherBookingById2(id);
                ViewBag.ListPackagesOptional = new List<OtherBookingPackagesOptionalViewModel>();
                if (other_detail != null && other_detail.Id > 0)
                {
                    ViewBag.Service = allcode_list.FirstOrDefault(x => x.CodeValue == other_detail.ServiceType);
                    ViewBag.Booking = other_detail;
                    var optional_list = await _otherBookingRepository.GetOtherBookingPackagesOptionalByBookingId(other_detail.Id);
                    if (optional_list != null && optional_list.Count > 0)
                    {
                        var list_optional = new List<OtherBookingPackagesOptionalViewModel>();
                        foreach (var item in optional_list)
                        {
                            var model = JsonConvert.DeserializeObject<OtherBookingPackagesOptionalViewModel>(JsonConvert.SerializeObject(item));
                            if (model.SuplierId > 0)
                            {
                                model.supplier = null;
                            }
                            list_optional.Add(model);
                        }
                        ViewBag.ListPackagesOptional = list_optional;
                    }
                    return View();
                }
                else
                {
                    return RedirectToAction("Error", "Home");
                }
            }
            catch (Exception ex)
            {
                LogHelper.InsertLogTelegram("OtherDetailBookingOperator - SetServiceFlyBookingController: " + ex);
                return RedirectToAction("Error", "Home");
            }
        }
        public async Task<IActionResult> OtherDetailBookingChangeOperator(string operator_name, long booking_id)
        {

            try
            {
                ViewBag.Name = operator_name;
                ViewBag.Id = booking_id;

            }
            catch
            {

            }
            return View();
        }
        [HttpPost]
        public async Task<IActionResult> SummitOtherDetailChangeOperator(long booking_id, int user_id)
        {

            try
            {
                if (booking_id <= 0 || user_id <= 0)
                {
                    return Ok(new
                    {
                        status = (int)ResponseType.FAILED,
                        msg = "Dữ liệu gửi lên không chính xác, vui lòng kiểm tra lại"
                    });
                }
                int _UserId = 0;
                if (HttpContext.User.FindFirst(ClaimTypes.NameIdentifier) != null)
                {
                    _UserId = Convert.ToInt32(HttpContext.User.FindFirst(ClaimTypes.NameIdentifier).Value);
                }
                var success = await _otherBookingRepository.UpdateServiceOperator(booking_id, user_id, _UserId);

                return Ok(new
                {
                    status = (int)ResponseType.SUCCESS,
                    msg = "Đổi điều hành viên thành công"
                });

            }
            catch
            {
                return Ok(new
                {
                    status = (int)ResponseType.ERROR,
                    msg = "Đổi điều hành viên thất bại, vui lòng liên hệ IT"
                });
            }
        }
        [HttpPost]
        public async Task<IActionResult> UpdateServiceStatus(int id, int orderid, int type, string Note, string groupbookingid)
        {
            var status = (int)ResponseType.SUCCESS;
            var msg = "Không thành công";
            try
            {
                if (type != 0)
                {
                    var order = _orderRepository.GetByOrderId(orderid);
                    string link = "/Order/" + orderid;
                    var current_user = _ManagementUser.GetCurrentUser();
                    var CreatedatedBy = Convert.ToInt32(HttpContext.User.FindFirst(ClaimTypes.NameIdentifier).Value);
                    var model = new ServiceDeclines();
                    model.OrderId = orderid;
                    model.CreatedBy = CreatedatedBy;
                    model.Type = type;
                    if (type == (int)ServiceType.PRODUCT_FLY_TICKET)
                    {
                        model.ServiceId = groupbookingid;
                    }
                    else
                    {
                        model.ServiceId = id.ToString();
                    }

                    model.Note = Note;
                    //await _hotelBookingRepositories.InsertServiceDeclines(model);
                    long UpdatedBy = int.Parse(HttpContext.User.FindFirst(ClaimTypes.NameIdentifier).Value);
                    long UserVerify = 0;
                    switch (type)
                    {
                      
                        case (int)ServiceType.Other:
                            {
                                var data = await _otherBookingRepository.UpdateServiceStatus((int)ServiceStatus.Decline, id, (int)UpdatedBy);
                                if (data != 0 && data > 0)
                                {
                                    var otherBookings = await _otherBookingRepository.GetOtherBookingById2(id);
                                    var dataOrder2 = await _orderRepository.ProductServiceName(orderid.ToString());
                                    dataOrder2 = dataOrder2.Where(s => s.Status != (int)ServiceStatus.Cancel).ToList();
                                    var listdata = dataOrder2.Where(s => s.Status == (int)ServiceStatus.Payment ? s.Status == (int)ServiceStatus.Payment : s.Status == (int)ServiceStatus.Decline).ToList();
                                    var listdata2 = dataOrder2.Where(s => s.Status == (int)ServiceStatus.Decline).ToList();
                                    if (listdata2.Count == dataOrder2.Count && listdata.Count == dataOrder2.Count)
                                    {
                                        await _orderRepository.UpdateOrderStatus(orderid, (int)OrderStatus.Rejected_by_management, UpdatedBy, UserVerify);
                                    }
                                    apiService.SendMessage(CreatedatedBy.ToString(), ((int)ModuleType.DICH_VU).ToString(), ((int)ActionType.TU_CHOI).ToString(), order.OrderNo, link, current_user.Role, otherBookings.ServiceCode);
                                    status = (int)ResponseType.SUCCESS;
                                    msg = "Từ chối thành công";
                                }
                                else
                                {
                                    status = (int)ResponseType.ERROR;
                                    msg = "Từ chối không thành công";
                                }


                            }
                            break;
                       

                    }


                }
            }
            catch (Exception ex)
            {
                LogHelper.InsertLogTelegram("UpdateServiceStatus - SetServiceController: " + ex);
                status = (int)ResponseType.ERROR;
                msg = "Đã xảy ra lỗi, vui lòng liên hệ IT";
            }
            return Ok(new
            {
                status = status,
                msg = msg
            });
        }
        [HttpPost]
        public async Task<IActionResult> UpdateOtherOperatorOrderPrice(List<OtherBookingPackagesOptional> data)
        {

            try
            {
                 if (data == null || data.Count <= 0)
                {
                    return Ok(new
                    {
                        status = (int)ResponseType.FAILED,
                        msg = "Dữ liệu gửi lên không chính xác, vui lòng kiểm tra lại"
                    });
                }
                var other_booking = await _otherBookingRepository.GetOtherBookingById2(data[0].BookingId);
                double amount = 0;
                double price = 0;
                double profit = 0;
                if (other_booking != null && other_booking.Id > 0)
                {
                    int _UserId = 0;
                    if (HttpContext.User.FindFirst(ClaimTypes.NameIdentifier) != null)
                    {
                        _UserId = Convert.ToInt32(HttpContext.User.FindFirst(ClaimTypes.NameIdentifier).Value);
                    }
                    var id = await _otherBookingRepository.UpdateOtherBookingOptional(data, data[0].BookingId, _UserId);
                    other_booking = await _otherBookingRepository.GetOtherBookingById2(data[0].BookingId);
                    amount = other_booking.Amount;
                    price = (other_booking.Price != null ? (double)other_booking.Price : 0);
                    if (price <= 0) price = 0;
                    profit = amount - price - (other_booking.Commission == null ? 0 : (double)other_booking.Commission) - (other_booking.OthersAmount == null ? 0 : (double)other_booking.OthersAmount);

                    #region Update Order Amount:
                    await _orderRepository2.UpdateOrderDetail(other_booking.OrderId, _UserId);
                    #endregion
                }
                return Ok(new
                {
                    status = (int)ResponseType.SUCCESS,
                    msg = "Cập nhật giá đặt dịch vụ thành công",
                    amount = price,
                    profit = profit
                });
            }
            catch (Exception ex)
            {
                LogHelper.InsertLogTelegram("UpdateOperatorOrderPrice - OrderManualController: " + ex.ToString());

            }
            return Ok(new
            {
                status = (int)ResponseType.FAILED,
                msg = "Cập nhật giá đặt dịch vụ thất bại, vui lòng liên hệ IT"
            });

        }

        //[HttpPost]
        //public async Task<IActionResult> OtherExportExcel(SearchFlyBookingViewModel searchModel)
        //{
        //    try
        //    {
        //        var current_user = _ManagementUser.GetCurrentUser();
        //        string _FileName = "Danh sách đặt dịch vụ khác(" + current_user.Id + ").xlsx";
        //        string _UploadFolder = @"Template\Export";
        //        string _UploadDirectory = Path.Combine(_WebHostEnvironment.WebRootPath, _UploadFolder);

        //        if (!Directory.Exists(_UploadDirectory))
        //        {
        //            Directory.CreateDirectory(_UploadDirectory);
        //        }
        //        //delete all file in folder before export
        //        try
        //        {
        //            System.IO.DirectoryInfo di = new DirectoryInfo(_UploadDirectory);
        //            foreach (FileInfo file in di.GetFiles())
        //            {
        //                file.Delete();
        //            }
        //        }
        //        catch
        //        {
        //        }


        //        string FilePath = Path.Combine(_UploadDirectory, _FileName);

        //        var rsPath = await _otherBookingRepository.ExportDeposit(searchModel, FilePath);

        //        if (!string.IsNullOrEmpty(rsPath))
        //        {
        //            return new JsonResult(new
        //            {
        //                isSuccess = true,
        //                message = "Xuất dữ liệu thành công",
        //                path = "/" + _UploadFolder + "/" + _FileName
        //            });
        //        }
        //        else
        //        {
        //            return new JsonResult(new
        //            {
        //                isSuccess = false,
        //                message = "Xuất dữ liệu thất bại"
        //            });
        //        }
        //    }
        //    catch (Exception ex)
        //    {
        //        LogHelper.InsertLogTelegram("ExportExcel - FundingController: " + ex);
        //        return new JsonResult(new
        //        {
        //            isSuccess = false,
        //            message = ex.Message.ToString()
        //        });
        //    }
        //}

        [HttpPost]
        public async Task<IActionResult> GrantOtherServiceOrderPermission(long booking_id)
        {
            try
            {
                var current_user = _ManagementUser.GetCurrentUser();
                if (booking_id <= 0)
                {
                    return Ok(new
                    {
                        status = (int)ResponseType.FAILED,
                        msg = "Nhận đặt dịch vụ thất bại, vui lòng tải lại trang và thử lại"
                    });
                }
                int _UserId = 0;
                if (HttpContext.User.FindFirst(ClaimTypes.NameIdentifier) != null)
                {
                    _UserId = Convert.ToInt32(HttpContext.User.FindFirst(ClaimTypes.NameIdentifier).Value);
                }
                var id = await _otherBookingRepository.UpdateServiceOperator2(booking_id, _UserId);
                if (id > 0)
                {
                    return Ok(new
                    {
                        status = (int)ResponseType.SUCCESS,
                        msg = "Nhận đặt dịch vụ thành công"
                    });
                }

            }
            catch (Exception ex)
            {
                LogHelper.InsertLogTelegram("GrantOrderPermission - OrderManualController: " + ex.ToString());

            }
            return Ok(new
            {
                status = (int)ResponseType.ERROR,
                msg = "Nhận đặt dịch vụ thất bại, vui lòng liên hệ IT"
            });
        }
        [HttpPost]
        public async Task<IActionResult> OtherServiceSendServiceCode(long booking_id)
        {
            try
            {
                var current_user = _ManagementUser.GetCurrentUser();
                if (booking_id <= 0)
                {
                    return Ok(new
                    {
                        status = (int)ResponseType.FAILED,
                        msg = "Dữ liệu gửi lên không chính xác, vui lòng kiểm tra lại"
                    });
                }
                int _UserId = 0;
                if (HttpContext.User.FindFirst(ClaimTypes.NameIdentifier) != null)
                {
                    _UserId = Convert.ToInt32(HttpContext.User.FindFirst(ClaimTypes.NameIdentifier).Value);
                }
                
                    var id = await _otherBookingRepository.UpdateServiceStatus((int)ServiceStatus.ServeCode, booking_id, _UserId);
                    if (id > 0)
                    {
                        var detail = await _otherBookingRepository.GetOtherBookingById2(booking_id);
                        var order = _orderRepository.GetByOrderId(detail.OrderId);
                        string link = "/Order/" + detail.OrderId;
                        apiService.SendMessage(_UserId.ToString(), ((int)ModuleType.DICH_VU).ToString(), ((int)ActionType.TRA_CODE).ToString(), order.OrderNo, link, current_user.Role, detail.ServiceCode);
                        return Ok(new
                        {
                            status = (int)ResponseType.SUCCESS,
                            msg = "Gửi Code dịch vụ thành công"
                        });
                    }
                
               
            }
            catch (Exception ex)
            {
                LogHelper.InsertLogTelegram("SendServiceCode - OrderManualController: " + ex.ToString());

            }
            return Ok(new
            {
                status = (int)ResponseType.ERROR,
                msg = "Cập nhật trạng thái đặt dịch vụ thất bại, vui lòng liên hệ IT"
            });
        }
        [HttpPost]
        public async Task<IActionResult> OtherServiceChangeToConfirmPaymentStatus(long booking_id)
        {
            try
            {
                var current_user = _ManagementUser.GetCurrentUser();
                if (booking_id <= 0)
                {
                    return Ok(new
                    {
                        status = (int)ResponseType.FAILED,
                        msg = "Dữ liệu gửi lên không chính xác, vui lòng kiểm tra lại"
                    });
                }
                int _UserId = 0;
                if (HttpContext.User.FindFirst(ClaimTypes.NameIdentifier) != null)
                {
                    _UserId = Convert.ToInt32(HttpContext.User.FindFirst(ClaimTypes.NameIdentifier).Value);
                }
                //var payment_request = _paymentRequestRepository.GetByServiceId(booking_id, (int)ServicesType.Other);
                //if (payment_request == null || payment_request.Count <= 0)
                //{
                //    return Ok(new
                //    {
                //        status = (int)ResponseType.FAILED,
                //        msg = "Dịch vụ chưa có yêu cầu chi nào, vui lòng bổ sung"
                //    });
                //}
                var detail = await _otherBookingRepository.GetOtherBookingById2(booking_id);
                double amount = detail.Price != null ? (double)detail.Price : (detail.Amount - detail.Profit);
                //var request_amount = Convert.ToDouble(payment_request.Sum(x => (((x.Status == (int)(PAYMENT_REQUEST_STATUS.DA_CHI) || x.Status == (int)(PAYMENT_REQUEST_STATUS.CHO_CHI) || x.IsSupplierDebt == true))) ? x.Amount : 0));
                //if (request_amount >= amount)
                //{
                    var id = await _otherBookingRepository.UpdateServiceStatus((int)ServiceStatus.Payment, booking_id, _UserId);
                    var dataOrder2 = await _orderRepository2.ProductServiceName(detail.OrderId.ToString());
                    List<int> confirm_payment_status_allow = new List<int>() { (int)ServiceStatus.Payment, (int)ServiceStatus.Cancel };
                    var is_other_than_payment = dataOrder2.Any(x => !confirm_payment_status_allow.Contains(x.Status));
                    var is_has_payment = dataOrder2.Any(x => x.Status == (int)ServiceStatus.Payment);
                    if (!is_other_than_payment && is_has_payment)
                    {
                        long UpdatedBy = int.Parse(HttpContext.User.FindFirst(ClaimTypes.NameIdentifier).Value);
                        long UserVerify = 0;
                        var UpdateOrderStatus = await _orderRepository2.UpdateOrderStatus(detail.OrderId, (int)OrderStatus.Approved_by_accounting, UpdatedBy, UserVerify);
                    }
                    if (id > 0)
                    {
                        var order = _orderRepository.GetByOrderId(detail.OrderId);
                        string link = "/Order/" + detail.OrderId;
                        apiService.SendMessage(_UserId.ToString(), ((int)ModuleType.DICH_VU).ToString(), ((int)ActionType.QUYET_TOAN).ToString(), order.OrderNo, link, current_user.Role, detail.ServiceCode);
                        //-- update order payment_status
                        await _orderRepository2.UpdateOrderDetail(detail.OrderId, _UserId);


                        return Ok(new
                        {
                            status = (int)ResponseType.SUCCESS,
                            msg = "Quyết toán dịch vụ thành công"
                        });
                    }
                //}
                //else
                //{
                //    return Ok(new
                //    {
                //        status = (int)ResponseType.FAILED,
                //        msg = "Đơn hàng chưa được thanh toán đủ"
                //    });
                //}


            }
            catch (Exception ex)
            {
                LogHelper.InsertLogTelegram("ChangeToConfirmPaymentStatus - OrderManualController: " + ex.ToString());

            }
            return Ok(new
            {
                status = (int)ResponseType.ERROR,
                msg = "Cập nhật trạng thái đặt dịch vụ thất bại, vui lòng liên hệ IT"
            });
        }
        [HttpPost]
        public async Task<IActionResult> OtherServiceCodeSuggestion(string txt_search)
        {
            List<OtherBooking> data = new List<OtherBooking>();

            try
            {
                data = await _otherBookingRepository.ServiceCodeSuggesstion(txt_search);
                return Ok(new
                {
                    status = (int)ResponseType.SUCCESS,
                    msg = "Success",
                    data = data
                });

            }
            catch
            {
            }

            return Ok(new
            {
                status = (int)ResponseType.ERROR,
                msg = "OtherServiceCodeSuggestion error on excution",
                data = data
            });
        }
    }
}
