//using System;
//using System.Collections.Generic;
//using System.IO;
//using System.Linq;
//using System.Net.Http;
//using System.Security.Claims;
//using System.Threading.Tasks;
//using Entities.ViewModels;
//using Entities.ViewModels.Funding;
//using Microsoft.AspNetCore.Hosting;
//using Microsoft.AspNetCore.Mvc;
//using Microsoft.Extensions.Configuration;
//using Newtonsoft.Json;
//using Newtonsoft.Json.Linq;
//using Repositories.IRepositories;
//using Utilities;
//using Utilities.Contants;
//using WEB.Adavigo.CMS.Service;
//using WEB.CMS.Customize;
//using WEB.CMS.Models;

//namespace WEB.Adavigo.CMS.Controllers.Funding
//{
//    public class DebtStatisticController : Controller
//    {
//        private ManagementUser _ManagementUser;
//        private IDebtStatisticRepository _debtStatisticRepository;
//        private readonly IAllCodeRepository _allCodeRepository;
//        private readonly IWebHostEnvironment _WebHostEnvironment;
//        private readonly IPaymentRequestRepository _paymentRequestRepository;
//        private readonly IUserRepository _userRepository;
//        private APIService apiService;

//        public DebtStatisticController(IAllCodeRepository allCodeRepository, IWebHostEnvironment hostEnvironment, ManagementUser ManagementUser,
//            IDebtStatisticRepository debtStatisticRepository, IUserRepository userRepository, IConfiguration configuration, IPaymentRequestRepository paymentRequestRepository)
//        {
//            _ManagementUser = ManagementUser;
//            _debtStatisticRepository = debtStatisticRepository;
//            _allCodeRepository = allCodeRepository;
//            _paymentRequestRepository = paymentRequestRepository;
//            _WebHostEnvironment = hostEnvironment;
//            apiService = new APIService(configuration, userRepository);
//            _userRepository = userRepository;
//        }

//        public IActionResult Index()
//        {
//            ViewBag.allCode_DEBT_STATISTIC_STATUS = _allCodeRepository.GetListByType(AllCodeType.DEBT_STATISTIC_STATUS);
//            return View();
//        }

//        [HttpPost]
//        public IActionResult Search(DebtStatisticViewModel searchModel, int currentPage = 1, int pageSize = 20)
//        {
//            var model = new GenericViewModel<DebtStatisticViewModel>();
//            try
//            {
//                if (searchModel.CreateByIds == null) searchModel.CreateByIds = new List<int>();
//                if (searchModel.StatusMulti == null) searchModel.StatusMulti = new List<int>();
//                if (searchModel.Status > -1 && searchModel.Status != null)
//                    searchModel.StatusMulti.Add(searchModel.Status.Value);
//                if (!string.IsNullOrEmpty(searchModel.Code))
//                    searchModel.Code = searchModel.Code.Trim();
//                var current_user = _ManagementUser.GetCurrentUser();
//                if (searchModel.CreateByIds.Count == 0)
//                {
//                    if (!string.IsNullOrEmpty(current_user.UserUnderList))
//                        searchModel.CreateByIds = current_user.UserUnderList.Split(',').Select(n => int.Parse(n)).ToList();
//                }
//                var listDebtStatistic = _debtStatisticRepository.GetDebtStatistics(searchModel, out long total, currentPage, pageSize);
//                model.CurrentPage = currentPage;
//                model.ListData = listDebtStatistic;
//                model.PageSize = pageSize;
//                model.TotalRecord = total;
//                model.TotalPage = (int)Math.Ceiling((double)total / pageSize);
//            }
//            catch (Exception ex)
//            {
//                LogHelper.InsertLogTelegram("Search - DebtStatisticController: " + ex + ". Đã có lỗi xảy ra");
//            }
//            return PartialView(model);
//        }

//        [HttpPost]
//        public async Task<IActionResult> CountStatus(DebtStatisticViewModel searchModel)
//        {
//            try
//            {
//                if (searchModel.CreateByIds == null) searchModel.CreateByIds = new List<int>();
//                if (searchModel.StatusMulti == null) searchModel.StatusMulti = new List<int>();
//                if (searchModel.Status > -1 && searchModel.Status != null)
//                    searchModel.StatusMulti.Add(searchModel.Status.Value);
//                if (!string.IsNullOrEmpty(searchModel.Code))
//                    searchModel.Code = searchModel.Code.Trim();
//                var current_user = _ManagementUser.GetCurrentUser();
//                if (searchModel.CreateByIds.Count == 0)
//                {
//                    if (!string.IsNullOrEmpty(current_user.UserUnderList))
//                        searchModel.CreateByIds = current_user.UserUnderList.Split(',').Select(n => int.Parse(n)).ToList();
//                }
//                var countStatus = _debtStatisticRepository.GetCountStatus(searchModel);
//                var countStatusAll = new CountStatus();
//                countStatusAll.Status = -1;
//                countStatusAll.Total = countStatus.Sum(n => n.Total);
//                countStatus.Add(countStatusAll);
//                return Ok(new
//                {
//                    isSuccess = true,
//                    data = countStatus
//                });
//            }
//            catch (Exception ex)
//            {
//                LogHelper.InsertLogTelegram("CountStatus - DebtStatisticController: " + ex + ". Đã có lỗi xảy ra");
//                return Ok(new
//                {
//                    isSuccess = false,
//                    data = new List<CountStatus>()
//                });
//            }
//        }

//        [HttpPost]
//        public IActionResult ExportExcel(DebtStatisticViewModel searchModel)
//        {
//            try
//            {
//                string _FileName = "DebtStatistic_" + Guid.NewGuid() + ".xlsx";
//                string _UploadFolder = @"Template/Export";
//                string _UploadDirectory = Path.Combine(_WebHostEnvironment.WebRootPath, _UploadFolder);

//                if (!Directory.Exists(_UploadDirectory))
//                {
//                    Directory.CreateDirectory(_UploadDirectory);
//                }
//                //delete all file in folder before export
//                try
//                {
//                    System.IO.DirectoryInfo di = new DirectoryInfo(_UploadDirectory);
//                    foreach (FileInfo file in di.GetFiles())
//                    {
//                        file.Delete();
//                    }
//                }
//                catch
//                {
//                }
//                if (searchModel.CreateByIds == null) searchModel.CreateByIds = new List<int>();
//                if (searchModel.StatusMulti == null) searchModel.StatusMulti = new List<int>();
//                if (searchModel.Status > -1 && searchModel.Status != null)
//                    searchModel.StatusMulti.Add(searchModel.Status.Value);
//                if (!string.IsNullOrEmpty(searchModel.Code))
//                    searchModel.Code = searchModel.Code.Trim();
//                var current_user = _ManagementUser.GetCurrentUser();
//                if (searchModel.CreateByIds.Count == 0)
//                {
//                    if (!string.IsNullOrEmpty(current_user.UserUnderList))
//                        searchModel.CreateByIds = current_user.UserUnderList.Split(',').Select(n => int.Parse(n)).ToList();
//                }
//                string FilePath = Path.Combine(_UploadDirectory, _FileName);
//                var rsPath = _debtStatisticRepository.ExportDebtStatistic(searchModel, FilePath);

//                if (!string.IsNullOrEmpty(rsPath))
//                {
//                    return new JsonResult(new
//                    {
//                        isSuccess = true,
//                        message = "Xuất dữ liệu thành công",
//                        path = "/" + _UploadFolder + "/" + _FileName
//                    });
//                }
//                else
//                {
//                    return new JsonResult(new
//                    {
//                        isSuccess = false,
//                        message = "Xuất dữ liệu thất bại"
//                    });
//                }
//            }
//            catch (Exception ex)
//            {
//                return new JsonResult(new
//                {
//                    isSuccess = false,
//                    message = ex.Message.ToString() + ". Đã có lỗi xảy ra"
//                });
//            }
//        }

//        [HttpPost]
//        public IActionResult ExportExcelOrderList(DebtStatisticViewModel searchModel)
//        {
//            try
//            {
//                string _FileName = "DebtStatistic_" + Guid.NewGuid() + ".xlsx";
//                string _UploadFolder = @"Template/Export";
//                string _UploadDirectory = Path.Combine(_WebHostEnvironment.WebRootPath, _UploadFolder);

//                if (!Directory.Exists(_UploadDirectory))
//                {
//                    Directory.CreateDirectory(_UploadDirectory);
//                }
//                //delete all file in folder before export
//                try
//                {
//                    System.IO.DirectoryInfo di = new DirectoryInfo(_UploadDirectory);
//                    foreach (FileInfo file in di.GetFiles())
//                    {
//                        file.Delete();
//                    }
//                }
//                catch
//                {
//                }
//                string FilePath = Path.Combine(_UploadDirectory, _FileName);
//                var rsPath = _debtStatisticRepository.ExportDebtStatistic_OrderList(searchModel, FilePath);

//                if (!string.IsNullOrEmpty(rsPath))
//                {
//                    return new JsonResult(new
//                    {
//                        isSuccess = true,
//                        message = "Xuất dữ liệu thành công",
//                        path = "/" + _UploadFolder + "/" + _FileName
//                    });
//                }
//                else
//                {
//                    return new JsonResult(new
//                    {
//                        isSuccess = false,
//                        message = "Xuất dữ liệu thất bại"
//                    });
//                }
//            }
//            catch (Exception ex)
//            {
//                return new JsonResult(new
//                {
//                    isSuccess = false,
//                    message = ex.Message.ToString() + ". Đã có lỗi xảy ra"
//                });
//            }
//        }

//        public IActionResult Add()
//        {
//            return PartialView();
//        }

//        private int Validate(DebtStatisticViewModel model, out string message, bool isEdit = false)
//        {
//            message = string.Empty;
//            if (model.ClientId == 0)
//            {
//                message = "Vui lòng chọn khách hàng";
//                return -1;
//            }
//            if (model.FromDate == null || model.ToDate == null)
//            {
//                message = "Vui lòng chọn thời gian";
//                return -1;
//            }
//            if (model.ToDate >= DateTime.Today)
//            {
//                message = "Vui lòng chọn ngày đến nhỏ hơn ngày hiện tại";
//                return -1;
//            }
//            if (string.IsNullOrEmpty(model.Note))
//            {
//                message = "Vui lòng nhập nội dung";
//                return -1;
//            }
//            if (!string.IsNullOrEmpty(model.Note) && model.Note.Length > 500)
//            {
//                message = "Nội dung chỉ được nhập tối đa 500 kí tự";
//                return -1;
//            }
//            if (!string.IsNullOrEmpty(model.Description) && model.Description.Length > 3000)
//            {
//                message = "Ghi chú chỉ được nhập tối đa 3000 kí tự";
//                return -1;
//            }
//            return 1;
//        }

//        [HttpPost]
//        public async Task<IActionResult> AddJson(DebtStatisticViewModel model)
//        {
//            try
//            {
//                var userLogin = 0;
//                if (HttpContext.User.FindFirst(ClaimTypes.NameIdentifier) != null)
//                {
//                    userLogin = int.Parse(HttpContext.User.FindFirst(ClaimTypes.NameIdentifier).Value);
//                }
//                model.CreateBy = userLogin;
//                var validate = Validate(model, out string messages);
//                if (validate < 0)
//                {
//                    return Ok(new
//                    {
//                        isSuccess = false,
//                        message = messages
//                    });
//                }
//                var client = new HttpClient();
//                var apiPrefix = ReadFile.LoadConfig().API_URL + ReadFile.LoadConfig().API_GET_BILL_NO;
//                var key_token_api = ReadFile.LoadConfig().KEY_TOKEN_API_MANUAL;
//                HttpClient httpClient = new HttpClient();
//                JObject jsonObject = new JObject(
//                   new JProperty("code_type", ((int)GET_CODE_MODULE.BANG_KE).ToString())
//                );
//                var j_param = new Dictionary<string, object>
//                 {
//                     { "key",jsonObject}
//                 };
//                var data_product = JsonConvert.SerializeObject(j_param);
//                var token = CommonHelper.Encode(data_product, key_token_api);
//                var content = new FormUrlEncodedContent(new[] { new KeyValuePair<string, string>("token", token) });
//                var response = await httpClient.PostAsync(apiPrefix, content);
//                var resultAPI = await response.Content.ReadAsStringAsync();
//                var output = JsonConvert.DeserializeObject<OutputAPI>(resultAPI);
//                model.Code = output.code;
//                var result = _debtStatisticRepository.CreateDebtStatistic(model);
//                if (result == -2)
//                    return Ok(new
//                    {
//                        isSuccess = false,
//                        message = "Mã bảng kê " + model.Code + " đã tồn tại trên hệ thống . Vui lòng kiểm tra lại."
//                    });
//                if (result < 1)
//                    return Ok(new
//                    {
//                        isSuccess = true,
//                        message = "Thêm bảng kê thất bại"
//                    });
//                if (model.IsSend == 1)
//                {
//                    var current_user = _ManagementUser.GetCurrentUser();
//                    DebtStatisticLogViewModel debtStatisticLogViewModel = new DebtStatisticLogViewModel();
//                    debtStatisticLogViewModel.DebtStatisticId = result;
//                    debtStatisticLogViewModel.DebtStatisticCode = model.Code;
//                    debtStatisticLogViewModel.Action = "Gửi đi";
//                    debtStatisticLogViewModel.UserId = userLogin;
//                    debtStatisticLogViewModel.CreateTime = DateTime.Now;
//                    debtStatisticLogViewModel.UserName = current_user.Name;
//                    client = new HttpClient();
//                    apiPrefix = ReadFile.LoadConfig().API_URL + ReadFile.LoadConfig().API_INSERT_LOG_ACTIVITY;
//                    key_token_api = ReadFile.LoadConfig().KEY_TOKEN_API_MANUAL;
//                    httpClient = new HttpClient();
//                    j_param = new Dictionary<string, object>
//                        {
//                             {"SourceID", (int)LogSource.BACKEND},
//                             {"KeyID", result},
//                             {"Type", LogType.ACTIVITY},
//                             {"CompanyType", LogHelper.CompanyTypeInt},
//                             {"ObjectType", ObjectType.DEBT_STATISTIC_LOG},
//                             {"Log", JsonConvert.SerializeObject(debtStatisticLogViewModel)},
//                        };
//                    var data = JsonConvert.SerializeObject(j_param);
//                    token = CommonHelper.Encode(data, key_token_api);
//                    content = new FormUrlEncodedContent(new[] { new KeyValuePair<string, string>("token", token) });
//                    response = await httpClient.PostAsync(apiPrefix, content);
//                    var contents = await response.Content.ReadAsStringAsync();
//                    output = JsonConvert.DeserializeObject<OutputAPI>(contents);
//                    if (output.status != 0)
//                        LogHelper.InsertLogTelegram("AddJson - DebtStatisticController. Lỗi không push được log Gửi bảng kê. " + contents);
//                }
//                return Ok(new
//                {
//                    isSuccess = true,
//                    message = "Thêm bảng kê thành công"
//                });
//            }
//            catch (Exception ex)
//            {
//                LogHelper.InsertLogTelegram("AddJson - DebtStatisticController: " + ex + ". Đã có lỗi xảy ra");
//                return Ok(new
//                {
//                    isSuccess = false,
//                    message = "Thêm bảng kê thất bại" + ". Đã có lỗi xảy ra"
//                });
//            }
//        }

//        public IActionResult Edit(int debtStatisticId)
//        {
//            var model = _debtStatisticRepository.GetDetailDebtStatistic(debtStatisticId);
//            return PartialView(model);
//        }

//        [HttpPost]
//        public async Task<IActionResult> Update(DebtStatisticViewModel model)
//        {
//            try
//            {
//                var userLogin = 0;
//                if (HttpContext.User.FindFirst(ClaimTypes.NameIdentifier) != null)
//                {
//                    userLogin = int.Parse(HttpContext.User.FindFirst(ClaimTypes.NameIdentifier).Value);
//                }
//                model.UpdatedBy = userLogin;
//                var validate = Validate(model, out string messages, true);
//                if (validate < 0)
//                {
//                    return Ok(new
//                    {
//                        isSuccess = false,
//                        message = messages
//                    });
//                }
//                var result = _debtStatisticRepository.UpdateDebtStatistic(model);
//                if (result < 1)
//                    return Ok(new
//                    {
//                        isSuccess = true,
//                        message = "Chỉnh sửa bảng kê thất bại"
//                    });
//                if (model.IsSend == 1)
//                {
//                    var current_user = _ManagementUser.GetCurrentUser();
//                    DebtStatisticLogViewModel debtStatisticLogViewModel = new DebtStatisticLogViewModel();
//                    debtStatisticLogViewModel.DebtStatisticId = result;
//                    debtStatisticLogViewModel.DebtStatisticCode = model.Code;
//                    debtStatisticLogViewModel.Action = "Gửi đi";
//                    debtStatisticLogViewModel.UserId = userLogin;
//                    debtStatisticLogViewModel.UserName = current_user.Name;
//                    debtStatisticLogViewModel.CreateTime = DateTime.Now;
//                    var client = new HttpClient();
//                    var apiPrefix = ReadFile.LoadConfig().API_URL + ReadFile.LoadConfig().API_INSERT_LOG_ACTIVITY;
//                    var key_token_api = ReadFile.LoadConfig().KEY_TOKEN_API_MANUAL;
//                    var httpClient = new HttpClient();
//                    var j_param = new Dictionary<string, object>
//                        {
//                             {"SourceID", (int)LogSource.BACKEND},
//                             {"KeyID", result},
//                             {"Type", LogType.ACTIVITY},
//                             {"CompanyType", LogHelper.CompanyTypeInt},
//                             {"ObjectType", ObjectType.DEBT_STATISTIC_LOG},
//                             {"Log", JsonConvert.SerializeObject(debtStatisticLogViewModel)},
//                        };
//                    var data = JsonConvert.SerializeObject(j_param);
//                    var token = CommonHelper.Encode(data, key_token_api);
//                    var content = new FormUrlEncodedContent(new[] { new KeyValuePair<string, string>("token", token) });
//                    var response = await httpClient.PostAsync(apiPrefix, content);
//                    var contents = await response.Content.ReadAsStringAsync();
//                    var output = JsonConvert.DeserializeObject<OutputAPI>(contents);
//                    if (output.status != 0)
//                        LogHelper.InsertLogTelegram("AddJson - DebtStatisticController. Lỗi không push được log Gửi bảng kê. " + contents);
//                }
//                return Ok(new
//                {
//                    isSuccess = true,
//                    message = "Cập nhật bảng kê thành công"
//                });
//            }
//            catch (Exception ex)
//            {
//                LogHelper.InsertLogTelegram("Update - DebtStatisticController: " + ex + ". Đã có lỗi xảy ra");
//                return Ok(new
//                {
//                    isSuccess = false,
//                    message = "Cập nhật bảng kê thất bại" + ". Đã có lỗi xảy ra"
//                });
//            }
//        }

//        public async Task<IActionResult> Detail(int debtStatisticId)
//        {
//            var model = _debtStatisticRepository.GetDetailDebtStatistic(debtStatisticId);
//            var current_user = _ManagementUser.GetCurrentUser();
//            ViewBag.KTT_DUYET_BANG_KE = 0;
//            ViewBag.isAdmin = false;
//            if (current_user != null && !string.IsNullOrEmpty(current_user.Role))
//            {
//                var listRole = Array.ConvertAll(current_user.Role.Split(','), int.Parse);
//                bool isAdmin = _userRepository.IsAdmin(current_user.Id);
//                ViewBag.isAdmin = isAdmin;

//                bool isHeadOfAccountant = _userRepository.IsHeadOfAccountant(current_user.Id);
//                if (isHeadOfAccountant)
//                    ViewBag.KTT_DUYET_BANG_KE = 1;

//                bool IsAccountant = _userRepository.IsAccountant(current_user.Id);
//                if (IsAccountant)
//                    ViewBag.KTT_DUYET_BANG_KE = 1;
//            }
//            return View(model);
//        }

//        [HttpPost]
//        public async Task<IActionResult> Approve(long requestId, string debtStatisticCode, int status)
//        {
//            try
//            {
//                var userLogin = 0;
//                if (HttpContext.User.FindFirst(ClaimTypes.NameIdentifier) != null)
//                {
//                    userLogin = int.Parse(HttpContext.User.FindFirst(ClaimTypes.NameIdentifier).Value);
//                }
//                var current_user = _ManagementUser.GetCurrentUser();
//                var orderNosApproved = _debtStatisticRepository.CheckApproveDebtStatistic(debtStatisticCode, out string orderNoList);
//                if (!string.IsNullOrEmpty(orderNosApproved))
//                {
//                    return Ok(new
//                    {
//                        isSuccess = false,
//                        message = "Đơn hàng " + orderNosApproved + " đã được xác nhận ở bảng kê " + orderNoList + ". Vui lòng kiểm tra lại"
//                    });
//                }
//                var result = _debtStatisticRepository.ApproveDebtStatistic(debtStatisticCode, userLogin, status);
//                if (result < 1)
//                    return Ok(new
//                    {
//                        isSuccess = true,
//                        message = "Xác nhận bảng kê thất bại"
//                    });
//                DebtStatisticLogViewModel debtStatisticLogViewModel = new DebtStatisticLogViewModel();
//                debtStatisticLogViewModel.DebtStatisticId = requestId;
//                debtStatisticLogViewModel.DebtStatisticCode = debtStatisticCode;
//                if (status == (int)DEBT_STATISTIC_STATUS.CHO_KHACH_HANG_XAC_NHAN)
//                    debtStatisticLogViewModel.Action = "Kế toán xác nhận";
//                else
//                    debtStatisticLogViewModel.Action = "Khách hàng xác nhận";
//                debtStatisticLogViewModel.UserId = userLogin;
//                debtStatisticLogViewModel.UserName = current_user.Name;
//                debtStatisticLogViewModel.CreateTime = DateTime.Now;
//                var client = new HttpClient();
//                var apiPrefix = ReadFile.LoadConfig().API_URL + ReadFile.LoadConfig().API_INSERT_LOG_ACTIVITY;
//                var key_token_api = ReadFile.LoadConfig().KEY_TOKEN_API_MANUAL;
//                HttpClient httpClient = new HttpClient();
//                var j_param = new Dictionary<string, object>
//                {
//                     {"SourceID", (int)LogSource.BACKEND},
//                     {"KeyID", requestId},
//                     {"Type", LogType.ACTIVITY},
//                     {"CompanyType", LogHelper.CompanyTypeInt},
//                     {"ObjectType", ObjectType.DEBT_STATISTIC_LOG},
//                     {"Log", JsonConvert.SerializeObject(debtStatisticLogViewModel)},
//                };
//                var data = JsonConvert.SerializeObject(j_param);
//                var token = CommonHelper.Encode(data, key_token_api);
//                var content = new FormUrlEncodedContent(new[] { new KeyValuePair<string, string>("token", token) });
//                var response = await httpClient.PostAsync(apiPrefix, content);
//                var contents = await response.Content.ReadAsStringAsync();
//                var output = JsonConvert.DeserializeObject<OutputAPI>(contents);
//                if (output.status != 0)
//                    LogHelper.InsertLogTelegram("Approve - DebtStatisticController. Lỗi không push được log Xác nhận bảng kê. " + contents);

//                long _UserId = 0;
//                if (HttpContext.User.FindFirst(ClaimTypes.NameIdentifier) != null)
//                {
//                    _UserId = Convert.ToInt64(HttpContext.User.FindFirst(ClaimTypes.NameIdentifier).Value);
//                }
//                string link = "/DebtStatistic/Detail?debtStatisticId=" + requestId;
//                apiService.SendMessage(_UserId.ToString(), ((int)ModuleType.BANG_KE).ToString(), ((int)ActionType.DUYET_YEU_CAU_CHI).ToString(), debtStatisticCode, link, current_user == null ? "0" : current_user.Role);

//                return Ok(new
//                {
//                    isSuccess = true,
//                    message = "Xác nhận bảng kê thành công"
//                });
//            }
//            catch (Exception ex)
//            {
//                LogHelper.InsertLogTelegram("Approve - DebtStatisticController: " + ex + ". Đã có lỗi xảy ra");
//                return Ok(new
//                {
//                    isSuccess = false,
//                    message = "Xác nhận bảng kê thất bại" + ". Đã có lỗi xảy ra"
//                });
//            }
//        }

//        [HttpPost]
//        public async Task<IActionResult> Delete(string debtStatisticCode)
//        {
//            try
//            {
//                var userLogin = 0;
//                if (HttpContext.User.FindFirst(ClaimTypes.NameIdentifier) != null)
//                {
//                    userLogin = int.Parse(HttpContext.User.FindFirst(ClaimTypes.NameIdentifier).Value);
//                }
//                var result = _debtStatisticRepository.DeleteDebtStatistic(debtStatisticCode, userLogin);
//                if (result < 1)
//                    return Ok(new
//                    {
//                        isSuccess = true,
//                        message = "Xóa bảng kê thất bại"
//                    });

//                return Ok(new
//                {
//                    isSuccess = true,
//                    message = "Xóa bảng kê thành công"
//                });
//            }
//            catch (Exception ex)
//            {
//                LogHelper.InsertLogTelegram("Delete - DebtStatisticController: " + ex + ". Đã có lỗi xảy ra");
//                return Ok(new
//                {
//                    isSuccess = false,
//                    message = "Xóa bảng kê thất bại" + ". Đã có lỗi xảy ra"
//                });
//            }
//        }

//        [HttpPost]
//        public async Task<IActionResult> Cancel(long requestId, string debtStatisticCode, double totalAmountPay)
//        {
//            try
//            {
//                var userLogin = 0;
//                if (HttpContext.User.FindFirst(ClaimTypes.NameIdentifier) != null)
//                {
//                    userLogin = int.Parse(HttpContext.User.FindFirst(ClaimTypes.NameIdentifier).Value);
//                }
//                if (totalAmountPay > 0)
//                {
//                    return Ok(new
//                    {
//                        isSuccess = false,
//                        message = "Đã có đơn hàng của bảng kê đã được thanh toán nên không thể hủy. Vui lòng kiểm tra lại"
//                    });
//                }
//                var result = _debtStatisticRepository.CancelDebtStatistic(debtStatisticCode, userLogin);
//                if (result < 1)
//                    return Ok(new
//                    {
//                        isSuccess = true,
//                        message = "Xóa bảng kê thất bại"
//                    });
//                var current_user = _ManagementUser.GetCurrentUser();
//                DebtStatisticLogViewModel debtStatisticLogViewModel = new DebtStatisticLogViewModel();
//                debtStatisticLogViewModel.DebtStatisticId = requestId;
//                debtStatisticLogViewModel.DebtStatisticCode = debtStatisticCode;
//                debtStatisticLogViewModel.Action = "Hủy";
//                debtStatisticLogViewModel.UserId = userLogin;
//                debtStatisticLogViewModel.UserName = current_user.Name;
//                debtStatisticLogViewModel.CreateTime = DateTime.Now;
//                var client = new HttpClient();
//                var apiPrefix = ReadFile.LoadConfig().API_URL + ReadFile.LoadConfig().API_INSERT_LOG_ACTIVITY;
//                var key_token_api = ReadFile.LoadConfig().KEY_TOKEN_API_MANUAL;
//                HttpClient httpClient = new HttpClient();
//                var j_param = new Dictionary<string, object>
//                {
//                     {"SourceID", (int)LogSource.BACKEND},
//                     {"KeyID", requestId},
//                     {"Type", LogType.ACTIVITY},
//                     {"CompanyType", LogHelper.CompanyTypeInt},
//                     {"ObjectType", ObjectType.DEBT_STATISTIC_LOG},
//                     {"Log", JsonConvert.SerializeObject(debtStatisticLogViewModel)},
//                };
//                var data = JsonConvert.SerializeObject(j_param);
//                var token = CommonHelper.Encode(data, key_token_api);
//                var content = new FormUrlEncodedContent(new[] { new KeyValuePair<string, string>("token", token) });
//                var response = await httpClient.PostAsync(apiPrefix, content);
//                var contents = await response.Content.ReadAsStringAsync();
//                var output = JsonConvert.DeserializeObject<OutputAPI>(contents);
//                if (output.status != 0)
//                    LogHelper.InsertLogTelegram("Cancel - DebtStatisticController. Lỗi không push được log Hủy bảng kê. " + contents);

//                return Ok(new
//                {
//                    isSuccess = true,
//                    message = "Xóa bảng kê thành công"
//                });
//            }
//            catch (Exception ex)
//            {
//                LogHelper.InsertLogTelegram("Delete - DebtStatisticController: " + ex + ". Đã có lỗi xảy ra");
//                return Ok(new
//                {
//                    isSuccess = false,
//                    message = "Xóa bảng kê thất bại" + ". Đã có lỗi xảy ra"
//                });
//            }
//        }

//        public IActionResult Reject()
//        {
//            return PartialView();
//        }

//        [HttpPost]
//        public async Task<IActionResult> RejectRequest(long requestId, string debtStatisticCode, string note)
//        {
//            try
//            {
//                if (!string.IsNullOrEmpty(note) && note.Length > 300)
//                {
//                    return Ok(new
//                    {
//                        isSuccess = false,
//                        message = "Nội dung từ chối không được lớn hơn 300 kí tự"
//                    });
//                }
//                var userLogin = 0;
//                if (HttpContext.User.FindFirst(ClaimTypes.NameIdentifier) != null)
//                {
//                    userLogin = int.Parse(HttpContext.User.FindFirst(ClaimTypes.NameIdentifier).Value);
//                }
//                var result = _debtStatisticRepository.RejectDebtStatistic(debtStatisticCode, note, userLogin);
//                if (result < 1)
//                    return Ok(new
//                    {
//                        isSuccess = true,
//                        message = "Từ chối bảng kê thất bại"
//                    });
//                var current_user = _ManagementUser.GetCurrentUser();
//                DebtStatisticLogViewModel debtStatisticLogViewModel = new DebtStatisticLogViewModel();
//                debtStatisticLogViewModel.DebtStatisticId = requestId;
//                debtStatisticLogViewModel.DebtStatisticCode = debtStatisticCode;
//                debtStatisticLogViewModel.Action = "Từ chối[" + note + "]";
//                debtStatisticLogViewModel.UserId = userLogin;
//                debtStatisticLogViewModel.CreateTime = DateTime.Now;
//                debtStatisticLogViewModel.UserName = current_user.Name;
//                var client = new HttpClient();
//                var apiPrefix = ReadFile.LoadConfig().API_URL + ReadFile.LoadConfig().API_INSERT_LOG_ACTIVITY;
//                var key_token_api = ReadFile.LoadConfig().KEY_TOKEN_API_MANUAL;
//                HttpClient httpClient = new HttpClient();
//                var j_param = new Dictionary<string, object>
//                {
//                     {"SourceID", (int)LogSource.BACKEND},
//                     {"KeyID", requestId},
//                     {"Type", LogType.ACTIVITY},
//                     {"CompanyType", LogHelper.CompanyTypeInt},
//                     {"ObjectType", ObjectType.DEBT_STATISTIC_LOG},
//                     {"Log", JsonConvert.SerializeObject(debtStatisticLogViewModel)},
//                };
//                var data = JsonConvert.SerializeObject(j_param);
//                var token = CommonHelper.Encode(data, key_token_api);
//                var content = new FormUrlEncodedContent(new[] { new KeyValuePair<string, string>("token", token) });
//                var response = await httpClient.PostAsync(apiPrefix, content);
//                var contents = await response.Content.ReadAsStringAsync();
//                var output = JsonConvert.DeserializeObject<OutputAPI>(contents);
//                if (output.status != 0)
//                    LogHelper.InsertLogTelegram("Cancel - DebtStatisticController. Lỗi không push được log Từ chối bảng kê. " + contents);
//                long _UserId = 0;
//                if (HttpContext.User.FindFirst(ClaimTypes.NameIdentifier) != null)
//                {
//                    _UserId = Convert.ToInt64(HttpContext.User.FindFirst(ClaimTypes.NameIdentifier).Value);
//                }
//                string link = "/DebtStatistic/Detail?debtStatisticId=" + requestId;
//                apiService.SendMessage(_UserId.ToString(), ((int)ModuleType.BANG_KE).ToString(), ((int)ActionType.TU_CHOI_DUYET_BANG_KE).ToString(), debtStatisticCode, link, current_user == null ? "0" : current_user.Role);
//                return Ok(new
//                {
//                    isSuccess = true,
//                    message = "Từ chối bảng kê thành công"
//                });
//            }
//            catch (Exception ex)
//            {
//                LogHelper.InsertLogTelegram("Update - PaymentRequestController: " + ex + ". Đã có lỗi xảy ra");
//                return Ok(new
//                {
//                    isSuccess = false,
//                    message = "Từ chối bảng kê thất bại" + ". Đã có lỗi xảy ra"
//                });
//            }
//        }

//        public IActionResult History(int requestId)
//        {
//            //ViewBag.listRequestHistory = _debtStatisticRepository.GetHistoriesByRequestId(requestId);
//            var listDebtStatisticLogViewModel = new List<DebtStatisticLogViewModel>();
//            FilterMongoService filterMongoService = new FilterMongoService();
//            var listDebtBrickLog = filterMongoService.SearchHistoryBackend(requestId, new List<string> { ObjectType.DEBT_STATISTIC_LOG });
//            foreach (var item in listDebtBrickLog)
//            {
//                var model = JsonConvert.DeserializeObject<DebtStatisticLogViewModel>(item.Log);
//                listDebtStatisticLogViewModel.Add(model);
//            }
//            return PartialView(listDebtStatisticLogViewModel);
//        }

//        [HttpPost]
//        public async Task<IActionResult> GetOrderListByClientId(DebtStatisticViewModel model, int requestId = 0)
//        {
//            try
//            {
//                if (model.ClientId == null || model.ClientId == 0 || model.FromDate == null || model.ToDate == null)
//                    return Ok(new
//                    {
//                        isSuccess = false,
//                        message = "Thất bại",
//                        data = new List<Entities.Models.Order>()
//                    });
//                var listOrder = _debtStatisticRepository.GetOrderListByClientId(model.ClientId.Value, model.FromDate.Value, model.ToDate.Value, requestId);
//                return Ok(new
//                {
//                    isSuccess = true,
//                    message = "Thành công",
//                    data = listOrder
//                });
//            }
//            catch (Exception ex)
//            {
//                LogHelper.InsertLogTelegram("GetOrderListByClientId - PaymentRequestController: " + ex + ". Đã có lỗi xảy ra");
//                return Ok(new
//                {
//                    isSuccess = false,
//                    message = "Thất bại" + ". Đã có lỗi xảy ra",
//                    data = new List<Entities.Models.Order>()
//                });
//            }
//        }
//    }
//}
