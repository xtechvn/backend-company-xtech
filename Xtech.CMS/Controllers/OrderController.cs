using DocumentFormat.OpenXml.Office2010.Excel;
using Entities.Models;
using Entities.OrderDetail;
using Entities.ViewModels;
using Entities.ViewModels.OtherBooking;
using Entities.ViewModels.OtherBookingPackage;
using Microsoft.AspNetCore.Mvc;
using Nest;
using Repositories.IRepositories;
using Repositories.Repositories;
using System.Security.Claims;
using Ultilities.Constants;
using Utilities;

namespace Xtech.CMS.Controllers
{
    public class OrderController : Controller
    {
        private readonly IAllCodeRepository _allCodeRepository;
        private readonly IOrderRepository _orderRepository;
        private readonly IInvoiceRequestRepository _invoiceRequestRepository;
        private readonly IOtherBookingRepository _otherBookingRepository;
        private readonly IClientRepository _clientRepository;
        private readonly IUserRepository _userRepository;
        private readonly IContractPayRepository _contractPayRepository;
        private readonly IPaymentRequestRepository _paymentRequestRepository;
        private readonly IOtherBookingPackageRepository _otherBookingPackageRepository;
        public OrderController(IAllCodeRepository allCodeRepository,
            IInvoiceRequestRepository invoiceRequestRepository,
            IOrderRepository orderRepository, 
            IClientRepository clientRepository, 
            IUserRepository userRepositor, 
            IOtherBookingRepository otherBookingRepository,
            IOtherBookingPackageRepository otherBookingPackageRepository,
            IContractPayRepository contractPayRepository, IPaymentRequestRepository paymentRequestRepository)
        {
            _paymentRequestRepository = paymentRequestRepository;
            _contractPayRepository = contractPayRepository;
            _invoiceRequestRepository = invoiceRequestRepository;
            _otherBookingRepository = otherBookingRepository;
            _userRepository = userRepositor;
            _allCodeRepository = allCodeRepository;
            _orderRepository = orderRepository;
            _clientRepository = clientRepository;
            _otherBookingPackageRepository = otherBookingPackageRepository;

        }

        public IActionResult Index()
        {
            try
            {
                var orderStatus = _allCodeRepository.GetListByType("ORDER_STATUS");
                var PAYMENT_STATUS = _allCodeRepository.GetListByType("PAYMENT_STATUS");
                var PAYMENT_TYPE = _allCodeRepository.GetListByType("PAYMENT_TYPE");
                var SERVICE_TYPE = _allCodeRepository.GetListByType("SERVICE_TYPE");
                ViewBag.Order_Status = orderStatus;
                ViewBag.PAYMENT_STATUS = PAYMENT_STATUS;
                ViewBag.PAYMENT_TYPE = PAYMENT_TYPE;
                ViewBag.SERVICE_TYPE = SERVICE_TYPE;
            }
            catch (Exception ex)
            {
                LogHelper.InsertLogTelegram("Index - OrderController: " + ex.ToString());
                return Content("");
            }
            return View();
        }

        public async Task<IActionResult> Search(OrderViewSearchModel searchModel)
        {

            try
            {
                var all_amount = await _orderRepository.GetTotalAmountByPaymentStatus(null);
                var unpaid_amount = await _orderRepository.GetTotalAmountByPaymentStatus((int)PaymentStatus.UNPAID);
                var paid_amount = await _orderRepository.GetTotalAmountByPaymentStatus((int)PaymentStatus.PAID);
                var paid_not_enough = await _orderRepository.GetTotalAmountByPaymentStatus((int)PaymentStatus.PAID_NOT_ENOUGH);
                var model = new GenericViewModel<OrderViewModel>();
                model = await _orderRepository.GetList(searchModel);
                ViewBag.Amounts = new TotalValueOrder
                {
                    TotalAmmount = all_amount.ToString("N0"),
                    TotalPaid = paid_amount.ToString("N0"),
                    TotalUnPaid = unpaid_amount.ToString("N0"),
                    TotalPaidNotEnough = paid_not_enough.ToString("N0")
                };
                return PartialView(model);
            }
            catch (Exception ex)
            {
                LogHelper.InsertLogTelegram("Search - OrderController: " + ex);
            }

            return PartialView();
        }

        public async Task<IActionResult> Packages(int orderId)
        {
            var data = new List<OtherBookingViewModel>();
            try
            {
                data = await _otherBookingRepository.GetAllOtherBookingByOrderId(orderId);
                var dataOrder = await _orderRepository.GetOrderDetailByOrderId(orderId);
                ViewBag.dataOrder = dataOrder;
                return PartialView(data);
            }
            catch (Exception ex)
            {
                LogHelper.InsertLogTelegram("Packages-OrderController" + ex.ToString());
                return PartialView();
            }
        }

        public async Task<IActionResult> PersonInCharge(int SalerId, string SalerGroupId)
        {
            try
            {
                if (SalerId != null)
                {
                    var SalerGroup = await _userRepository.GetClientDetailAsync(SalerId);
                    ViewBag.Saler = SalerGroup;
                }
                List<User> List_SalerGroup = new List<User>();
                if (SalerGroupId != null && SalerGroupId != "")
                {
                    var list_SalerGroupId = Array.ConvertAll(SalerGroupId.ToString().Split(','), s => (s).ToString());

                    foreach (var item in list_SalerGroupId)
                    {
                        long id = Convert.ToInt32(item);
                        var SalerGroup = await _userRepository.GetClientDetailAsync(id);
                        if (SalerGroup != null)
                        {
                            var ClientName = SalerGroup.FullName.ToString();
                            List_SalerGroup.Add(SalerGroup);
                        }
                        ViewBag.SalerGroup = List_SalerGroup;
                    }

                }
                return PartialView();
            }
            catch (Exception ex)
            {
                LogHelper.InsertLogTelegram("PersonInCharge-OrderController" + ex.ToString());
                return PartialView();
            }
        }
        public async Task<IActionResult> ContractPay(int orderId)
        {
            try
            {
                if (orderId != 0)
                {

                    var dataOrder = await _orderRepository.GetOrderDetailByOrderId(orderId);
                    if (dataOrder != null)
                    {
                        var data = await _contractPayRepository.GetContractPayByOrderId(Convert.ToInt32(dataOrder.OrderId));
                        if (data != null)
                        {
                            ViewBag.listPayment = data;
                            ViewBag.paymentAmount = data.Sum(s => s.AmountPay);
                            return PartialView(data);
                        }
                    }
                }

                return PartialView();
            }
            catch (Exception ex)
            {
                LogHelper.InsertLogTelegram("ContractPay - OrderController" + ex.ToString());
                return PartialView();
            }

        }
        public async Task<IActionResult> BillVAT(int orderId)
        {
            try
            {
                if (orderId != 0)
                {

                  /*var dataOrder = await _orderRepository.GetOrderDetailByOrderId(orderId);
                    if (dataOrder != null)
                    {*/
                        var data = await _invoiceRequestRepository.GetListInvoiceRequestByOrderId(Convert.ToInt32(orderId));
                        if (data != null)
                        {
                            return PartialView(data);
                        }
                  /*}*/
                }
                return PartialView();
            }
            catch (Exception ex)
            {
                LogHelper.InsertLogTelegram("BillVAT-OrderController" + ex.ToString());
                return PartialView();
            }
        }

        public async Task<IActionResult> AddOtherService(int? BookingId,int OrderId) 
        {
            try
            {
                List<User> ListUser = new List<User>();
                List<OtherBookingPackageViewModel> data = new();
                OtherBookingViewModel OtherBooking = new();
                OrderDetailViewModel orderDetail = new OrderDetailViewModel();
                if (BookingId != null) 
                {
                    data = await _otherBookingPackageRepository.GetListOtherBookingPackageByBookingId(BookingId);
                    OtherBooking = await _otherBookingRepository.GetOtherBookingById(BookingId);
                }
                orderDetail = await _orderRepository.GetOrderDetailByOrderId(OrderId);
                var ServiceType = _allCodeRepository.GetListByType("SERVICE_TYPE");
                ListUser = _userRepository.GetAll();

                ViewBag.OtherBooking = OtherBooking;
                ViewBag.ListUser = ListUser;
                ViewBag.BookingId = BookingId;
                ViewBag.OrderId = OrderId;
                ViewBag.OrderDetail = orderDetail;
                ViewBag.ServiceType = ServiceType;
                return PartialView(data);
            }
            catch (Exception ex)
            {
                LogHelper.InsertLogTelegram("AddOtherService-OrderController" + ex.ToString());
            }
            return PartialView();
        }

        public async Task<IActionResult> SubmitChange(List<OtherBookingPackageSubmitModel> lstUpdate,List<int> lstDelete, OtherBookingSubmitModel OtherBooking) 
        {
            try
            {
                int OrderId = (int)OtherBooking.OrderId;
                OrderDetailViewModel orderDetail = new OrderDetailViewModel();
                orderDetail = await _orderRepository.GetOrderDetailByOrderId(OrderId);
                var IdBooking = 0;
                if (orderDetail.Status == (int)OrderStatus.New)//Đơn mới được thêm,sửa xóa
                {
                    IdBooking = await _otherBookingRepository.SetUpOtherBooking(OtherBooking);
                    foreach (var item in lstUpdate)
                    {
                        item.BookingId = IdBooking;
                        await _otherBookingPackageRepository.SetUpOtherBookingPackage(item);
                    }
                    foreach (var item in lstDelete)
                    {
                        await _otherBookingPackageRepository.DeleteOtherBookingPackage(item);
                    }
                }
                if (orderDetail.Status == (int)OrderStatus.Rejected_by_management)//đơn điều hành từ chối chỉ được sửa
                {
                    IdBooking = await _otherBookingRepository.SetUpOtherBooking(OtherBooking);
                    foreach (var item in lstUpdate)
                    {
                        item.BookingId = IdBooking;
                        await _otherBookingPackageRepository.SetUpOtherBookingPackage(item);
                    }
                }

                Order order = new Order()
                {
                    OrderId = OrderId,
                    UserUpdateId = Convert.ToInt32(HttpContext.User.FindFirst(ClaimTypes.NameIdentifier).Value)
                };

                await _orderRepository.UpdateOrder(order);
                await _orderRepository.UpdateAmountOrder(OrderId);
                return Ok(new
                {
                    status = (int)ResponseType.SUCCESS,
                    IdBooking = IdBooking
                });
            }
            catch (Exception ex)
            {
                LogHelper.InsertLogTelegram("DeleteOtherBookingPackage - OrderController" + ex.ToString());
                return Ok(new
                {
                    status = (int)ResponseType.ERROR
                });
            }
        }

        public async Task<IActionResult> OrderDetail(int orderId)
        {
            try
            {
                OrderViewSearchModel searchModel = new OrderViewSearchModel() 
                {
                    OrderId = orderId,
                    PageIndex = 1,
                    pageSize = 1
                };
                ViewBag.OrderId = orderId;
                var model = new GenericViewModel<OrderViewModel>();
                model = await _orderRepository.GetList(searchModel);
                if (model != null) 
                {
                    var client = await _clientRepository.GetClientDetailByClientId((int)model.ListData[0].ClientId);
                    ViewBag.Client = client;
                    ViewBag.SalerId = model.ListData[0].SalerId;
                    ViewBag.SalerGroupId = model.ListData[0].SalerGroupId;
                }
                return View(model);
            }
            catch (Exception ex)
            {
                LogHelper.InsertLogTelegram("Index - OrderController: " + ex.ToString());
            }

            return View();
        }

        public async Task<IActionResult> UpdateOrder(Order model)
        {
            try
            {
                model.UserUpdateId = Convert.ToInt32(HttpContext.User.FindFirst(ClaimTypes.NameIdentifier).Value);
                await _orderRepository.UpdateOrder(model);
                return Ok(new
                {
                    status = (int)ResponseType.SUCCESS
                });
            }
            catch (Exception ex)
            {
                LogHelper.InsertLogTelegram("UpdateOrder - OrderController: " + ex.ToString());
            }

            return Ok();
        }
    }
}
