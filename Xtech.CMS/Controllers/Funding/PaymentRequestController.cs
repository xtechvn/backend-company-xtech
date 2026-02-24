using Entities.Models;
using Entities.ViewModels;
using Entities.ViewModels.Funding;
using Entities.ViewModels.HotelBookingRoom;
using Entities.ViewModels.SetServices;
using Entities.ViewModels.SupplierConfig;
using Microsoft.AspNetCore.Mvc;
using Nest;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

using Repositories.IRepositories;
using Repositories.Repositories;
using StackExchange.Redis;
using System.Linq;
using System.Security.Claims;
using Ultilities.Constants;
using Utilities;
using Utilities.Contants;
using WEB.Adavigo.CMS.Service;
using WEB.CMS.Customize;

using WEB.CMS.Service;
using Xtech.CMS.Models;
using Xtech.CMS.Services;
using static Utilities.Contants.Constants;

namespace WEB.Adavigo.CMS.Controllers.Funding
{
    [CustomAuthorize]

    public class PaymentRequestController : Controller
    {
        private readonly IAllCodeRepository _allCodeRepository;
        private readonly IWebHostEnvironment _WebHostEnvironment;
        private readonly IPaymentRequestRepository _paymentRequestRepository;
        private readonly ISupplierRepository _supplierRepository;
       
        private readonly IOtherBookingRepository _otherBookingRepository;
       
        private readonly IBankingAccountRepository _bankingAccountRepository;
        private readonly IClientRepository _clientRepository;
        private readonly Xtech.CMS.Models.AppSettings config;
        private IndentiferService    _indentiferService;
        private readonly IContractPayRepository _contractPayRepository;
        private ManagementUser _ManagementUser;
        private APIService apiService;
        private readonly IUserRepository _userRepository;
     
        
        public PaymentRequestController(IAllCodeRepository allCodeRepository, IWebHostEnvironment hostEnvironment, ManagementUser ManagementUser,
           IPaymentRequestRepository paymentRequestRepository, ISupplierRepository supplierRepository, IUserRepository userRepository,
          IConfiguration configuration,
           IOtherBookingRepository otherBookingRepository, IBankingAccountRepository bankingAccountRepository,
           IClientRepository clientRepository, IContractPayRepository contractPayRepository, IIdentifierServiceRepository identifierServiceRepository, IOrderRepository orderRepository)
        {
            _contractPayRepository = contractPayRepository;
            _WebHostEnvironment = hostEnvironment;
            _allCodeRepository = allCodeRepository;
            _paymentRequestRepository = paymentRequestRepository;
            _supplierRepository = supplierRepository;
            _indentiferService = new IndentiferService(configuration, identifierServiceRepository, orderRepository, contractPayRepository);
            _ManagementUser = ManagementUser;
            _userRepository = userRepository;
            apiService = new APIService(configuration, userRepository);
            _otherBookingRepository = otherBookingRepository;
          
            config = ReadFile.LoadConfig();
          
            _bankingAccountRepository = bankingAccountRepository;
            _clientRepository = clientRepository;
          
        }

        public IActionResult Index()
        {
            ViewBag.allCode_PAY_TYPE = _allCodeRepository.GetListByType(AllCodeType.PAY_TYPE);
            ViewBag.allCode_PAYMENT_VOUCHER_TYPE = _allCodeRepository.GetListByType(AllCodeType.PAYMENT_VOUCHER_TYPE);
            ViewBag.allCode_PAYMENT_REQUEST_STATUS = _allCodeRepository.GetListByType(AllCodeType.PAYMENT_REQUEST_STATUS);
            return View();
        }
        [HttpPost]
        public async Task<IActionResult> GetListBankAccountBySupplierId(int supplierId)
        {
            try
            {
                var listPayment = _supplierRepository.GetSupplierPaymentList(supplierId);
                return Ok(new
                {
                    isSuccess = true,
                    message = "Thành công",
                    data = listPayment
                });
            }
            catch (Exception ex)
            {
                LogHelper.InsertLogTelegram("GetListBankAccountBySupplierId - PaymentRequestController: " + ex + ". Đã có lỗi xảy ra");
                return Ok(new
                {
                    isSuccess = false,
                    message = "Thất bại",
                    data = new List<SupplierPaymentViewModel>()
                });
            }
        }

        [HttpPost]
        public async Task<IActionResult> GetListBankAccountByClientID(int clientId)
        {
            try
            {
                var listPayment = _bankingAccountRepository.GetBankAccountByClientId(clientId);
                return Ok(new
                {
                    isSuccess = true,
                    message = "Thành công",
                    data = listPayment
                });
            }
            catch (Exception ex)
            {
                LogHelper.InsertLogTelegram("GetListBankAccountBySupplierId - PaymentRequestController: " + ex + ". Đã có lỗi xảy ra");
                return Ok(new
                {
                    isSuccess = false,
                    message = "Thất bại",
                    data = new List<SupplierPaymentViewModel>()
                });
            }
        }
        public async Task<string> GetSuppliersSuggest(string name)
        {
            try
            {
                var supplierList = await _supplierRepository.GetSuggestionList(name);
                var suggestionlist = supplierList.Select(s => new SupplierViewModel
                {
                    id = s.SupplierId,
                    fullname = s.FullName,
                    Email = s.Email,
                    Phone = s.Phone,
                }).ToList();
                return JsonConvert.SerializeObject(suggestionlist);
            }
            catch (Exception ex)
            {
                LogHelper.InsertLogTelegram("GetSuppliersSuggest - PaymentRequestController: " + ex + ". Đã có lỗi xảy ra");
                return null;
            }
        }

        [HttpPost]
        public IActionResult Search(PaymentRequestSearchModel searchModel, int currentPage = 1, int pageSize = 20)
        {
            var model = new GenericViewModel<PaymentRequestViewModel>();
            try
            {
                if (searchModel.CreateByIds == null) searchModel.CreateByIds = new List<int>();
                if (searchModel.PaymentTypeMulti == null) searchModel.PaymentTypeMulti = new List<int>();
                if (searchModel.StatusMulti == null) searchModel.StatusMulti = new List<int>();
                if (searchModel.Status > -1)
                    searchModel.StatusMulti.Add(searchModel.Status);
                if (searchModel.TypeMulti == null) searchModel.TypeMulti = new List<int>();
                if (!string.IsNullOrEmpty(searchModel.PaymentCode))
                    searchModel.PaymentCode = searchModel.PaymentCode.Trim();
                if (!string.IsNullOrEmpty(searchModel.OrderNo))
                    searchModel.OrderNo = searchModel.OrderNo.Trim();
                if (!string.IsNullOrEmpty(searchModel.ServiceCode))
                    searchModel.ServiceCode = searchModel.ServiceCode.Trim();
                if (!string.IsNullOrEmpty(searchModel.Content))
                    searchModel.Content = searchModel.Content.Trim();
                var current_user = _ManagementUser.GetCurrentUser();
                if (searchModel.CreateByIds.Count == 0)
                {
                    if (!string.IsNullOrEmpty(current_user.UserUnderList))
                        searchModel.CreateByIds = current_user.UserUnderList.Split(',').Select(n => int.Parse(n)).ToList();
                    var list = Array.ConvertAll(current_user.Role.Split(','), int.Parse);
                    foreach (var item in list)
                    {
                        if (item == (int)RoleType.Admin || item == (int)RoleType.PhoTPKeToan || item == (int)RoleType.KeToanTruong)
                        {
                            searchModel.CreateByIds = null;
                        }
                    }
                }

                var listPaymentRequest = _paymentRequestRepository.GetPaymentRequests(searchModel, out long total, currentPage, pageSize);
                model.CurrentPage = currentPage;
                model.ListData = listPaymentRequest;
                model.PageSize = pageSize;
                model.TotalRecord = total;
                model.TotalPage = (int)Math.Ceiling((double)total / pageSize);
                ViewBag.Amount = 0;
                var countStatus = _paymentRequestRepository.GetCountStatus(searchModel);
                switch (searchModel.Status)
                {
                    case -1:
                        {
                            ViewBag.Amount = countStatus.Sum(s => s.Amount);
                        }
                        break;
                    case (int)PaymentRequetStatus.Luu_Nhap:
                        {
                            ViewBag.Amount = countStatus.Where(s => s.Status == (int)PaymentRequetStatus.Luu_Nhap).Sum(s => s.Amount);
                        }
                        break;
                    case (int)PaymentRequetStatus.Tu_choi:
                        {
                            ViewBag.Amount = countStatus.Where(s => s.Status == (int)PaymentRequetStatus.Tu_choi).Sum(s => s.Amount);
                        }
                        break;
                    case (int)PaymentRequetStatus.Cho_TBP_duyet:
                        {
                            ViewBag.Amount = countStatus.Where(s => s.Status == (int)PaymentRequetStatus.Cho_TBP_duyet).Sum(s => s.Amount);
                        }
                        break;
                    case (int)PaymentRequetStatus.Cho_KTT_duyet:
                        {
                            ViewBag.Amount = countStatus.Where(s => s.Status == (int)PaymentRequetStatus.Cho_KTT_duyet).Sum(s => s.Amount);

                        }
                        break;
                    case (int)PaymentRequetStatus.Cho_chi:
                        {
                            ViewBag.Amount = countStatus.Where(s => s.Status == (int)PaymentRequetStatus.Cho_chi).Sum(s => s.Amount);
                        }
                        break;
                    case (int)PaymentRequetStatus.Da_chi:
                        {
                            ViewBag.Amount = countStatus.Where(s => s.Status == (int)PaymentRequetStatus.Da_chi).Sum(s => s.Amount);
                        }
                        break;
                }
                ViewBag.VIEW_PHIEU_CHI = 0;
                if (current_user != null && !string.IsNullOrEmpty(current_user.Role))
                {
                    var listRole = Array.ConvertAll(current_user.Role.Split(','), int.Parse);
                    foreach (var item in listRole)
                    {
                        //kiểm tra chức năng có đc phép sử dụng
                        var checkRolePermission = _userRepository.CheckRolePermissionByUserAndRole(current_user.Id, item,
                            1, (int)MenuId.PHIEU_CHI).Result;
                        if (checkRolePermission == true)
                        {
                            ViewBag.VIEW_PHIEU_CHI = 1;
                            break;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                LogHelper.InsertLogTelegram("Search - PaymentRequestController: " + ex + ". Đã có lỗi xảy ra");
            }
            return PartialView(model);
        }

        [HttpPost]
        public async Task<IActionResult> CountStatus(PaymentRequestSearchModel searchModel)
        {
            try
            {
                if (searchModel.CreateByIds == null) searchModel.CreateByIds = new List<int>();
                if (searchModel.PaymentTypeMulti == null) searchModel.PaymentTypeMulti = new List<int>();
                if (searchModel.StatusMulti == null) searchModel.StatusMulti = new List<int>();
                if (searchModel.Status > -1)
                    searchModel.StatusMulti.Add(searchModel.Status);
                if (searchModel.TypeMulti == null) searchModel.TypeMulti = new List<int>();
                if (!string.IsNullOrEmpty(searchModel.PaymentCode))
                    searchModel.PaymentCode = searchModel.PaymentCode.Trim();
                if (!string.IsNullOrEmpty(searchModel.Content))
                    searchModel.Content = searchModel.Content.Trim();
                if (!string.IsNullOrEmpty(searchModel.OrderNo))
                    searchModel.OrderNo = searchModel.OrderNo.Trim();
                var current_user = _ManagementUser.GetCurrentUser();
                if (!string.IsNullOrEmpty(current_user.UserUnderList) && (searchModel.CreateByIds == null || searchModel.CreateByIds.Count == 0))
                    searchModel.CreateByIds = current_user.UserUnderList.Split(',').Select(n => int.Parse(n)).ToList();
                var countStatus = _paymentRequestRepository.GetCountStatus(searchModel);
                var countStatusAll = new CountStatus();
                countStatusAll.Status = -1;
                countStatusAll.Total = countStatus.Sum(n => n.Total);
                countStatus.Add(countStatusAll);
                return Ok(new
                {
                    isSuccess = true,
                    data = countStatus
                });
            }
            catch (Exception ex)
            {
                LogHelper.InsertLogTelegram("CountStatus - PaymentRequestController: " + ex + ". Đã có lỗi xảy ra");
                return Ok(new
                {
                    isSuccess = false,
                    data = new List<CountStatus>()
                });
            }
        }

        public IActionResult AddNew(long serviceId, int serviceType, long supplierId, decimal amount, long orderId, string serviceCode,
            int clientId, decimal amount_supplier_refund, int payment_request_type = 1)
        {
            var supplier = _supplierRepository.GetById((int)supplierId);
            ViewBag.allCode_PAY_TYPE = _allCodeRepository.GetListByType(AllCodeType.PAY_TYPE);
            ViewBag.allCode_PAYMENT_VOUCHER_TYPE = _allCodeRepository.GetListByType(AllCodeType.PAYMENT_VOUCHER_TYPE);
            ViewBag.serviceId = serviceId;
            ViewBag.serviceType = serviceType;
            ViewBag.supplierId = supplierId;
            ViewBag.supplierName = supplier?.FullName;
            ViewBag.amount = amount;
            ViewBag.amount_supplier_refund = amount_supplier_refund;
            ViewBag.serviceCode = serviceCode;
            ViewBag.orderId = orderId;
            ViewBag.clientId = clientId;
            ViewBag.payment_request_type = payment_request_type;
            var listPaymentRequestByService = _paymentRequestRepository.GetByServiceId(serviceId, serviceType).Where(n => (n.IsDelete == null || n.IsDelete.Value == false)
                    && n.Status != (int)PAYMENT_REQUEST_STATUS.TU_CHOI).ToList();
            ViewBag.totalPayment = listPaymentRequestByService.Sum(n => n.Amount);
            ViewBag.listPayment = new List<BankingAccount>();
            if (clientId > 0)
            {
                listPaymentRequestByService = _paymentRequestRepository.GetRequestByClientId(clientId).Where(n => (n.IsDelete == null || n.IsDelete.Value == false)
                   && n.Status != (int)PAYMENT_REQUEST_STATUS.TU_CHOI && n.ServiceId == serviceId).ToList();
                ViewBag.totalPayment = listPaymentRequestByService.Sum(n => n.Amount);
                ViewBag.listPayment = _bankingAccountRepository.GetBankAccountByClientId(clientId);
                var clientInfo = _clientRepository.GetClientDetailByClientId(clientId).Result;
                ViewBag.clientName = clientInfo.ClientName;
            }
            return PartialView();
        }

        //public IActionResult Edit(int paymentRequestId, long serviceId, int serviceType, long supplierId, decimal amount,
        //    long orderId, int clientId, decimal amount_supplier_refund, int payment_request_type = 1)
        //{
        //    ViewBag.allCode_PAY_TYPE = _allCodeRepository.GetListByType(AllCodeType.PAY_TYPE);
        //    ViewBag.allCode_PAYMENT_VOUCHER_TYPE = _allCodeRepository.GetListByType(AllCodeType.PAYMENT_VOUCHER_TYPE);
        //    var requestDetail = _paymentRequestRepository.GetById(paymentRequestId);
        //    if (requestDetail.IsSupplierDebt == null)
        //        requestDetail.IsSupplierDebt = false;
        //    var supplier = _supplierRepository.GetById((int)supplierId);
        //    ViewBag.serviceId = serviceId;
        //    ViewBag.amount_supplier_refund = amount_supplier_refund;
        //    ViewBag.serviceType = serviceType;
        //    ViewBag.supplierId = supplierId;
        //    ViewBag.supplierName = supplier?.FullName;
        //    ViewBag.amount = amount;
        //    ViewBag.clientId = clientId;
        //    ViewBag.orderId = orderId;
        //    ViewBag.isEditAmountReject = false;
        //    ViewBag.payment_request_type = payment_request_type;
        //    if (serviceId == 0 && requestDetail != null && requestDetail.RelateData != null && requestDetail.RelateData.Count > 0)
        //    {
        //        ViewBag.serviceType = requestDetail.RelateData[0].ServiceType;
        //        ViewBag.serviceId = requestDetail.RelateData[0].ServiceId;
        //    }
        //    if (requestDetail != null && requestDetail.Type != (int)PAYMENT_VOUCHER_TYPE.HOAN_TRA_KHACH_HANG
        //        && requestDetail.Status == (int)PAYMENT_REQUEST_STATUS.TU_CHOI)
        //    {
        //        var current_user = _ManagementUser.GetCurrentUser();
        //        bool isAdmin = _userRepository.IsAdmin(current_user.Id);
        //        if (requestDetail.CreatedBy == current_user.Id || isAdmin)
        //        {
        //            ViewBag.isEditAmountReject = true;
        //        }
        //    }
        //    ViewBag.listPayment = new List<BankingAccount>();
        //    if (clientId > 0)
        //    {
        //        ViewBag.listPayment = _bankingAccountRepository.GetBankAccountByClientId(clientId);
        //    }
        //    return PartialView(requestDetail);
        //}

        public IActionResult EditAdmin(int paymentRequestId, long serviceId, int serviceType, long supplierId, decimal amount, long orderId, int clientId
            , int payment_request_type = 1)
        {
            ViewBag.allCode_PAY_TYPE = _allCodeRepository.GetListByType(AllCodeType.PAY_TYPE);
            ViewBag.allCode_PAYMENT_VOUCHER_TYPE = _allCodeRepository.GetListByType(AllCodeType.PAYMENT_VOUCHER_TYPE);
            var requestDetail = _paymentRequestRepository.GetById(paymentRequestId);
            if (requestDetail.IsSupplierDebt == null)
                requestDetail.IsSupplierDebt = false;
            var supplier = _supplierRepository.GetById((int)supplierId);
            ViewBag.serviceId = serviceId;
            ViewBag.serviceType = serviceType;
            ViewBag.supplierId = supplierId;
            ViewBag.supplierName = supplier?.FullName;
            ViewBag.amount = amount;
            ViewBag.clientId = clientId;
            ViewBag.payment_request_type = payment_request_type;
            ViewBag.orderId = orderId;
            ViewBag.isEditAmountReject = false;
            if (requestDetail != null && requestDetail.Type != (int)PAYMENT_VOUCHER_TYPE.HOAN_TRA_KHACH_HANG
                && requestDetail.Status == (int)PAYMENT_REQUEST_STATUS.TU_CHOI)
            {
                var current_user = _ManagementUser.GetCurrentUser();
                bool isAdmin = _userRepository.IsAdmin(current_user.Id);
                if (requestDetail.CreatedBy == current_user.Id || isAdmin)
                {
                    ViewBag.isEditAmountReject = true;
                }
            }
            ViewBag.listPayment = new List<BankingAccount>();
            if (clientId > 0)
            {
                ViewBag.listPayment = _bankingAccountRepository.GetBankAccountByClientId(clientId);
            }
            return PartialView(requestDetail);
        }

        private int Validate(PaymentRequestViewModel model, out string message, bool isEdit = false)
        {
            var result = 1;
            message = string.Empty;
            if (model.Type == 0)
            {
                message = "Vui lòng chọn loại nghiệp vụ";
                return -1;
            }
            if (model.PaymentType == 0)
            {
                message = "Vui lòng chọn hình thức";
                return -1;
            }
            if (model.SupplierId == 0 && model.Type != (int)PAYMENT_VOUCHER_TYPE.HOAN_TRA_KHACH_HANG)
            {
                message = "Vui lòng chọn nhà cung cấp";
                return -1;
            }
            if (model.ClientId == 0 && model.Type == (int)PAYMENT_VOUCHER_TYPE.HOAN_TRA_KHACH_HANG)
            {
                message = "Vui lòng chọn khách hàng";
                return -1;
            }
            if (model.Amount < 0)
            {
                message = "Vui lòng nhập số tiền";
                return -1;
            }
            if (model.PaymentType == (int)DepositHistoryConstant.CONTRACT_PAYMENT_TYPE.CHUYEN_KHOAN
              && model.BankingAccountId == 0 && model.Type != (int)PAYMENT_VOUCHER_TYPE.QUY_CHAM_SOC_KHACH_HANG)
            {
                message = "Vui lòng chọn tài khoản ngân hàng nhận";
                return -1;
            }
            //if (model.PaymentType == (int)DepositHistoryConstant.CONTRACT_PAYMENT_TYPE.CHUYEN_KHOAN && string.IsNullOrEmpty(model.BankName))
            //{
            //    message = "Vui lòng nhập tên ngân hàng nhận";
            //    return -1;
            //}
            //if (model.PaymentType == (int)DepositHistoryConstant.CONTRACT_PAYMENT_TYPE.CHUYEN_KHOAN && string.IsNullOrEmpty(model.BankAccount))
            //{
            //    message = "Vui lòng nhập tài khoản ngân hàng nhận";
            //    return -1;
            //}
            if (model.PaymentDate == null)
            {
                message = "Ngày thanh toán không được để trống";
                return -1;
            }
            if (!isEdit && model.PaymentDate < DateTime.Today)
            {
                message = "Ngày thanh toán không được nhỏ hơn ngày hiện tại";
                return -1;
            }
            if (string.IsNullOrEmpty(model.Note))
            {
                message = "Vui lòng nhập nội dung";
                return -1;
            }
            if (!string.IsNullOrEmpty(model.Note) && model.Note.Length > 500)
            {
                message = "Nội dung chỉ được nhập tối đa 500 kí tự";
                return -1;
            }
            if (!string.IsNullOrEmpty(model.Description) && model.Description.Length > 3000)
            {
                message = "Ghi chú chỉ được nhập tối đa 3000 kí tự";
                return -1;
            }

            if (model.Type == (int)PAYMENT_VOUCHER_TYPE.THANH_TOAN_DICH_VU && isEdit && model.IsEditAmountReject && !model.IsAdminEdit && model.SupplierId != 0)//check admin edit Amount
            {

                var requestDetail = _paymentRequestRepository.GetById((int)model.Id);

                foreach (var item in requestDetail.RelateData)
                {
                    if ((double)model.Amount > (item.ServicePrice - Convert.ToInt32(model.TotalSupplierRefund == null ? 0 : model.TotalSupplierRefund)))
                    {
                        message = "Vui lòng nhập số tiền nhỏ hơn hoặc bằng số tiền yêu cầu chi cho nhà cung cấp. " +
                            "Tổng tiền yêu cầu chi tối đa: " + item.ServicePrice.ToString("N0");
                        return -1;
                    }
                    //var listPaymentRequestCreated = _paymentRequestRepository.GetByServiceId(item.ServiceId, item.ServiceType)
                    //    .Where(n => n.Status != (int)PAYMENT_REQUEST_STATUS.TU_CHOI && n.Id != requestDetail.Id).ToList();
                    //if (listPaymentRequestCreated.Count > 0 && (double)model.Amount > (item.ServicePrice - (double)listPaymentRequestCreated.Sum(n => n.Amount)))
                    //{
                    //    message = "Vui lòng nhập số tiền nhỏ hơn hoặc bằng số tiền yêu cầu chi còn lại cho nhà cung cấp. " +
                    //      "Tổng tiền yêu cầu chi còn lại: " + (item.ServicePrice - (double)listPaymentRequestCreated.Sum(n => n.Amount)).ToString("N0");
                    //    return -1;
                    //}

                }
            }

            if (model.Type == (int)PAYMENT_VOUCHER_TYPE.THANH_TOAN_DICH_VU && model.IsServiceIncluded != null && model.IsServiceIncluded.Value && !model.IsAdminEdit)
            {
                var listPaymentRequestByServiceId = _paymentRequestRepository.GetByServiceId(model.PaymentRequestDetails != null ? model.PaymentRequestDetails[0].ServiceId : model.ServiceId,
                   model.PaymentRequestDetails != null ? model.PaymentRequestDetails[0].ServiceType : model.ServiceType);

                if (model.TotalAmountService == 0 && model.TotalSupplierRefund == 0)
                {
                    model.TotalAmountService = listPaymentRequestByServiceId.Where(n => n.Status != (int)PAYMENT_REQUEST_STATUS.LUU_NHAP).Sum(n => n.Amount);
                }
                var totalPaymentRequest = listPaymentRequestByServiceId.Where(n => n.Status != (int)PAYMENT_REQUEST_STATUS.TU_CHOI).Sum(n => n.Amount);
                if (model.TotalAmountService + model.TotalSupplierRefund < totalPaymentRequest + model.Amount)
                {
                    message = "Tổng tiền yêu cầu chi bạn tạo đã vượt quá số tiền yêu cho phép trong dịch vụ. Vui lòng kiểm tra lại!";
                    return -1;
                }
            }
            return result;
        }

        [HttpPost]
        public async Task<IActionResult> AddJson(PaymentRequestViewModel model)
        {
            try
            {
                var userLogin = 0;
                if (HttpContext.User.FindFirst(ClaimTypes.NameIdentifier) != null)
                {
                    userLogin = int.Parse(HttpContext.User.FindFirst(ClaimTypes.NameIdentifier).Value);
                }
                model.CreatedBy = userLogin;
                model.PaymentDate = DateUtil.StringToDateTime(model.PaymentDateStr, "dd/MM/yyyy HH:mm");
                if (model.PaymentDate == null)
                    model.PaymentDate = DateUtil.StringToDate(model.PaymentDateStr);
                //var validate = Validate(model, out string messages);
                //if (validate < 0)
                //{
                //    return Ok(new
                //    {
                //        isSuccess = false,
                //        message = messages
                //    });
                //}
                //var client = new HttpClient();
                //var apiPrefix = ReadFile.LoadConfig().API_URL + ReadFile.LoadConfig().API_GET_BILL_NO;
                //var key_token_api = ReadFile.LoadConfig().KEY_TOKEN_API_MANUAL;
                //HttpClient httpClient = new HttpClient();
                //JObject jsonObject = new JObject(
                //   new JProperty("code_type", ((int)GET_CODE_MODULE.YEU_CAU_CHI).ToString())
                //);
                //var j_param = new Dictionary<string, object>
                // {
                //     { "key",jsonObject}
                // };
                //var data_product = JsonConvert.SerializeObject(j_param);
                //var token = CommonHelper.Encode(data_product, key_token_api);
                //var content = new FormUrlEncodedContent(new[] { new KeyValuePair<string, string>("token", token) });
                //var response = await httpClient.PostAsync(apiPrefix, content);
                //var resultAPI = await response.Content.ReadAsStringAsync();
                //var output = JsonConvert.DeserializeObject<OutputAPI>(resultAPI);
                model.PaymentCode = await _indentiferService.BuildPaymentRequest();
                var result = _paymentRequestRepository.CreatePaymentRequest(model);
                if (result == -2)
                    return Ok(new
                    {
                        isSuccess = false,
                        message = "Mã phiếu yêu cầu chi " + model.PaymentCode + " đã tồn tại trên hệ thống . Vui lòng kiểm tra lại."
                    });
                if (result < 1)
                    return Ok(new
                    {
                        isSuccess = false,
                        message = "Thêm phiếu yêu cầu chi thất bại"
                    });

                long _UserId = 0;
                if (HttpContext.User.FindFirst(ClaimTypes.NameIdentifier) != null)
                {
                    _UserId = Convert.ToInt64(HttpContext.User.FindFirst(ClaimTypes.NameIdentifier).Value);
                }
                var current_user = _ManagementUser.GetCurrentUser();
                string link = "/PaymentRequest/Detail?paymentRequestId=" + result;
                apiService.SendMessage(_UserId.ToString(), ((int)ModuleType.PHIEU_YEU_CAU_CHI).ToString(), ((int)Utilities.Contants.ActionType.TAO_YEU_CAU_CHI).ToString(), model.PaymentCode, link, current_user == null ? "0" : current_user.Role);

                return Ok(new
                {
                    isSuccess = true,
                    message = "Thêm phiếu chi yêu cầu thành công"
                });
            }
            catch (Exception ex)
            {
                LogHelper.InsertLogTelegram("AddJson - PaymentRequestController: " + ex + ". Đã có lỗi xảy ra");
                return Ok(new
                {
                    isSuccess = false,
                    message = "Thêm phiếu chi yêu cầu thất bại" + ". Đã có lỗi xảy ra"
                });
            }
        }

        [HttpPost]
        public async Task<IActionResult> Update(PaymentRequestViewModel model)
        {
            try
            {
                var userLogin = 0;
                if (HttpContext.User.FindFirst(ClaimTypes.NameIdentifier) != null)
                {
                    userLogin = int.Parse(HttpContext.User.FindFirst(ClaimTypes.NameIdentifier).Value);
                }
                model.UpdatedBy = userLogin;
                model.PaymentDate = DateUtil.StringToDateTime(model.PaymentDateStr, "dd/MM/yyyy HH:mm");
                if (model.PaymentDate == null)
                    model.PaymentDate = DateUtil.StringToDate(model.PaymentDateStr);
                var validate = Validate(model, out string messages, true);
                if (validate < 0)
                {
                    return Ok(new
                    {
                        isSuccess = false,
                        message = messages
                    });
                }
                var result = _paymentRequestRepository.UpdatePaymentRequest(model);
                if (result < 1)
                    return Ok(new
                    {
                        isSuccess = false,
                        message = "Cập nhật phiếu yêu cầu chi thất bại"
                    });
                return Ok(new
                {
                    isSuccess = true,
                    message = "Cập nhật phiếu yêu cầu chi thành công"
                });
            }
            catch (Exception ex)
            {
                LogHelper.InsertLogTelegram("Update - PaymentRequestController: " + ex + ". Đã có lỗi xảy ra");
                return Ok(new
                {
                    isSuccess = false,
                    message = "Cập nhật phiếu yêu cầu chi thất bại" + ". Đã có lỗi xảy ra"
                });
            }
        }

        public async Task<IActionResult> Detail(int paymentRequestId)
        {
            ViewBag.count_ycc = 0;
            //ViewBag.count_ycc = _countYCCMongoService.GetCountYCC(paymentRequestId);
            var model = _paymentRequestRepository.GetById(paymentRequestId);
            if (model.RelateData == null) model.RelateData = new List<PaymentRequestDetailViewModel>();
            var current_user = _ManagementUser.GetCurrentUser();
            ViewBag.TYPE = model.Type;
            ViewBag.TBP_DUYET_YEU_CAU_CHI = 0;
            ViewBag.KTT_DUYET_YEU_CAU_CHI = 0;
            ViewBag.KTV_DUYET_YEU_CAU_CHI = 0;
            ViewBag.isAdmin = false;
            if (current_user != null && !string.IsNullOrEmpty(current_user.Role))
            {
                var listRole = Array.ConvertAll(current_user.Role.Split(','), int.Parse);
                bool isAdmin = _userRepository.IsAdmin(current_user.Id);
                ViewBag.isAdmin = isAdmin;

                bool IsHeadOfAccountantPhoTPKeToan = _userRepository.IsHeadOfAccountantPhoTPKeToan(current_user.Id);
                if (IsHeadOfAccountantPhoTPKeToan)
                    ViewBag.KTT_DUYET_YEU_CAU_CHI = 1;
                bool isHeadOfAccountant = _userRepository.IsHeadOfAccountant(current_user.Id);
                if (isHeadOfAccountant)
                    ViewBag.KTT_DUYET_YEU_CAU_CHI = 1;

                bool IsAccountant = _userRepository.IsAccountant(current_user.Id);
                if (IsAccountant)
                    ViewBag.KTV_DUYET_YEU_CAU_CHI = 1;
                bool IsAccountantTour = _userRepository.IsAccountantTour(current_user.Id);
                if (IsAccountantTour)
                    ViewBag.TBP_DUYET_YEU_CAU_CHI = 1;

                var listServiceType = model.RelateData.Select(n => n.ServiceType).ToList();

                if (model.Type == (int)PAYMENT_VOUCHER_TYPE.THANH_TOAN_KHAC || model.Type == (int)PAYMENT_VOUCHER_TYPE.HOAN_TRA_KHACH_HANG)
                {
                    listServiceType = new List<int>();
                    listServiceType.Add((int)ServiceType.PRODUCT_FLY_TICKET);
                    listServiceType.Add((int)ServiceType.BOOK_HOTEL_ROOM_VIN);
                    listServiceType.Add((int)ServiceType.Tour);
                    listServiceType.Add((int)ServiceType.Other);
                }
                if (current_user.UserUnderList != null && current_user.UserUnderList != "" && current_user.Role.Contains(((int)RoleType.SaleKd).ToString()) == false && current_user.UserUnderList.Contains(model.CreatedBy.ToString()))
                {
                    ViewBag.TBP_DUYET_YEU_CAU_CHI = 1;
                }
                foreach (var item in listServiceType)
                {
                    var chiefOfDepartment = _userRepository.GetChiefofDepartmentByServiceTypeNew(item).Result.Select(n => n.Id).ToList();
                    if (chiefOfDepartment.Contains(current_user.Id))
                    {
                        ViewBag.TBP_DUYET_YEU_CAU_CHI = 1;
                        break;
                    }
                }
                //var list_note = await _noteRepository.GetListByType(paymentRequestId, (int)AttachmentType.YCC_Comment);
                //ViewBag.ListNote = list_note;
            }

            return View(model);
        }

        //[HttpPost]
        //public IActionResult ExportExcel(PaymentRequestSearchModel searchModel)
        //{
        //    try
        //    {
        //        string _FileName = "PaymentRequest_" + Guid.NewGuid() + ".xlsx";
        //        string _UploadFolder = @"Template/Export";
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
        //        if (searchModel.CreateByIds == null) searchModel.CreateByIds = new List<int>();
        //        if (!string.IsNullOrEmpty(searchModel.PaymentCode))
        //            searchModel.PaymentCode = searchModel.PaymentCode.Trim();
        //        if (!string.IsNullOrEmpty(searchModel.Content))
        //            searchModel.Content = searchModel.Content.Trim();

        //        string FilePath = Path.Combine(_UploadDirectory, _FileName);
        //        var rsPath = _paymentRequestRepository.ExportPaymentRequest(searchModel, FilePath);

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
        //        return new JsonResult(new
        //        {
        //            isSuccess = false,
        //            message = ex.Message.ToString() + ". Đã có lỗi xảy ra"
        //        });
        //    }
        //}

        //public IActionResult Reject()
        //{
        //    return PartialView();
        //}

        //[HttpPost]
        //public async Task<IActionResult> RejectRequest(string paymentRequestNo, string note)
        //{
        //    try
        //    {
        //        if (!string.IsNullOrEmpty(note) && note.Length > 300)
        //        {
        //            return Ok(new
        //            {
        //                isSuccess = false,
        //                message = "Nội dung từ chối không được lớn hơn 300 kí tự"
        //            });
        //        }
        //        var userLogin = 0;
        //        if (HttpContext.User.FindFirst(ClaimTypes.NameIdentifier) != null)
        //        {
        //            userLogin = int.Parse(HttpContext.User.FindFirst(ClaimTypes.NameIdentifier).Value);
        //        }
        //        var model = _paymentRequestRepository.GetByRequestNo(paymentRequestNo);
        //        var result = _paymentRequestRepository.RejectPaymentRequest(paymentRequestNo, note, userLogin);
        //        if (result < 1)
        //            return Ok(new
        //            {
        //                isSuccess = true,
        //                message = "Từ chối yêu cầu chi thất bại"
        //            });
        //        long _UserId = 0;
        //        if (HttpContext.User.FindFirst(ClaimTypes.NameIdentifier) != null)
        //        {
        //            _UserId = Convert.ToInt64(HttpContext.User.FindFirst(ClaimTypes.NameIdentifier).Value);
        //        }
        //        var current_user = _ManagementUser.GetCurrentUser();
        //        string link = "/PaymentRequest/Detail?paymentRequestId=" + model.Id;
        //        apiService.SendMessage(_UserId.ToString(), ((int)ModuleType.PHIEU_YEU_CAU_CHI).ToString(), ((int)Utilities.Contants.ActionType.TU_CHOI_DUYET_YEU_CAU_CHI).ToString(), model.PaymentCode, link, current_user == null ? "0" : current_user.Role);
        //        return Ok(new
        //        {
        //            isSuccess = true,
        //            message = "Từ chối yêu cầu chi thành công"
        //        });
        //    }
        //    catch (Exception ex)
        //    {
        //        LogHelper.InsertLogTelegram("Update - PaymentRequestController: " + ex + ". Đã có lỗi xảy ra");
        //        return Ok(new
        //        {
        //            isSuccess = false,
        //            message = "Từ chối yêu cầu chi thất bại" + ". Đã có lỗi xảy ra"
        //        });
        //    }
        //}

        [HttpPost]
        public async Task<IActionResult> Approve(string paymentRequestNo, int status)
        {
            try
            {
                var userLogin = 0;
                if (HttpContext.User.FindFirst(ClaimTypes.NameIdentifier) != null)
                {
                    userLogin = int.Parse(HttpContext.User.FindFirst(ClaimTypes.NameIdentifier).Value);
                }
                var model = _paymentRequestRepository.GetByRequestNo(paymentRequestNo);
                //int status = GetStatusUpdate(model);
                var result = _paymentRequestRepository.ApprovePaymentRequest(paymentRequestNo, userLogin, status);
                if (result < 1)
                    return Ok(new
                    {
                        isSuccess = true,
                        message = "Phê duyệt yêu cầu chi thất bại"
                    });
                long _UserId = 0;
                if (HttpContext.User.FindFirst(ClaimTypes.NameIdentifier) != null)
                {
                    _UserId = Convert.ToInt64(HttpContext.User.FindFirst(ClaimTypes.NameIdentifier).Value);
                }
                var current_user = _ManagementUser.GetCurrentUser();
                string link = "/PaymentRequest/Detail?paymentRequestId=" + model.Id;
                apiService.SendMessage(_UserId.ToString(), ((int)ModuleType.PHIEU_YEU_CAU_CHI).ToString(), ((int)Utilities.Contants.ActionType.DUYET_YEU_CAU_CHI).ToString(), model.PaymentCode, link, current_user == null ? "0" : current_user.Role);

                return Ok(new
                {
                    isSuccess = true,
                    message = "Phê duyệt yêu cầu chi thành công"
                });
            }
            catch (Exception ex)
            {
                LogHelper.InsertLogTelegram("Approve - PaymentRequestController: " + ex + ". Đã có lỗi xảy ra");
                return Ok(new
                {
                    isSuccess = false,
                    message = "Phê duyệt yêu cầu chi thất bại" + ". Đã có lỗi xảy ra"
                });
            }
        }

        //[HttpPost]
        //public async Task<IActionResult> Delete(string paymentRequestNo)
        //{
        //    try
        //    {
        //        var userLogin = 0;
        //        if (HttpContext.User.FindFirst(ClaimTypes.NameIdentifier) != null)
        //        {
        //            userLogin = int.Parse(HttpContext.User.FindFirst(ClaimTypes.NameIdentifier).Value);
        //        }
        //        var result = _paymentRequestRepository.DeletePaymentRequest(paymentRequestNo, userLogin);
        //        if (result < 1)
        //            return Ok(new
        //            {
        //                isSuccess = true,
        //                message = "Xóa yêu cầu chi thất bại"
        //            });

        //        return Ok(new
        //        {
        //            isSuccess = true,
        //            message = "Xóa yêu cầu chi thành công"
        //        });
        //    }
        //    catch (Exception ex)
        //    {
        //        LogHelper.InsertLogTelegram("Delete - PaymentRequestController: " + ex + ". Đã có lỗi xảy ra");
        //        return Ok(new
        //        {
        //            isSuccess = false,
        //            message = "Xóa yêu cầu chi thất bại" + ". Đã có lỗi xảy ra"
        //        });
        //    }
        //}

        [HttpPost]
        public async Task<IActionResult> GetServiceListBySupplierId(int supplierId, int requestId = 0, int serviceId = 0)
        {
            try
            {
                var listOrder = _paymentRequestRepository.GetServiceListBySupplierId(supplierId, requestId, serviceId);
                return Ok(new
                {
                    isSuccess = true,
                    message = "Thành công",
                    data = listOrder
                });
            }
            catch (Exception ex)
            {
                LogHelper.InsertLogTelegram("GetOrderListBySupplierId - PaymentRequestController: " + ex + ". Đã có lỗi xảy ra");
                return Ok(new
                {
                    isSuccess = false,
                    message = "Thất bại" + ". Đã có lỗi xảy ra",
                    data = new List<Entities.Models.Order>()
                });
            }
        }

        //[HttpPost]
        //public async Task<IActionResult> GetServiceListByClientId(int clientId, int requestId = 0)
        //{
        //    try
        //    {
        //        var listOrder = _paymentRequestRepository.GetServiceListByClientId(clientId, requestId);
        //        return Ok(new
        //        {
        //            isSuccess = true,
        //            message = "Thành công",
        //            data = listOrder
        //        });
        //    }
        //    catch (Exception ex)
        //    {
        //        LogHelper.InsertLogTelegram("GetOrderListByClientId - PaymentRequestController: " + ex + ". Đã có lỗi xảy ra");
        //        return Ok(new
        //        {
        //            isSuccess = false,
        //            message = "Thất bại" + ". Đã có lỗi xảy ra",
        //            data = new List<Entities.Models.Order>()
        //        });
        //    }
        //}

        //[HttpPost]
        //public async Task<IActionResult> GetAllSupplier()
        //{
        //    try
        //    {
        //        var supplierList = await _supplierRepository.GetSuggestionList("");

        //        return Ok(new
        //        {
        //            isSuccess = true,
        //            message = "Thành công",
        //            data = supplierList
        //        });
        //    }
        //    catch (Exception ex)
        //    {
        //        LogHelper.InsertLogTelegram("GetOtherPackageOption - PaymentRequestController: " + ex + ". Đã có lỗi xảy ra");
        //        return Ok(new
        //        {
        //            isSuccess = false,
        //            message = "Thất bại" + ". Đã có lỗi xảy ra",
        //            data = new List<Entities.Models.Order>()
        //        });
        //    }
        //}



        //[HttpPost]
        //public async Task<IActionResult> GetTourPackageOption(long serviceId, long orderId, int serviceType = 0)
        //{
        //    try
        //    {
        //        var listTourPackage = _tourPackagesOptionalRepository.GetTourPackagesOptional(serviceId).Result;
        //        listTourPackage = listTourPackage.Where(n => n.SupplierId != null && n.SupplierId != 0 && n.Status != 1).ToList();
        //        var listTourPackageReturn = new List<TourPackagesOptionalViewModel>();
        //        foreach (var item in listTourPackage)
        //        {
        //            var listContractPay = _contractPayRepository.GetContractPayBySupplierId(orderId, item.Id, (int)ServiceType.BOOK_HOTEL_ROOM_VIN);
        //            var listContractPays = listContractPay.Where(n => (n.IsDelete == null || n.IsDelete.Value == false)
        //                   && n.Type == ContractType.TT_NCC_HT && n.SupplierId == item.SupplierId).ToList();
        //            item.TotalContractPay = listContractPays.Sum(n => n.AmountPayDetail);
        //            var getBySupplierId = listTourPackageReturn.Where(n => n.SupplierId != null && n.SupplierId == item.SupplierId).ToList();
        //            if (getBySupplierId.Count > 0)
        //            {
        //                foreach (var sup in listTourPackageReturn)
        //                {
        //                    if (sup.SupplierId == item.SupplierId)
        //                    {
        //                        sup.Amount += item.Amount;
        //                        sup.TotalContractPay += item.TotalContractPay;
        //                    }
        //                }
        //            }
        //            else
        //            {
        //                listTourPackageReturn.Add(item);
        //            }
        //        }
        //        //var listContractPay = _contractPayRepository.GetContractPayBySupplierId(orderId, serviceId, serviceType);
        //        //var listContractPaySub = _contractPayRepository.GetContractPayBySupplierId(orderId, serviceId, (int)SubServiceType.Tour);
        //        //foreach (var item in listTourPackageReturn)
        //        //{
        //        //    listContractPay.AsParallel().ForAll(n =>
        //        //    {
        //        //        //if (item.SupplierId == n.SupplierId)
        //        //        item.Amount += n.AmountPayDetail;
        //        //    });
        //        //    listContractPaySub.AsParallel().ForAll(n =>
        //        //    {
        //        //        //if (item.SupplierId == n.SupplierId)
        //        //        item.Amount += n.AmountPayDetail;
        //        //    });
        //        //}
        //        var listPaymentRequest = _paymentRequestRepository.GetByServiceId(serviceId, serviceType);
        //        if (listPaymentRequest.Count > 0)
        //        {
        //            //var listSupplierId = listPaymentRequest.Select(n => n.SupplierId).ToList();
        //            //listTourPackage = listTourPackage.Where(n => !listSupplierId.Contains(n.SupplierId.Value)).ToList();
        //            foreach (var item in listTourPackageReturn)
        //            {
        //                var paymentRequests = listPaymentRequest.Where(n => (n.IsDelete == null || n.IsDelete.Value == false)
        //                    && n.Status != (int)PAYMENT_REQUEST_STATUS.TU_CHOI && n.SupplierId == item.SupplierId).ToList();
        //                item.TotalAmountPay = paymentRequests.Sum(n => n.Amount);
        //            }
        //        }
        //        return Ok(new
        //        {
        //            isSuccess = true,
        //            message = "Thành công",
        //            data = listTourPackageReturn
        //        });
        //    }
        //    catch (Exception ex)
        //    {
        //        LogHelper.InsertLogTelegram("GetTourPackageOption - PaymentRequestController: " + ex + ". Đã có lỗi xảy ra");
        //        return Ok(new
        //        {
        //            isSuccess = false,
        //            message = "Thất bại" + ". Đã có lỗi xảy ra",
        //            data = new List<Entities.Models.Order>()
        //        });
        //    }
        //}

        //public async Task<string> GetTourPackageOptionSuggest(string name, long serviceId)
        //{
        //    try
        //    {
        //        var listTourPackage = _tourPackagesOptionalRepository.GetTourPackagesOptional(serviceId).Result;
        //        if (listTourPackage == null) listTourPackage = new List<Entities.ViewModels.Tour.TourPackagesOptionalViewModel>();
        //        var listTourPackageReturn = new List<TourPackagesOptionalViewModel>();
        //        foreach (var item in listTourPackage.Where(s => s.Status != 1).ToList())
        //        {
        //            var getBySupplierId = listTourPackageReturn.Where(n => n.SupplierId != null && n.SupplierId == item.SupplierId).ToList();
        //            if (getBySupplierId.Count == 0)
        //            {
        //                listTourPackageReturn.Add(item);
        //            }
        //        }
        //        if (!string.IsNullOrEmpty(name))
        //        {
        //            listTourPackageReturn = listTourPackageReturn.Where(n =>
        //            n.SupplierName.Trim().ToLower().Contains(name.Trim().ToLower())
        //            || (!string.IsNullOrEmpty(n.Email) && n.Email.Trim().ToLower().Contains(name.Trim().ToLower()))
        //            || (!string.IsNullOrEmpty(n.Phone)) && n.Phone.Trim().ToLower().Contains(name.Trim().ToLower())).ToList();
        //        }
        //        var suggestionlist = listTourPackageReturn.Select(s => new SupplierViewModel
        //        {
        //            id = s.SupplierId.Value,
        //            fullname = s.SupplierName,
        //        }).ToList();
        //        return JsonConvert.SerializeObject(suggestionlist);
        //    }
        //    catch (Exception ex)
        //    {
        //        LogHelper.InsertLogTelegram("GetTourPackageOptionSuggest - PaymentRequestController: " + ex + ". Đã có lỗi xảy ra");
        //        return null;
        //    }
        //}

        //[HttpPost]
        //public async Task<IActionResult> GetOtherPackageOption(long serviceId, long orderId)
        //{
        //    try
        //    {
        //        var listOtherPackage = _otherBookingRepository.GetOtherBookingPackagesOptionalByServiceId(serviceId);
        //        listOtherPackage = listOtherPackage.Where(n => n.SuplierId != 0 && n.Status != 1).ToList();
        //        var listOtherPackageReturn = new List<OtherBookingPackagesOptionalViewModel>();
        //        foreach (var item in listOtherPackage)
        //        {
        //            var listContractPay = _contractPayRepository.GetContractPayBySupplierId(orderId, item.Id, (int)ServiceType.BOOK_HOTEL_ROOM_VIN);
        //            var listContractPays = listContractPay.Where(n => (n.IsDelete == null || n.IsDelete.Value == false)
        //                   && n.Type == ContractType.TT_NCC_HT && n.SupplierId == item.SuplierId).ToList();
        //            item.TotalContractPay = listContractPays.Sum(n => n.AmountPayDetail);
        //            var getBySupplierId = listOtherPackageReturn.Where(n => n.SuplierId == item.SuplierId).ToList();
        //            if (getBySupplierId.Count > 0)
        //            {
        //                foreach (var sup in listOtherPackageReturn)
        //                {
        //                    if (sup.SuplierId == item.SuplierId)
        //                    {
        //                        sup.Amount += item.Amount;
        //                        sup.TotalContractPay += item.TotalContractPay;
        //                    }
        //                }
        //            }
        //            else
        //            {
        //                listOtherPackageReturn.Add(item);
        //            }
        //        }
        //        //var listContractPay = _contractPayRepository.GetContractPayBySupplierId(orderId, serviceId, (int)ServiceType.Other);
        //        //var listContractPaySub = _contractPayRepository.GetContractPayBySupplierId(orderId, serviceId, (int)SubServiceType.Other);
        //        //foreach (var item in listOtherPackageReturn)
        //        //{
        //        //    listContractPay.AsParallel().ForAll(n =>
        //        //    {
        //        //        //if (item.SuplierId == n.SupplierId)
        //        //        item.Amount += n.AmountPayDetail;
        //        //    });
        //        //    listContractPaySub.AsParallel().ForAll(n =>
        //        //    {
        //        //        //if (item.SuplierId == n.SupplierId)
        //        //        item.Amount += n.AmountPayDetail;
        //        //    });
        //        //}
        //        var listPaymentRequest = _paymentRequestRepository.GetByServiceId(serviceId, (int)ServiceType.Other);
        //        if (listPaymentRequest.Count > 0)
        //        {
        //            foreach (var item in listOtherPackageReturn)
        //            {
        //                var paymentRequests = listPaymentRequest.Where(n => (n.IsDelete == null || n.IsDelete.Value == false)
        //                    && n.Status != (int)PAYMENT_REQUEST_STATUS.TU_CHOI && n.SupplierId == item.SuplierId).ToList();
        //                item.TotalAmountPay = paymentRequests.Sum(n => n.Amount);
        //            }
        //        }
        //        return Ok(new
        //        {
        //            isSuccess = true,
        //            message = "Thành công",
        //            data = listOtherPackageReturn
        //        });
        //    }
        //    catch (Exception ex)
        //    {
        //        LogHelper.InsertLogTelegram("GetOtherPackageOption - PaymentRequestController: " + ex + ". Đã có lỗi xảy ra");
        //        return Ok(new
        //        {
        //            isSuccess = false,
        //            message = "Thất bại" + ". Đã có lỗi xảy ra",
        //            data = new List<Entities.Models.Order>()
        //        });
        //    }
        //}

        //public async Task<string> GetOtherPackageOptionSuggest(string name, long serviceId)
        //{
        //    try
        //    {
        //        var listOtherPackage = _otherBookingRepository.GetOtherBookingPackagesOptionalByServiceId(serviceId);
        //        if (listOtherPackage == null) listOtherPackage = new List<OtherBookingPackagesOptionalViewModel>();
        //        var listOtherPackageReturn = new List<OtherBookingPackagesOptionalViewModel>();
        //        foreach (var item in listOtherPackage.Where(s => s.Status != 1).ToList())
        //        {
        //            var getBySupplierId = listOtherPackageReturn.Where(n => n.SuplierId == item.SuplierId).ToList();
        //            if (getBySupplierId.Count == 0)
        //            {
        //                listOtherPackageReturn.Add(item);
        //            }
        //        }
        //        if (!string.IsNullOrEmpty(name))
        //        {
        //            listOtherPackageReturn = listOtherPackageReturn.Where(n =>
        //            n.SupplierName.Trim().ToLower().Contains(name.Trim().ToLower())
        //            || (!string.IsNullOrEmpty(n.Email) && n.Email.Trim().ToLower().Contains(name.Trim().ToLower()))
        //            || (!string.IsNullOrEmpty(n.Phone)) && n.Phone.Trim().ToLower().Contains(name.Trim().ToLower())).ToList();
        //        }
        //        var suggestionlist = listOtherPackageReturn.Select(s => new SupplierViewModel
        //        {
        //            id = (int)s.SuplierId,
        //            fullname = s.SupplierName,
        //        }).ToList();
        //        return JsonConvert.SerializeObject(suggestionlist);
        //    }
        //    catch (Exception ex)
        //    {
        //        LogHelper.InsertLogTelegram("GetOtherPackageOptionSuggest - PaymentRequestController: " + ex + ". Đã có lỗi xảy ra");
        //        return null;
        //    }
        //}

        //public async Task<string> GetUserSuggestionList(string name)
        //{
        //    try
        //    {
        //        var userlist = await _userRepository.GetUserSuggestionList(name);
        //        var current_user = _ManagementUser.GetCurrentUser();
        //        if (current_user != null && !string.IsNullOrEmpty(current_user.UserUnderList))
        //        {
        //            var listUserId = current_user.UserUnderList.Split(',').Select(n => int.Parse(n)).ToList();
        //            if (listUserId.Count > 0)
        //                userlist = userlist.Where(n => listUserId.Contains(n.Id)).ToList();
        //        }
        //        var suggestionlist = userlist.Select(s => new
        //        {
        //            id = s.Id,
        //            name = s.UserName,
        //            fullname = s.FullName,
        //            email = s.Email
        //        }).ToList();
        //        return JsonConvert.SerializeObject(suggestionlist);
        //    }
        //    catch (Exception ex)
        //    {
        //        LogHelper.InsertLogTelegram("GetUserSuggestionList - PaymentRequestController: " + ex + ". Đã có lỗi xảy ra");
        //        return null;
        //    }
        //}

        //[HttpPost]
        //public async Task<IActionResult> GetHotelPackageOption(long serviceId, long orderId)
        //{
        //    try
        //    {
        //        var listHotelPackages = _hotelBookingRepositories.GetHotelBookingOptionalListByHotelBookingId(serviceId).Result;
        //        var listHotelExtraPackages = _hotelBookingRepositories.GetListHotelBookingRoomExtraPackagesHotelBookingId(serviceId).Result;
        //        foreach (var item in listHotelExtraPackages)
        //        {
        //            item.Amount = item.UnitPrice;
        //            item.TotalAmount = item.UnitPrice;
        //            listHotelPackages.Add(item);
        //        }
        //        var listHotelPackageReturn = new List<HotelBookingsRoomOptionalViewModel>();
        //        foreach (var item in listHotelPackages)
        //        {
        //            var listContractPay = _contractPayRepository.GetContractPayBySupplierId(orderId, item.Id, (int)ServiceType.BOOK_HOTEL_ROOM_VIN);
        //            var listContractPays = listContractPay.Where(n => (n.IsDelete == null || n.IsDelete.Value == false)
        //                   && n.Type == ContractType.TT_NCC_HT && n.SupplierId == item.SupplierId).ToList();
        //            item.TotalContractPay = listContractPays.Sum(n => n.AmountPayDetail);
        //            item.Amount = item.TotalAmount;
        //            var getBySupplierId = listHotelPackageReturn.Where(n => n.SupplierId == item.SupplierId).ToList();
        //            if (getBySupplierId.Count > 0)
        //            {
        //                foreach (var sup in listHotelPackageReturn)
        //                {
        //                    if (sup.SupplierId == item.SupplierId)
        //                    {
        //                        sup.TotalAmount += item.TotalAmount;
        //                        sup.Amount += item.Amount;
        //                        sup.TotalContractPay += item.TotalContractPay;
        //                    }
        //                }
        //            }
        //            else
        //            {
        //                listHotelPackageReturn.Add(item);
        //            }
        //        }
        //        //var listContractPay = _contractPayRepository.GetContractPayBySupplierId(orderId, serviceId, (int)ServiceType.BOOK_HOTEL_ROOM_VIN);
        //        //var listContractPaySub = _contractPayRepository.GetContractPayBySupplierId(orderId, serviceId, (int)SubServiceType.BOOK_HOTEL_ROOM_VIN);
        //        //foreach (var item in listHotelPackageReturn)
        //        //{
        //        //    listContractPay.AsParallel().ForAll(n =>
        //        //    {
        //        //        //if (item.SupplierId == n.SupplierId)
        //        //        item.Amount += n.AmountPayDetail;
        //        //    });
        //        //    listContractPaySub.AsParallel().ForAll(n =>
        //        //    {
        //        //        //if (item.SupplierId == n.SupplierId)
        //        //        item.Amount += n.AmountPayDetail;
        //        //    });
        //        //}
        //        var listPaymentRequest = _paymentRequestRepository.GetByServiceId(serviceId, (int)ServiceType.BOOK_HOTEL_ROOM_VIN);

        //        if (listPaymentRequest.Count > 0)
        //        {


        //            foreach (var item in listHotelPackageReturn)
        //            {
        //                var paymentRequests = listPaymentRequest.Where(n => (n.IsDelete == null || n.IsDelete.Value == false)
        //                    && n.Status != (int)PAYMENT_REQUEST_STATUS.TU_CHOI && n.SupplierId == item.SupplierId).ToList();

        //                item.TotalAmountPay = paymentRequests.Sum(n => n.Amount);

        //            }
        //        }
        //        return Ok(new
        //        {
        //            isSuccess = true,
        //            message = "Thành công",
        //            data = listHotelPackageReturn
        //        });
        //    }
        //    catch (Exception ex)
        //    {
        //        LogHelper.InsertLogTelegram("GetHotelPackageOption - PaymentRequestController: " + ex + ". Đã có lỗi xảy ra");
        //        return Ok(new
        //        {
        //            isSuccess = false,
        //            message = "Thất bại" + ". Đã có lỗi xảy ra",
        //            data = new List<Entities.Models.Order>()
        //        });
        //    }
        //}

        //public async Task<string> GetHotelPackageOptionSuggest(string name, long serviceId)
        //{
        //    try
        //    {

        //        var listHotelPackages = _hotelBookingRepositories.GetHotelBookingOptionalListByHotelBookingId(serviceId).Result;
        //        var listHotelExtraPackages = _hotelBookingRepositories.GetListHotelBookingRoomExtraPackagesHotelBookingId(serviceId).Result;
        //        foreach (var item in listHotelExtraPackages)
        //        {
        //            item.Amount = item.UnitPrice;
        //            item.TotalAmount = item.UnitPrice;
        //            listHotelPackages.Add(item);
        //        }
        //        var listHotelPackageReturn = new List<HotelBookingsRoomOptionalViewModel>();
        //        foreach (var item in listHotelPackages)
        //        {
        //            var getBySupplierId = listHotelPackageReturn.Where(n => n.SupplierId == item.SupplierId).ToList();
        //            if (getBySupplierId.Count == 0)
        //            {
        //                listHotelPackageReturn.Add(item);
        //            }
        //        }
        //        if (listHotelPackageReturn == null) listHotelPackageReturn = new List<HotelBookingsRoomOptionalViewModel>();
        //        if (!string.IsNullOrEmpty(name))
        //        {
        //            listHotelPackageReturn = listHotelPackageReturn.Where(n =>
        //            n.SupplierName.Trim().ToLower().Contains(name.Trim().ToLower())
        //            || (!string.IsNullOrEmpty(n.Email) && n.Email.Trim().ToLower().Contains(name.Trim().ToLower()))
        //            || (!string.IsNullOrEmpty(n.Phone)) && n.Phone.Trim().ToLower().Contains(name.Trim().ToLower())).ToList();
        //        }
        //        var suggestionlist = listHotelPackageReturn.Select(s => new SupplierViewModel
        //        {
        //            id = (int)s.SupplierId,
        //            fullname = s.SupplierName,
        //        }).ToList();
        //        return JsonConvert.SerializeObject(suggestionlist);
        //    }
        //    catch (Exception ex)
        //    {
        //        LogHelper.InsertLogTelegram("GetHotelPackageOptionSuggest - PaymentRequestController: " + ex + ". Đã có lỗi xảy ra");
        //        return null;
        //    }
        //}

        //[HttpPost]
        //public async Task<IActionResult> GetFlyBookingPackagesOptional(long serviceId, long orderId)
        //{
        //    try
        //    {
        //        var listFlyBookingPackagesOptional = _flyBookingDetailRepository.GetFlyBookingPackagesOptionalsByBookingId(serviceId).Result;
        //        var listFlyBookingPackagesOptionalReturn = new List<FlyBookingPackagesOptionalViewModel>();
        //        foreach (var item in listFlyBookingPackagesOptional)
        //        {
        //            var listContractPay = _contractPayRepository.GetContractPayBySupplierId(orderId, item.Id, (int)ServiceType.BOOK_HOTEL_ROOM_VIN);
        //            var listContractPays = listContractPay.Where(n => (n.IsDelete == null || n.IsDelete.Value == false)
        //                   && n.Type == ContractType.TT_NCC_HT && n.SupplierId == item.SupplierId).ToList();
        //            item.TotalContractPay = listContractPays.Sum(n => n.AmountPayDetail);
        //            var getBySupplierId = listFlyBookingPackagesOptionalReturn.Where(n => n.SuplierId == item.SuplierId && n.Status != 1).ToList();
        //            if (getBySupplierId.Count > 0)
        //            {
        //                foreach (var sup in listFlyBookingPackagesOptionalReturn)
        //                {
        //                    if (sup.SuplierId == item.SuplierId)
        //                    {
        //                        sup.Amount += item.Amount;
        //                        sup.TotalContractPay += item.TotalContractPay;
        //                    }
        //                }
        //            }
        //            else
        //            {
        //                listFlyBookingPackagesOptionalReturn.Add(item);
        //            }
        //        }
        //        //var listContractPay = _contractPayRepository.GetContractPayBySupplierId(orderId, serviceId, (int)ServiceType.PRODUCT_FLY_TICKET);
        //        //var listContractPaySub = _contractPayRepository.GetContractPayBySupplierId(orderId, serviceId, (int)SubServiceType.PRODUCT_FLY_TICKET);
        //        //foreach (var item in listFlyBookingPackagesOptionalReturn)
        //        //{
        //        //    listContractPay.AsParallel().ForAll(n =>
        //        //    {
        //        //        //if (item.SupplierId == n.SupplierId)
        //        //        item.Amount += n.AmountPayDetail;
        //        //    });
        //        //    listContractPaySub.AsParallel().ForAll(n =>
        //        //    {
        //        //        //if (item.SupplierId == n.SupplierId)
        //        //        item.Amount += n.AmountPayDetail;
        //        //    });
        //        //}
        //        var listPaymentRequest = _paymentRequestRepository.GetByServiceId(serviceId, (int)ServiceType.BOOK_HOTEL_ROOM_VIN);
        //        if (listPaymentRequest.Count > 0)
        //        {
        //            foreach (var item in listFlyBookingPackagesOptionalReturn)
        //            {
        //                var paymentRequests = listPaymentRequest.Where(n => (n.IsDelete == null || n.IsDelete.Value == false)
        //                    && n.Status != (int)PAYMENT_REQUEST_STATUS.TU_CHOI && n.SupplierId == item.SuplierId).ToList();
        //                item.TotalAmountPay = paymentRequests.Sum(n => n.Amount);
        //            }
        //        }
        //        return Ok(new
        //        {
        //            isSuccess = true,
        //            message = "Thành công",
        //            data = listFlyBookingPackagesOptionalReturn
        //        });
        //    }
        //    catch (Exception ex)
        //    {
        //        LogHelper.InsertLogTelegram("GetFlyBookingPackagesOptional - PaymentRequestController: " + ex + ". Đã có lỗi xảy ra");
        //        return Ok(new
        //        {
        //            isSuccess = false,
        //            message = "Thất bại" + ". Đã có lỗi xảy ra",
        //            data = new List<Entities.Models.Order>()
        //        });
        //    }
        //}

        //public async Task<string> GetFlyBookingPackagesOptionalSuggest(string name, long serviceId)
        //{
        //    try
        //    {
        //        var listFlyBookingPackagesOptional = _flyBookingDetailRepository.GetFlyBookingPackagesOptionalsByBookingId(serviceId).Result;
        //        var listFlyBookingPackagesOptionalReturn = new List<FlyBookingPackagesOptionalViewModel>();
        //        foreach (var item in listFlyBookingPackagesOptional.Where(s => s.Status != 1).ToList())
        //        {
        //            var getBySupplierId = listFlyBookingPackagesOptionalReturn.Where(n => n.SuplierId == item.SuplierId).ToList();
        //            if (getBySupplierId.Count == 0)
        //            {
        //                listFlyBookingPackagesOptionalReturn.Add(item);
        //            }
        //        }
        //        if (listFlyBookingPackagesOptionalReturn == null) listFlyBookingPackagesOptionalReturn = new List<FlyBookingPackagesOptionalViewModel>();
        //        if (!string.IsNullOrEmpty(name))
        //        {
        //            listFlyBookingPackagesOptionalReturn = listFlyBookingPackagesOptionalReturn.Where(n =>
        //            n.SupplierName.Trim().ToLower().Contains(name.Trim().ToLower())
        //            || (!string.IsNullOrEmpty(n.Email) && n.Email.Trim().ToLower().Contains(name.Trim().ToLower()))
        //            || (!string.IsNullOrEmpty(n.Phone)) && n.Phone.Trim().ToLower().Contains(name.Trim().ToLower())).ToList();
        //        }
        //        var suggestionlist = listFlyBookingPackagesOptionalReturn.Select(s => new SupplierViewModel
        //        {
        //            id = (int)s.SuplierId,
        //            fullname = s.SupplierName,
        //        }).ToList();
        //        return JsonConvert.SerializeObject(suggestionlist);
        //    }
        //    catch (Exception ex)
        //    {
        //        LogHelper.InsertLogTelegram("GetFlyBookingPackagesOptionalSuggest - PaymentRequestController: " + ex + ". Đã có lỗi xảy ra");
        //        return null;
        //    }
        //}



        //public IActionResult UndoApprove(int status)
        //{
        //    ViewBag.status = status;
        //    return PartialView();
        //}

        //[HttpPost]
        //public async Task<IActionResult> UndoApproveRequest(string paymentRequestNo, string note, int status)
        //{
        //    try
        //    {
        //        if (!string.IsNullOrEmpty(note) && note.Length > 300)
        //        {
        //            return Ok(new
        //            {
        //                isSuccess = false,
        //                message = "Nội dung bỏ duyệt không được lớn hơn 300 kí tự"
        //            });
        //        }
        //        long _UserId = 0;
        //        if (HttpContext.User.FindFirst(ClaimTypes.NameIdentifier) != null)
        //        {
        //            _UserId = Convert.ToInt64(HttpContext.User.FindFirst(ClaimTypes.NameIdentifier).Value);
        //        }
        //        var current_user = _ManagementUser.GetCurrentUser();
        //        var model = _paymentRequestRepository.GetByRequestNo(paymentRequestNo);
        //        //int status = GetStatusUpdate(model, false);
        //        var result = _paymentRequestRepository.UndoApprove(paymentRequestNo, note, (int)_UserId, status);
        //        if (result < 1)
        //            return Ok(new
        //            {
        //                isSuccess = false,
        //                message = "Bỏ duyệt yêu cầu chi thất bại"
        //            });

        //        string link = "/PaymentRequest/Detail?paymentRequestId=" + model.Id;
        //        apiService.SendMessage(_UserId.ToString(), ((int)ModuleType.PHIEU_YEU_CAU_CHI).ToString(), ((int)Utilities.Contants.ActionType.BO_DUYET_YEU_CAU_CHI).ToString(), model.PaymentCode, link, current_user == null ? "0" : current_user.Role);

        //        return Ok(new
        //        {
        //            isSuccess = true,
        //            message = "Bỏ duyệt yêu cầu chi thành công"
        //        });
        //    }
        //    catch (Exception ex)
        //    {
        //        LogHelper.InsertLogTelegram("Update - PaymentRequestController: " + ex + ". Đã có lỗi xảy ra");
        //        return Ok(new
        //        {
        //            isSuccess = false,
        //            message = "Bỏ duyệt yêu cầu chi thất bại. Đã có lỗi xảy ra"
        //        });
        //    }
        //}

        //private int GetStatusUpdate(PaymentRequest model, bool isApprove = true)
        //{
        //    var status = model.Status != null ? 0 : model.Status.Value;
        //    var current_user = _ManagementUser.GetCurrentUser();

        //    //check user permission
        //    if (isApprove)
        //    {
        //        if (((string)config.ROLE_KE_TOAN_TRUONG).Contains(current_user.Role))
        //            status = (int)PAYMENT_REQUEST_STATUS.CHO_CHI;
        //        else
        //            status = (int)PAYMENT_REQUEST_STATUS.CHO_KTT_DUYET;
        //    }
        //    else
        //    {
        //        if (!((string)config.ROLE_KE_TOAN_TRUONG).Contains(current_user.Role))
        //            status = (int)PAYMENT_REQUEST_STATUS.CHO_TBP_DUYET;
        //        else
        //            status = (int)PAYMENT_REQUEST_STATUS.CHO_KTT_DUYET;
        //    }
        //    return status;
        //}

        //public async Task<IActionResult> InYCChi(int id, int type)
        //{

        //    try
        //    {
        //        ViewBag.type = type;
        //        ViewBag.id = id;
        //        ViewBag.ngay = "Ngày " + DateTime.Now.Day.ToString() + ',' + "Tháng " + DateTime.Now.Month.ToString() + ',' + "Năm " + DateTime.Now.Year.ToString();

        //        ViewBag.model = new PaymentRequestViewModel();
        //        if (id > 0)
        //        {
        //            var model = _paymentRequestRepository.GetById(id);
        //            var client = await _clientRepository.GetClientDetailByClientId((long)model.ClientId);
        //            ViewBag.ClientName = client.ClientName;
        //            ViewBag.model = model;

        //        }

        //    }
        //    catch (Exception ex)
        //    {
        //        LogHelper.InsertLogTelegram("SendYCChi - SetServiceController: " + ex);
        //    }

        //    return PartialView();
        //}

        public async Task<IActionResult> AddPaymentVoucher(int paymentRequestId)
        {
            ViewBag.allCode_PAY_TYPE = _allCodeRepository.GetListByType(AllCodeType.PAY_TYPE);
            ViewBag.allCode_PAYMENT_VOUCHER_TYPE = _allCodeRepository.GetListByType(AllCodeType.PAYMENT_VOUCHER_TYPE);
            ViewBag.listBankingAccount = _allCodeRepository.GetBankingAccounts();
            ViewBag.listBankingAccountAdavigo = _allCodeRepository.GetBankingAccounts().Where(n => n.SupplierId == (long)config.SUPPLIERID_ADAVIGO).ToList();
            ViewBag.ClientId = 0;
            ViewBag.SupplierId = 0;

            var model = _paymentRequestRepository.GetById(paymentRequestId);
            if (model.RelateData == null) model.RelateData = new List<PaymentRequestDetailViewModel>();

            ViewBag.ClientId = model.ClientId == null ? 0 : model.ClientId;

            ViewBag.SupplierId = model.SupplierId == null ? 0 : model.SupplierId;

            ViewBag.type = model.Type;
            ViewBag.PaymentType = model.PaymentType;
            ViewBag.Note = model.Note;
            ViewBag.code = model.PaymentCode;
            ViewBag.id = paymentRequestId;
            ViewBag.ClientName = model.ClientName;
            ViewBag.SupplierName = model.SupplierName;

            return PartialView(model);
        }
        //public async Task<IActionResult> PopupNoteKT(int id, int noteId)
        //{
        //    ViewBag.id = id;
        //    ViewBag.noteId = 0;
        //    var data = new NoteViewModel();
        //    if (noteId > 0)
        //    {
        //        var list_note = await _noteRepository.GetListByType(id, (int)AttachmentType.YCC_Comment);
        //        if (list_note != null && list_note.Count > 0)
        //        {
        //            var detail = list_note.FirstOrDefault(s => s.Id == noteId);
        //            data = detail;
        //            ViewBag.noteId = data.Id;
        //        }
        //    }
        //    return PartialView(data);
        //}
        //public async Task<IActionResult> SetUpNoteKT(int id, string notekt, int noteid)
        //{
        //    try
        //    {
        //        if (!string.IsNullOrEmpty(notekt) && notekt.Length > 300)
        //        {
        //            return Ok(new
        //            {
        //                isSuccess = false,
        //                message = "Nội dung ghi chú không được lớn hơn 300 kí tự"
        //            });
        //        }
        //        int _UserId = 0;

        //        if (HttpContext.User.FindFirst(ClaimTypes.NameIdentifier) != null)
        //        {
        //            _UserId = Convert.ToInt32(HttpContext.User.FindFirst(ClaimTypes.NameIdentifier).Value);
        //        }
        //        var model = new Note();
        //        model.Id = noteid;
        //        model.Comment = notekt;
        //        model.DataId = id;
        //        model.UserId = _UserId;
        //        model.Type = (int?)AttachmentType.YCC_Comment;
        //        var setup = await _noteRepository.UpSert(model);
        //        if (setup > 0)
        //        {
        //            return Ok(new
        //            {
        //                isSuccess = true,
        //                message = "Ghi chú thành công"
        //            });
        //        }

        //        return Ok(new
        //        {
        //            isSuccess = false,
        //            message = "Lưu ghi chú không thành công"
        //        });
        //    }
        //    catch (Exception ex)
        //    {
        //        LogHelper.InsertLogTelegram("Update - PaymentRequestController: " + ex + ". Đã có lỗi xảy ra");
        //        return Ok(new
        //        {
        //            isSuccess = false,
        //            message = "Lưu ghi chú thất bại. Đã có lỗi xảy ra"
        //        });
        //    }
        //}
        //public async Task<IActionResult> GetVinwonderPackageOption(long serviceId, long orderId, int serviceType = 0)
        //{
        //    try
        //    {
        //        var listVinwonderTicket = await _vinWonderBookingRepository.GetVinWonderTicketByBookingIdSP(serviceId);
        //        listVinwonderTicket = listVinwonderTicket.Where(n => n.SupplierId != null && n.SupplierId != 0).ToList();
        //        var listVinwonderTicketReturn = new List<VinWonderBookingTicketViewModel>();
        //        foreach (var item in listVinwonderTicket)
        //        {
        //            var listContractPay = _contractPayRepository.GetContractPayBySupplierId(orderId, item.Id, (int)ServiceType.BOOK_HOTEL_ROOM_VIN);
        //            var listContractPays = listContractPay.Where(n => (n.IsDelete == null || n.IsDelete.Value == false)
        //                   && n.Type == ContractType.TT_NCC_HT && n.SupplierId == item.SupplierId).ToList();
        //            item.TotalContractPay = listContractPays.Sum(n => n.AmountPayDetail);
        //            item.Amount = item.BasePrice;
        //            var getBySupplierId = listVinwonderTicketReturn.Where(n => n.SupplierId != null && n.SupplierId == item.SupplierId).ToList();
        //            if (getBySupplierId.Count > 0)
        //            {
        //                foreach (var sup in listVinwonderTicketReturn)
        //                {
        //                    if (sup.SupplierId == item.SupplierId)
        //                    {
        //                        sup.Amount += item.Amount;
        //                        sup.TotalContractPay += item.TotalContractPay;
        //                    }
        //                }
        //            }
        //            else
        //            {
        //                listVinwonderTicketReturn.Add(item);
        //            }
        //        }

        //        var listPaymentRequest = _paymentRequestRepository.GetByServiceId(serviceId, serviceType);
        //        if (listPaymentRequest.Count > 0)
        //        {
        //            foreach (var item in listVinwonderTicketReturn)
        //            {
        //                var paymentRequests = listPaymentRequest.Where(n => (n.IsDelete == null || n.IsDelete.Value == false)
        //                    && n.Status != (int)PAYMENT_REQUEST_STATUS.TU_CHOI && n.SupplierId == item.SupplierId).ToList();
        //                item.TotalAmountPay = paymentRequests.Sum(n => n.Amount);
        //            }
        //        }
        //        return Ok(new
        //        {
        //            isSuccess = true,
        //            message = "Thành công",
        //            data = listVinwonderTicketReturn
        //        });
        //    }
        //    catch (Exception ex)
        //    {
        //        LogHelper.InsertLogTelegram("GetVinwonderPackageOption - PaymentRequestController: " + ex + ". Đã có lỗi xảy ra");
        //        return Ok(new
        //        {
        //            isSuccess = false,
        //            message = "Thất bại" + ". Đã có lỗi xảy ra",
        //            data = new List<Entities.Models.Order>()
        //        });
        //    }
        //}
        //public async Task<string> GetVinwonderPackagesOptionalSuggest(string name, long serviceId)
        //{
        //    try
        //    {
        //        var supplierList = await _vinWonderBookingRepository.GetVinWonderTicketByBookingIdSP(serviceId);
        //        if (!string.IsNullOrEmpty(name))
        //        {
        //            supplierList = supplierList.Where(n =>
        //            n.SupplierName.Trim().ToLower().Contains(name.Trim().ToLower())).ToList();
        //        }

        //        var suggestionlist = supplierList.Select(s => new SupplierViewModel
        //        {
        //            id = (int)s.SupplierId,
        //            fullname = s.SupplierName,

        //        }).ToList();

        //        suggestionlist = suggestionlist.GroupBy(s => s.id).Select(s => s.First()).ToList();
        //        return JsonConvert.SerializeObject(suggestionlist);

        //    }
        //    catch (Exception ex)
        //    {
        //        LogHelper.InsertLogTelegram("GetFlyBookingPackagesOptionalSuggest - PaymentRequestController: " + ex + ". Đã có lỗi xảy ra");
        //        return null;
        //    }

        //}
    }
}
