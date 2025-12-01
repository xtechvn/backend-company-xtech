//using System;
//using System.Collections.Generic;
//using System.IO;
//using System.Linq;
//using System.Net.Http;
//using System.Security.Claims;
//using System.Threading.Tasks;
//using Entities.ViewModels;
//using Entities.ViewModels.Attachment;
//using Entities.ViewModels.Funding;
//using Entities.ViewModels.HotelBookingCode;
//using Microsoft.AspNetCore.Hosting;
//using Microsoft.AspNetCore.Mvc;
//using Newtonsoft.Json;
//using OfficeOpenXml.FormulaParsing.Excel.Functions.Text;
//using Repositories.IRepositories;
//using Utilities;
//using Utilities.Contants;
//using WEB.Adavigo.CMS.Service;
//using WEB.Adavigo.CMS.Service.ServiceInterface;
//using WEB.CMS.Customize;
//using WEB.CMS.Models;

//namespace WEB.Adavigo.CMS.Controllers.Funding
//{
//    [CustomAuthorize]

//    public class DebtBrickController : Controller
//    {
//        private readonly IContractPayRepository _contractPayRepository;
//        private readonly IClientRepository _clientRepository;
//        private readonly IWebHostEnvironment _WebHostEnvironment;
//        private readonly IAllCodeRepository _allCodeRepository;
//        private ManagementUser _ManagementUser;
//        private readonly IEmailService _emailService;

//        public DebtBrickController(IContractPayRepository contractPayRepository, IAllCodeRepository allCodeRepository,
//               ManagementUser ManagementUser, IWebHostEnvironment hostEnvironment, IEmailService emailService, IClientRepository clientRepository)
//        {
//            _contractPayRepository = contractPayRepository;
//            _allCodeRepository = allCodeRepository;
//            _ManagementUser = ManagementUser;
//            _WebHostEnvironment = hostEnvironment;
//            _emailService = emailService;
//            _clientRepository = clientRepository;
//        }

//        //Gạch nợ đơn hàng
//        public IActionResult Index()
//        {
//            var listOrderStatus = _allCodeRepository.GetListByType(AllCodeType.ORDER_STATUS);
//            ViewBag.allCode_ORDER_STATUS = listOrderStatus.Where(n => n.CodeValue != (int)OrderStatus.CANCEL).ToList();
//            ViewBag.allCode_ORDER_DEBT_STATUS = _allCodeRepository.GetListByType(AllCodeType.ORDER_DEBT_STATUS);
//            return View();
//        }

//        [HttpPost]
//        public IActionResult Search(ContractPaySearchModel searchModel, int currentPage = 1, int pageSize = 20)
//        {
//            var model = new GenericViewModel<OrderDebtViewModel>();
//            try
//            {
//                if (!string.IsNullOrEmpty(searchModel.OrderNo)) searchModel.OrderNo = searchModel.OrderNo.Trim();
//                if (!string.IsNullOrEmpty(searchModel.LabelName)) searchModel.LabelName = searchModel.LabelName.Trim();
//                if (searchModel.StatusMulti == null) searchModel.StatusMulti = new List<int>();
//                var listOrderDebt = _contractPayRepository.GetListOrderDebt(searchModel, out long total, currentPage, pageSize);
//                model.CurrentPage = currentPage;
//                model.ListData = listOrderDebt;
//                model.PageSize = pageSize;
//                model.TotalRecord = total;
//                model.TotalPage = (int)Math.Ceiling((double)total / pageSize);
//            }
//            catch (Exception ex)
//            {
//                LogHelper.InsertLogTelegram("Search - DebtBrickController: " + ex);
//            }
//            return PartialView(model);
//        }

//        public IActionResult Add(long orderId, long clientId, string orderNo, double amount, double payment
//            , string debtNote)
//        {
//            var client = _clientRepository.GetClientDetailByClientId(clientId).Result;
//            ViewBag.clientName = client?.ClientName;
//            ViewBag.orderId = orderId;
//            ViewBag.clientId = clientId;
//            ViewBag.orderNo = orderNo;
//            ViewBag.amount = amount;
//            ViewBag.payment = payment;
//            ViewBag.debtNote = debtNote;
//            return PartialView();
//        }

//        [HttpPost]
//        public async Task<IActionResult> AddJson(ContractPayViewModel model)
//        {
//            try
//            {
//                var userLogin = 0;
//                if (HttpContext.User.FindFirst(ClaimTypes.NameIdentifier) != null)
//                {
//                    userLogin = int.Parse(HttpContext.User.FindFirst(ClaimTypes.NameIdentifier).Value);
//                }
//                model.CreatedBy = userLogin;
//                var current_user = _ManagementUser.GetCurrentUser();
//                DebtBrickLogViewModel debtBrickLogViewModel = new DebtBrickLogViewModel();
//                model.CopyProperties(debtBrickLogViewModel);
//                debtBrickLogViewModel.Amount = model.ContractPayDetails.Sum(n => n.Amount);
//                debtBrickLogViewModel.UserId = userLogin;
//                debtBrickLogViewModel.UserName = current_user.Name;
//                var result = _contractPayRepository.AddContractPayDetail(model, true);
//                if (result <= 0)
//                    return Ok(new
//                    {
//                        isSuccess = false,
//                        message = "Gạch nợ thất bại"
//                    });
//                var client = new HttpClient();
//                var apiPrefix = ReadFile.LoadConfig().API_URL + ReadFile.LoadConfig().API_INSERT_LOG_ACTIVITY;
//                var key_token_api = ReadFile.LoadConfig().KEY_TOKEN_API_MANUAL;
//                HttpClient httpClient = new HttpClient();
//                var j_param = new Dictionary<string, object>
//                {
//                     {"SourceID", (int)LogSource.BACKEND},
//                     {"KeyID", model.OrderId.ToString()},
//                     {"Type", LogType.ACTIVITY},
//                     {"CompanyType", LogHelper.CompanyTypeInt},
//                     {"ObjectType", ObjectType.DEBTBRICKLOG_ORDER},
//                     {"Log", JsonConvert.SerializeObject(debtBrickLogViewModel)},
//                };
//                var data = JsonConvert.SerializeObject(j_param);
//                var token = CommonHelper.Encode(data, key_token_api);
//                var content = new FormUrlEncodedContent(new[] { new KeyValuePair<string, string>("token", token) });
//                var response = await httpClient.PostAsync(apiPrefix, content);
//                var contents = await response.Content.ReadAsStringAsync();
//                var output = JsonConvert.DeserializeObject<OutputAPI>(contents);
//                if (output.status != 0)
//                    LogHelper.InsertLogTelegram("AddJson - DebtBrickController. Lỗi không push được log gạch nợ. " + contents);

//                foreach (var item in model.ContractPayDetails)
//                {
//                    var modelEmail = new SendEmailViewModel();
//                    var attach_file = new List<AttachfileViewModel>();
//                    modelEmail.Orderid = item.OrderId;
//                    modelEmail.ServiceType = (int)EmailType.SaleDH;
//                    var ContractPayByOrderId = await _contractPayRepository.GetContractPayByOrderId(item.OrderId);

//                    if (ContractPayByOrderId != null && ContractPayByOrderId.Count <= 1)
//                    {
//                        _emailService.SendEmail(modelEmail, attach_file);
//                    }
//                }

//                return Ok(new
//                {
//                    isSuccess = true,
//                    message = "Gạch nợ thành công"
//                });
//            }
//            catch (Exception ex)
//            {
//                LogHelper.InsertLogTelegram("AddJson - DebtBrickController: " + ex);
//                return Ok(new
//                {
//                    isSuccess = false,
//                    message = "Gạch nợ thất bại. Đã có lỗi xảy ra."
//                });
//            }
//        }

//        [HttpPost]
//        public IActionResult ExportExcel(ContractPaySearchModel searchModel)
//        {
//            try
//            {
//                string _FileName = "OrderDebt" + Guid.NewGuid() + ".xlsx";
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
//                if (!string.IsNullOrEmpty(searchModel.OrderNo)) searchModel.OrderNo = searchModel.OrderNo.Trim();
//                if (!string.IsNullOrEmpty(searchModel.LabelName)) searchModel.LabelName = searchModel.LabelName.Trim();
//                if (searchModel.StatusMulti == null) searchModel.StatusMulti = new List<int>();
//                string FilePath = Path.Combine(_UploadDirectory, _FileName);
//                var rsPath = _contractPayRepository.ExportOrderDebt(searchModel, FilePath);

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
//                    message = ex.Message.ToString()
//                });
//            }
//        }

//        [HttpPost]
//        public async Task<IActionResult> GetListContractPayByClientId(int clientId, long orderId, double amountOrder)
//        {
//            try
//            {
//                var listContractPayByClientId = _contractPayRepository.GetListContractPayByClientId(clientId);
//                foreach (var item in listContractPayByClientId)
//                {
//                    item.AmountOrder = amountOrder;
//                    item.OrderId = orderId;
//                }
//                return Ok(new
//                {
//                    isSuccess = true,
//                    message = "Thành công",
//                    data = listContractPayByClientId
//                });
//            }
//            catch (Exception ex)
//            {
//                LogHelper.InsertLogTelegram("GetListContractPayByClientId - DebtBrickController: " + ex);
//                return Ok(new
//                {
//                    isSuccess = false,
//                    message = "Thất bại",
//                    data = new List<ContractPayViewModel>()
//                });
//            }
//        }

//        [HttpPost]
//        public async Task<IActionResult> History(long orderId, string orderNo)
//        {
//            var listDebtBrickLogViewModel = new List<DebtBrickLogViewModel>();
//            FilterMongoService filterMongoService = new FilterMongoService();
//            var listDebtBrickLog = filterMongoService.SearchHistoryBackend(orderId, new List<string> { ObjectType.DEBTBRICKLOG_ORDER, ObjectType.UNDO_DEBTBRICKLOG_ORDER });
//            foreach (var item in listDebtBrickLog)
//            {
//                if (!string.IsNullOrEmpty(item.Log) && item.ObjectType == ObjectType.DEBTBRICKLOG_ORDER)
//                {
//                    var model = JsonConvert.DeserializeObject<DebtBrickLogViewModel>(item.Log);
//                    model.CreatedTime = item.CreatedTime;
//                    model.ObjectType = ObjectType.DEBTBRICKLOG_ORDER;
//                    model.OrderNo = orderNo;
//                    listDebtBrickLogViewModel.Add(model);
//                }

//                if (!string.IsNullOrEmpty(item.Log) && item.ObjectType == ObjectType.UNDO_DEBTBRICKLOG_ORDER)
//                {
//                    var model = JsonConvert.DeserializeObject<UndoDebtBrickLogViewModel>(item.Log);
//                    model.CreatedTime = item.CreatedTime;
//                    DebtBrickLogViewModel debtBrickLogViewModel = new DebtBrickLogViewModel();
//                    model.CopyProperties(debtBrickLogViewModel);
//                    debtBrickLogViewModel.ObjectType = ObjectType.UNDO_DEBTBRICKLOG_ORDER;
//                    debtBrickLogViewModel.OrderNo = orderNo;
//                    debtBrickLogViewModel.CreatedTime = item.CreatedTime;
//                    listDebtBrickLogViewModel.Add(debtBrickLogViewModel);
//                }
//            }
//            ViewBag.listDebtBrickLogViewModel = listDebtBrickLogViewModel;
//            return PartialView();
//        }

//        [HttpPost]
//        public async Task<IActionResult> GetListContractPayByOrderId(long orderId)
//        {
//            try
//            {
//                var listContractPayByClientId = _contractPayRepository.GetListContractPayByOrderId(orderId);
//                foreach (var item in listContractPayByClientId)
//                {
//                    item.OrderId = orderId;
//                }
//                return Ok(new
//                {
//                    isSuccess = true,
//                    message = "Thành công",
//                    data = listContractPayByClientId
//                });
//            }
//            catch (Exception ex)
//            {
//                LogHelper.InsertLogTelegram("GetListContractPayByClientId - DebtBrickController: " + ex);
//                return Ok(new
//                {
//                    isSuccess = false,
//                    message = "Thất bại",
//                    data = new List<ContractPayViewModel>()
//                });
//            }
//        }

//        public IActionResult UndoDebtBrick(long orderId, long clientId, string orderNo, double amount, double payment)
//        {
//            var client = _clientRepository.GetClientDetailByClientId(clientId).Result;
//            ViewBag.clientName = client?.ClientName;
//            ViewBag.orderId = orderId;
//            ViewBag.clientId = clientId;
//            ViewBag.orderNo = orderNo;
//            ViewBag.amount = amount;
//            ViewBag.payment = payment;
//            return PartialView();
//        }

//        [HttpPost]
//        public async Task<IActionResult> UndoDebtBrickJson(List<ContractPayViewModel> model)
//        {
//            try
//            {
//                var userLogin = 0;
//                if (HttpContext.User.FindFirst(ClaimTypes.NameIdentifier) != null)
//                {
//                    userLogin = int.Parse(HttpContext.User.FindFirst(ClaimTypes.NameIdentifier).Value);
//                }
//                var current_user = _ManagementUser.GetCurrentUser();
//                UndoDebtBrickLogViewModel undoDebtBrickLogViewModel = new UndoDebtBrickLogViewModel();
//                model.CopyProperties(undoDebtBrickLogViewModel);
//                undoDebtBrickLogViewModel.Amount = model.Sum(n => n.Amount);
//                undoDebtBrickLogViewModel.UserId = userLogin;
//                undoDebtBrickLogViewModel.UserName = current_user.Name;
//                undoDebtBrickLogViewModel.ClientId = model.FirstOrDefault().ClientId;
//                undoDebtBrickLogViewModel.ContractPayViewModels = model;
//                undoDebtBrickLogViewModel.OrderId = model[0].OrderId;
//                undoDebtBrickLogViewModel.CreatedTime = DateTime.Now;
//                foreach (var item in model)
//                {
//                    item.CreatedBy = userLogin;
//                    item.UpdatedBy = userLogin;
//                }
//                var result = _contractPayRepository.DeleteContractPayDetail(model);
//                if (result <= 0)
//                    return Ok(new
//                    {
//                        isSuccess = true,
//                        message = "Bỏ gạch nợ thất bại"
//                    });
//                var client = new HttpClient();
//                var apiPrefix = ReadFile.LoadConfig().API_URL + ReadFile.LoadConfig().API_INSERT_LOG_ACTIVITY;
//                var key_token_api = ReadFile.LoadConfig().KEY_TOKEN_API_MANUAL;
//                HttpClient httpClient = new HttpClient();
//                var j_param = new Dictionary<string, object>
//                {
//                     {"SourceID", (int)LogSource.BACKEND},
//                     {"KeyID", model[0].OrderId.ToString()},
//                     {"ObjectType", ObjectType.UNDO_DEBTBRICKLOG_ORDER},
//                     {"CompanyType", LogHelper.CompanyTypeInt},
//                     {"Type", LogType.ACTIVITY},
//                     {"Log", JsonConvert.SerializeObject(undoDebtBrickLogViewModel)},
//                };
//                var data = JsonConvert.SerializeObject(j_param);
//                var token = CommonHelper.Encode(data, key_token_api);
//                var content = new FormUrlEncodedContent(new[] { new KeyValuePair<string, string>("token", token) });
//                var response = await httpClient.PostAsync(apiPrefix, content);
//                var contents = await response.Content.ReadAsStringAsync();
//                var output = JsonConvert.DeserializeObject<OutputAPI>(contents);
//                if (output.status != 0)
//                    LogHelper.InsertLogTelegram("AddJson - DebtBrickController. Lỗi không push được log gạch nợ. " + contents);
//                return Ok(new
//                {
//                    isSuccess = true,
//                    message = "Bỏ gạch nợ thành công"
//                });
//            }
//            catch (Exception ex)
//            {
//                LogHelper.InsertLogTelegram("AddJson - DebtBrickController: " + ex);
//                return Ok(new
//                {
//                    isSuccess = false,
//                    message = "Bỏ gạch nợ thất bại. Đã có lỗi xảy ra."
//                });
//            }
//        }

//        //Gạch nợ phiếu thu
//        public IActionResult IndexContractPay()
//        {
//            var listOrderStatus = _allCodeRepository.GetListByType(AllCodeType.ORDER_STATUS);
//            ViewBag.allCode_ORDER_STATUS = listOrderStatus.Where(n => n.CodeValue != (int)OrderStatus.CANCEL).ToList();
//            ViewBag.allCode_ORDER_DEBT_STATUS = _allCodeRepository.GetListByType(AllCodeType.CONTRACTPAY_DEBT_STATUS);
//            return View();
//        }

//        [HttpPost]
//        public IActionResult SearchContractPay(ContractPaySearchModel searchModel, int currentPage = 1, int pageSize = 20)
//        {
//            var model = new GenericViewModel<ContractPayDebtViewModel>();
//            try
//            {
//                if (!string.IsNullOrEmpty(searchModel.BillNo)) searchModel.BillNo = searchModel.BillNo.Trim();
//                var listOrderDebt = _contractPayRepository.GetListContractPayDebt(searchModel, out long total, currentPage, pageSize);
//                model.CurrentPage = currentPage;
//                model.ListData = listOrderDebt;
//                model.PageSize = pageSize;
//                model.TotalRecord = total;
//                model.TotalPage = (int)Math.Ceiling((double)total / pageSize);
//            }
//            catch (Exception ex)
//            {
//                LogHelper.InsertLogTelegram("Search - DebtBrickController: " + ex);
//            }
//            return PartialView(model);
//        }

//        [HttpPost]
//        public IActionResult ExportExcelContractPay(ContractPaySearchModel searchModel)
//        {
//            try
//            {
//                string _FileName = "ContractPayDebt" + Guid.NewGuid() + ".xlsx";
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
//                if (!string.IsNullOrEmpty(searchModel.BillNo)) searchModel.BillNo = searchModel.BillNo.Trim();
//                string FilePath = Path.Combine(_UploadDirectory, _FileName);
//                var rsPath = _contractPayRepository.ExportContractPayDebt(searchModel, FilePath);

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
//                    message = ex.Message.ToString()
//                });
//            }
//        }

//        [HttpPost]
//        public async Task<IActionResult> GetListOrderDebtByClientId(int clientId, long payId, double amountOrder)
//        {
//            try
//            {
//                var listOrderDebtByClientId = _contractPayRepository.GetListOrderDebtByClientId(clientId);
//                foreach (var item in listOrderDebtByClientId)
//                {
//                    item.AmountOrder = item.Amount;
//                    item.Payment = item.Payment == null ? 0 : item.Payment;
//                    item.ContractPayId = payId;
//                }
//                return Ok(new
//                {
//                    isSuccess = true,
//                    message = "Thành công",
//                    data = listOrderDebtByClientId
//                });
//            }
//            catch (Exception ex)
//            {
//                LogHelper.InsertLogTelegram("GetListContractPayByClientId - DebtBrickController: " + ex);
//                return Ok(new
//                {
//                    isSuccess = false,
//                    message = "Thất bại",
//                    data = new List<ContractPayViewModel>()
//                });
//            }
//        }

//        public IActionResult AddWithContractPay(long contractPayId, long clientId, string clientName, string billNo, double amount,
//          double payment, string debtNote)
//        {
//            var client = _clientRepository.GetClientDetailByClientId(clientId).Result;
//            ViewBag.clientName = client?.ClientName;
//            ViewBag.contractPayId = contractPayId;
//            ViewBag.clientId = clientId;
//            ViewBag.billNo = billNo;
//            ViewBag.amount = amount;
//            ViewBag.payment = payment;
//            ViewBag.debtNote = debtNote;
//            return PartialView();
//        }

//        [HttpPost]
//        public async Task<IActionResult> AddWithContractPayJson(OrderDebtViewModel model)
//        {
//            try
//            {
//                var userLogin = 0;
//                if (HttpContext.User.FindFirst(ClaimTypes.NameIdentifier) != null)
//                {
//                    userLogin = int.Parse(HttpContext.User.FindFirst(ClaimTypes.NameIdentifier).Value);
//                }
//                model.CreatedBy = userLogin;
//                var current_user = _ManagementUser.GetCurrentUser();
//                DebtBrickLogViewModel debtBrickLogViewModel = new DebtBrickLogViewModel();
//                model.CopyProperties(debtBrickLogViewModel);
//                debtBrickLogViewModel.Amount = model.orderDebtViewModels.Sum(n => n.Amount);
//                debtBrickLogViewModel.UserId = userLogin;
//                debtBrickLogViewModel.UserName = current_user.Name;
//                debtBrickLogViewModel.ContractPayId = (int)model.ContractPayId;
//                debtBrickLogViewModel.ContractPayBillNo = model.BillNo;
//                ContractPayViewModel contractPayViewModel = new ContractPayViewModel();
//                contractPayViewModel.PayId = (int)model.ContractPayId;
//                contractPayViewModel.ContractPayDetails = new List<ContractPayDetailViewModel>();
//                contractPayViewModel.CreatedBy = userLogin;
//                contractPayViewModel.DebtStatus = model.orderDebtViewModels.Sum(n => n.Amount) == model.AmountRemain ?
//                    (int)DepositHistoryConstant.CONTRACTPAY_DEBT_STATUS.DA_GACH_HET :
//                    (int)DepositHistoryConstant.CONTRACTPAY_DEBT_STATUS.CHUA_GACH_HET;
//                contractPayViewModel.Note = model.Note;
//                foreach (var item in model.orderDebtViewModels)
//                {
//                    ContractPayDetailViewModel detailViewModel = new ContractPayDetailViewModel();
//                    detailViewModel.PayId = (int)model.ContractPayId;
//                    detailViewModel.OrderId = (int)item.OrderId;
//                    detailViewModel.CreatedBy = userLogin;
//                    detailViewModel.Amount = item.Amount;
//                    detailViewModel.AmountOrder = item.AmountRemain;
//                    contractPayViewModel.ContractPayDetails.Add(detailViewModel);
//                }

//                var result = _contractPayRepository.AddContractPayDetail(contractPayViewModel);
//                if (result <= 0)
//                    return Ok(new
//                    {
//                        isSuccess = true,
//                        message = "Gạch nợ thất bại"
//                    });
//                var client = new HttpClient();
//                var apiPrefix = ReadFile.LoadConfig().API_URL + ReadFile.LoadConfig().API_INSERT_LOG_ACTIVITY;
//                var key_token_api = ReadFile.LoadConfig().KEY_TOKEN_API_MANUAL;
//                HttpClient httpClient = new HttpClient();
//                var j_param = new Dictionary<string, object>
//                {
//                     {"SourceID", (int)LogSource.BACKEND},
//                     {"KeyID", model.ContractPayId.ToString()},
//                     {"Type", LogType.ACTIVITY},
//                     {"CompanyType", LogHelper.CompanyTypeInt},
//                     {"ObjectType", ObjectType.DEBTBRICKLOG_CONTRACTPAY},
//                     {"Log", JsonConvert.SerializeObject(debtBrickLogViewModel)},
//                };
//                var data = JsonConvert.SerializeObject(j_param);
//                var token = CommonHelper.Encode(data, key_token_api);
//                var content = new FormUrlEncodedContent(new[] { new KeyValuePair<string, string>("token", token) });
//                var response = await httpClient.PostAsync(apiPrefix, content);
//                var contents = await response.Content.ReadAsStringAsync();
//                var output = JsonConvert.DeserializeObject<OutputAPI>(contents);
//                if (output.status != 0)
//                    LogHelper.InsertLogTelegram("AddJson - DebtBrickController. Lỗi không push được log gạch nợ. " + contents);
//                return Ok(new
//                {
//                    isSuccess = true,
//                    message = "Gạch nợ thành công"
//                });
//            }
//            catch (Exception ex)
//            {
//                LogHelper.InsertLogTelegram("AddJson - DebtBrickController: " + ex);
//                return Ok(new
//                {
//                    isSuccess = false,
//                    message = "Gạch nợ thất bại. Đã có lỗi xảy ra."
//                });
//            }
//        }

//        public IActionResult UndoDebtBrickContractPay(long contractPayId, long clientId, string clientName, string billNo, double amount,
//          double payment)
//        {
//            var client = _clientRepository.GetClientDetailByClientId(clientId).Result;
//            ViewBag.clientName = client?.ClientName;
//            ViewBag.contractPayId = contractPayId;
//            ViewBag.clientId = clientId;
//            ViewBag.billNo = billNo;
//            ViewBag.amount = amount;
//            ViewBag.payment = payment;
//            return PartialView();
//        }

//        [HttpPost]
//        public async Task<IActionResult> UndoDebtBrickContractPayJson(List<ContractPayViewModel> model)
//        {
//            try
//            {
//                var userLogin = 0;
//                if (HttpContext.User.FindFirst(ClaimTypes.NameIdentifier) != null)
//                {
//                    userLogin = int.Parse(HttpContext.User.FindFirst(ClaimTypes.NameIdentifier).Value);
//                }
//                var current_user = _ManagementUser.GetCurrentUser();
//                UndoDebtBrickLogViewModel undoDebtBrickLogViewModel = new UndoDebtBrickLogViewModel();
//                model.CopyProperties(undoDebtBrickLogViewModel);
//                undoDebtBrickLogViewModel.Amount = model.Sum(n => n.AmountPayDetail);
//                undoDebtBrickLogViewModel.UserId = userLogin;
//                undoDebtBrickLogViewModel.UserName = current_user.Name;
//                undoDebtBrickLogViewModel.ClientId = model.FirstOrDefault().ClientId;
//                undoDebtBrickLogViewModel.ContractPayId = model[0].PayId;
//                undoDebtBrickLogViewModel.ContractPayViewModels = model;
//                foreach (var item in model)
//                {
//                    item.CreatedBy = userLogin;
//                    item.UpdatedBy = userLogin;
//                }
//                var result = _contractPayRepository.DeleteContractPayDetail(model);
//                if (result <= 0)
//                    return Ok(new
//                    {
//                        isSuccess = false,
//                        message = "Bỏ gạch nợ thất bại"
//                    });
//                var client = new HttpClient();
//                var apiPrefix = ReadFile.LoadConfig().API_URL + ReadFile.LoadConfig().API_INSERT_LOG_ACTIVITY;
//                var key_token_api = ReadFile.LoadConfig().KEY_TOKEN_API_MANUAL;
//                HttpClient httpClient = new HttpClient();
//                var j_param = new Dictionary<string, object>
//                {
//                     {"SourceID", (int)LogSource.BACKEND},
//                     {"KeyID", model[0].PayId.ToString()},
//                     {"ObjectType", ObjectType.UNDO_DEBTBRICKLOG_CONTRACTPAY},
//                     {"CompanyType", LogHelper.CompanyTypeInt},
//                     {"Type", LogType.ACTIVITY},
//                     {"Log", JsonConvert.SerializeObject(undoDebtBrickLogViewModel)},
//                };
//                var data = JsonConvert.SerializeObject(j_param);
//                var token = CommonHelper.Encode(data, key_token_api);
//                var content = new FormUrlEncodedContent(new[] { new KeyValuePair<string, string>("token", token) });
//                var response = await httpClient.PostAsync(apiPrefix, content);
//                var contents = await response.Content.ReadAsStringAsync();
//                var output = JsonConvert.DeserializeObject<OutputAPI>(contents);
//                if (output.status != 0)
//                    LogHelper.InsertLogTelegram("AddJson - DebtBrickController. Lỗi không push được log gạch nợ. " + contents);
//                return Ok(new
//                {
//                    isSuccess = true,
//                    message = "Bỏ gạch nợ thành công"
//                });
//            }
//            catch (Exception ex)
//            {
//                LogHelper.InsertLogTelegram("UndoDebtBrickContractPayJson - DebtBrickController: " + ex);
//                return Ok(new
//                {
//                    isSuccess = false,
//                    message = "Bỏ gạch nợ thất bại. Đã có lỗi xảy ra."
//                });
//            }
//        }

//        [HttpPost]
//        public async Task<IActionResult> HistoryWithContractPay(long contractPayId, string bIllNo)
//        {
//            var listDebtBrickLogViewModel = new List<DebtBrickLogViewModel>();
//            FilterMongoService filterMongoService = new FilterMongoService();
//            var listDebtBrickLog = filterMongoService.SearchHistoryBackend(contractPayId, new List<string> { ObjectType.DEBTBRICKLOG_CONTRACTPAY, ObjectType.UNDO_DEBTBRICKLOG_CONTRACTPAY });
//            foreach (var item in listDebtBrickLog)
//            {
//                if (!string.IsNullOrEmpty(item.Log) && item.ObjectType == ObjectType.DEBTBRICKLOG_CONTRACTPAY)
//                {
//                    var model = JsonConvert.DeserializeObject<DebtBrickLogViewModel>(item.Log);
//                    model.CreatedTime = item.CreatedTime;
//                    model.ObjectType = ObjectType.DEBTBRICKLOG_CONTRACTPAY;
//                    model.ContractPayBillNo = bIllNo;
//                    listDebtBrickLogViewModel.Add(model);
//                }

//                if (!string.IsNullOrEmpty(item.Log) && item.ObjectType == ObjectType.UNDO_DEBTBRICKLOG_CONTRACTPAY)
//                {
//                    var model = JsonConvert.DeserializeObject<UndoDebtBrickLogViewModel>(item.Log);
//                    model.CreatedTime = item.CreatedTime;
//                    DebtBrickLogViewModel debtBrickLogViewModel = new DebtBrickLogViewModel();
//                    model.CopyProperties(debtBrickLogViewModel);
//                    debtBrickLogViewModel.ObjectType = ObjectType.UNDO_DEBTBRICKLOG_CONTRACTPAY;
//                    debtBrickLogViewModel.ContractPayBillNo = bIllNo;
//                    debtBrickLogViewModel.CreatedTime = item.CreatedTime;
//                    debtBrickLogViewModel.Amount = model.Amount;
//                    listDebtBrickLogViewModel.Add(debtBrickLogViewModel);
//                }
//            }
//            ViewBag.listDebtBrickLogViewModel = listDebtBrickLogViewModel;
//            return PartialView();
//        }

//        [HttpPost]
//        public async Task<IActionResult> GetListContractPayByPayId(long payId)
//        {
//            try
//            {
//                var listContractPayByClientId = _contractPayRepository.GetListContractPayByPayId(payId);
//                foreach (var item in listContractPayByClientId)
//                {
//                    item.PayId = (int)payId;
//                }
//                return Ok(new
//                {
//                    isSuccess = true,
//                    message = "Thành công",
//                    data = listContractPayByClientId
//                });
//            }
//            catch (Exception ex)
//            {
//                LogHelper.InsertLogTelegram("GetListContractPayByClientId - DebtBrickController: " + ex);
//                return Ok(new
//                {
//                    isSuccess = false,
//                    message = "Thất bại",
//                    data = new List<ContractPayViewModel>()
//                });
//            }
//        }

//        //[HttpPost]
//        //public async Task<IActionResult> GetListOrderDebtByPayId(int clientId, long payId, double amountOrder)
//        //{
//        //    try
//        //    {
//        //        var listOrderDebtByClientId = _contractPayRepository.GetListOrderDebtByClientId(clientId);
//        //        foreach (var item in listOrderDebtByClientId)
//        //        {
//        //            item.AmountOrder = item.Amount;
//        //            item.Payment = item.Payment == null ? 0 : item.Payment;
//        //            item.ContractPayId = payId;
//        //        }
//        //        return Ok(new
//        //        {
//        //            isSuccess = true,
//        //            message = "Thành công",
//        //            data = listOrderDebtByClientId
//        //        });
//        //    }
//        //    catch (Exception ex)
//        //    {
//        //        LogHelper.InsertLogTelegram("GetListOrderDebtByPayId - DebtBrickController: " + ex);
//        //        return Ok(new
//        //        {
//        //            isSuccess = false,
//        //            message = "Thất bại",
//        //            data = new List<ContractPayViewModel>()
//        //        });
//        //    }
//        //}

//    }
//}
