using Entities.Models;
using Entities.ViewModels;
using Entities.ViewModels.InvoiceRequest;
using Microsoft.AspNetCore.Mvc;
using Nest;
using Repositories.IRepositories;
using Repositories.Repositories;
using System.Security.Claims;
using Ultilities.Constants;
using Utilities;
using Utilities.Contants;
using static Utilities.Contants.Constants;
using WEB.CMS.Customize;
using Catching.Elasticsearch;
using Entities.ViewModels.Elasticsearch;

namespace Xtech.CMS.Controllers
{
    public class InvoiceRequestController : Controller
    {
        private readonly IInvoiceRequestRepository _invoiceRequestRepository;
        private readonly IAllCodeRepository _allCodeRepository;
        private readonly InvoiceRequestESRepository _invoiceRequestESRepository;
        private readonly IUserRepository _userRepository;
        private readonly IConfiguration _configuration;
        private ManagementUser _ManagementUser;
        private readonly IOrderRepository _orderRepository;
        private readonly IInvoiceRequestDetailRepository _invoiceRequestDetailRepository;
        public InvoiceRequestController(IInvoiceRequestRepository invoiceRequestRepository, 
            IInvoiceRequestDetailRepository invoiceRequestDetailRepository,
            IOrderRepository orderRepository,
            IAllCodeRepository allCodeRepository,
            ManagementUser ManagementUser,
            IUserRepository userRepository,
            IConfiguration configuration)
        {
            _configuration = configuration;
            _invoiceRequestRepository = invoiceRequestRepository;
            _invoiceRequestDetailRepository = invoiceRequestDetailRepository;
            _invoiceRequestESRepository = new InvoiceRequestESRepository(_configuration["DataBaseConfig:Elastic:Host"], configuration);
            _orderRepository = orderRepository;
            _userRepository = userRepository;
            _ManagementUser = ManagementUser;
            _allCodeRepository = allCodeRepository;
        }


        public async Task<IActionResult> Index() 
        {
            var PAYMENT_REQUEST_STATUS = _allCodeRepository.GetListByType(AllCodeType.PAYMENT_REQUEST_STATUS);
            ViewBag.PAYMENT_REQUEST_STATUS = PAYMENT_REQUEST_STATUS;
            var current_user = _ManagementUser.GetCurrentUser();
            ViewBag.buttomThem = 0;
            if (current_user != null)
            {
                var i = 0;
                if (current_user != null && !string.IsNullOrEmpty(current_user.Role))
                {
                    var list = Array.ConvertAll(current_user.Role.Split(','), int.Parse);
                    foreach (var item in list)
                    {
                        //kiểm tra chức năng có đc phép sử dụng
                        var listPermissions = await _userRepository.CheckRolePermissionByUserAndRole(current_user.Id, item, (int)Constants.SortOrder.THEM, (int)MenuId.QL_KHACH_HANG);
                        if (listPermissions == true)
                        {
                            ViewBag.buttomThem = 1;
                        }

                    }
                }
            }
            return View();
        }

        public async Task<IActionResult> Search(InvoiceSearchViewModel searchModel) 
        {
            try
            {
                GenericViewModel<InvoiceRequestViewModel> model = new GenericViewModel<InvoiceRequestViewModel>();
                model = await _invoiceRequestRepository.GetListInvoiceRequest(searchModel);
                return PartialView(model);
            }
            catch (Exception ex)
            {
                LogHelper.InsertLogTelegram("Search - InvoiceRequestController: " + ex);
            }

            return PartialView();
        }

        public async Task<IActionResult> InvoiceRequestForm(int invoiceId,int client_Id,string Client_Name)
        {
            try
            {
                var current_user = _ManagementUser.GetCurrentUser();
                ViewBag.buttonDuyet = 0;
                ViewBag.buttonThem = 0;
                if (current_user != null)
                {
                    var i = 0;
                    if (current_user != null && !string.IsNullOrEmpty(current_user.Role))
                    {
                        var list = Array.ConvertAll(current_user.Role.Split(','), int.Parse);
                        foreach (var item in list)
                        {
                            //kiểm tra chức năng có đc phép sử dụng
                            var pemissionDuyet = await _userRepository.CheckRolePermissionByUserAndRole(current_user.Id, item, (int)Constants.SortOrder.DUYET, (int)MenuId.YEU_CAU_XUAT_HOA_DON);
                            if (pemissionDuyet == true)
                            {
                                ViewBag.buttonDuyet = 1;
                            }
                            var pemissionThem = await _userRepository.CheckRolePermissionByUserAndRole(current_user.Id, item, (int)Constants.SortOrder.THEM, (int)MenuId.YEU_CAU_XUAT_HOA_DON);
                            if (pemissionThem == true)
                            {
                                ViewBag.buttonThem = 1;
                            }
                        }
                    }
                }
                var data = await _invoiceRequestRepository.GetInvoiceRequestById(invoiceId);
                ViewBag.ClientName = Client_Name;
                if (data != null) 
                {
                    ViewBag.ClientName = data[0].ClientName;
                    ViewBag.PlanDate = data[0].PlanDate;
                    ViewBag.OrderId = data[0].OrderId;
                    ViewBag.Note = data[0].Note;
                    ViewBag.TaxNo = data[0].TaxNo;
                    ViewBag.CompanyName = data[0].CompanyName;
                    ViewBag.Address = data[0].Address;
                }
                ViewBag.Invoice_Id = invoiceId;
                ViewBag.ClientId = client_Id;
                return PartialView(data);
            }
            catch (Exception ex)
            {
                LogHelper.InsertLogTelegram("BillVAT-OrderController" + ex.ToString());
                return PartialView();
            }
        }

        [HttpPost]
        public async Task<GenericViewModel<OrderViewModel>> ListOrderRelated(OrderViewSearchModel searchModel) 
        {
            try
            {
                var model = new GenericViewModel<OrderViewModel>();
                model = await _orderRepository.GetList(searchModel);

                return model;
            }
            catch (Exception ex)
            {
                LogHelper.InsertLogTelegram("ListOrderRelated - InvoiceRequestController: " + ex.ToString());

            }
            return null;
        }

        [HttpPost]
        public async Task<IActionResult> InvoiceRequestSuggestion(string txt_search) 
        {
            try
            {
                long _UserId = 0;
                var data = new List<InvoiceRequestESViewModel>();
                if (HttpContext.User.FindFirst(ClaimTypes.NameIdentifier) != null)
                {
                    _UserId = Convert.ToInt64(HttpContext.User.FindFirst(ClaimTypes.NameIdentifier).Value);
                }
                if (txt_search != null)
                {
                    data = await _invoiceRequestESRepository.GetInvoiceRequestByNO(txt_search);
                    return Ok(new
                    {
                        status = (int)ResponseType.SUCCESS,
                        data = data,
                        selected = _UserId
                    });
                }
                else
                {
                    return Ok(new
                    {
                        status = (int)ResponseType.SUCCESS,
                        data = new List<InvoiceRequestESViewModel>()
                    });
                }
            }
            catch (Exception ex)
            {
                LogHelper.InsertLogTelegram("InvoiceRequestSuggestion - InvoiceRequestController: " + ex.ToString());
                return Ok(new
                {
                    status = (int)ResponseType.SUCCESS,
                    data = new List<InvoiceRequestESViewModel>()
                });
            }

        }

        [HttpPost]
        public async Task<int> ApproveOrReject(InvoiceRequest invoiceRequest)
        {
            try
            {
                var currentUserId = int.Parse(HttpContext.User.FindFirst(ClaimTypes.NameIdentifier).Value);
                if(invoiceRequest.Status == (int)Payment_Request_Status.DA_DUYET) 
                {
                    invoiceRequest.VerifyDate = DateTime.Now;
                    invoiceRequest.UserVerify = currentUserId;
                }
                if(invoiceRequest.Status == (int)Payment_Request_Status.BI_TU_CHOI) 
                {
                    invoiceRequest.UpdatedBy = currentUserId;
                    invoiceRequest.UpdatedDate = DateTime.Now;
                }
               
                var IdInvoiceREquest = await _invoiceRequestRepository.SetUpInvoiceRequest(invoiceRequest);
                return 1;
            }
            catch (Exception ex)
            {
                LogHelper.InsertLogTelegram("ApproveOrReject - InvoiceRequestController: " + ex.ToString());

            }
            return -1;
        }

        [HttpPost]
        public async Task<int> SubmitChange(List<InvoiceRequestDetail> lstInvoiceRQDetail, List<int> lstRemove,InvoiceRequest invoiceRequest)
        {
            try
            {
                var currentUserId = int.Parse(HttpContext.User.FindFirst(ClaimTypes.NameIdentifier).Value);
                invoiceRequest.UpdatedBy = currentUserId;
                invoiceRequest.CreatedBy = currentUserId;
                var IdInvoiceREquest = await _invoiceRequestRepository.SetUpInvoiceRequest(invoiceRequest);
                if (IdInvoiceREquest != 0) 
                {
                    foreach (var i in lstInvoiceRQDetail) 
                    {
                        i.UpdatedBy = currentUserId;
                        i.InvoiceRequestId = IdInvoiceREquest;
                        i.CreatedBy = currentUserId;
                        await _invoiceRequestDetailRepository.SetUpInvoiceRequestDetail(i);
                    }
                    foreach (var i in lstRemove)
                    {
                        await _invoiceRequestDetailRepository.DeleteInvoiceRequestDetailById(i);
                    }
                    return IdInvoiceREquest;
                }
                return -1;
            }
            catch (Exception ex)
            {
                LogHelper.InsertLogTelegram("SubmitChange - InvoiceRequestController: " + ex.ToString());

            }
            return -1;
        }
    }
}
