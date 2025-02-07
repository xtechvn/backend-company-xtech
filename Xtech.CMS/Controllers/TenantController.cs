using DocumentFormat.OpenXml.Office2016.Drawing.ChartDrawing;
using DocumentFormat.OpenXml.VariantTypes;
using Entities.Models;
using Entities.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using Repositories.IRepositories;
using System.Security.Claims;
using Utilities;
using WEB.CMS.Customize;

namespace Xtech.CMS.Controllers
{
    [CustomAuthorize]
    public class TenantController : Controller
    {
        private readonly IWebHostEnvironment _WebHostEnvironment;
        private readonly IUserRepository _UserRepository;
        private readonly IDepartmentRepository _DepartmentRepository;
        private readonly IRoleRepository _RoleRepository;
        private readonly IMFARepository _mFARepository;
        private readonly ManagementUser _ManagementUser;
        private readonly IConfiguration _configuration;


        public TenantController(IUserRepository userRepository, IRoleRepository roleRepository,
            IWebHostEnvironment hostEnvironment, IMFARepository mFARepository,
            IDepartmentRepository departmentRepository, ManagementUser managementUser, IConfiguration configuration)
        {
            _UserRepository = userRepository;
            _RoleRepository = roleRepository;
            _WebHostEnvironment = hostEnvironment;
            _mFARepository = mFARepository;
            _DepartmentRepository = departmentRepository;
            _ManagementUser = managementUser;
            _configuration = configuration;
        }
        public IActionResult Index()
        {
            return View();
        }
        [HttpPost]
        public async Task<IActionResult> Search(TenantSearchModel searchModel)
        {
            var model = new GenericViewModel<TenantViewModel>();
            try
            {
                model =await _UserRepository.GetListTenant(searchModel);
            }
            catch (Exception ex)
            {
                LogHelper.InsertLogTelegram("Search - TenantController: " + ex);
            }
            return PartialView(model);
        }
        [HttpPost]
        public async Task<IActionResult> UpSert(IFormFile imagefile, UserViewModel model)
        {
            try
            {
                string imageUrl = string.Empty;
                if (imagefile != null)
                {
                    string _FileName = Guid.NewGuid() + Path.GetExtension(imagefile.FileName);
                    string _UploadFolder = @"uploads/images";
                    string _UploadDirectory = Path.Combine(_WebHostEnvironment.WebRootPath, _UploadFolder);

                    if (!Directory.Exists(_UploadDirectory))
                    {
                        Directory.CreateDirectory(_UploadDirectory);
                    }

                    string filePath = Path.Combine(_UploadDirectory, _FileName);

                    if (!System.IO.File.Exists(filePath))
                    {
                        using (var fileStream = new FileStream(filePath, FileMode.Create))
                        {
                            await imagefile.CopyToAsync(fileStream);
                        }
                    }
                    model.Avata = "/" + _UploadFolder + "/" + _FileName;
                }

                int rs = 0;
                if (model.UserPositionId != null && model.UserPositionId > 0)
                {
                    var active_position = await _UserRepository.GetUserPositionsByID((int)model.UserPositionId);
                    if (active_position != null) model.Level = active_position.Rank;
                }
                if (model.CompanyType == null || model.CompanyType.Trim() == "")
                {
                    model.CompanyType = _configuration["CompanyType"];
                }
                var _UserLogin = 0;
                if (HttpContext.User.FindFirst(ClaimTypes.NameIdentifier) != null)
                {
                    _UserLogin = int.Parse(HttpContext.User.FindFirst(ClaimTypes.NameIdentifier).Value);
                }
                model.CreatedBy = _UserLogin;
                model.ModifiedBy = _UserLogin;
                if (model.Phone == null) model.Phone = "";
                if (model.Avata == null) model.Avata = "";
                if (model.Address == null) model.Address = "";


                if (model.TenantId != 0)
                {
                    rs = await _UserRepository.UpdateTenant(model);
                }
                else
                {
                     rs = await _UserRepository.InsertTenant(model);
                   
                    if (rs <= 0)
                    {
                        return new JsonResult(new
                        {
                            isSuccess = false,
                            message = "Tên đăng nhập hoặc email đã tồn tại"
                        });
                    }

                }

                if (rs > 0)
                {


                    return new JsonResult(new
                    {
                        isSuccess = true,
                        message = "Cập nhật thành công"
                    });
                }
                else if (rs == -1)
                {
                    return new JsonResult(new
                    {
                        isSuccess = false,
                        message = "Tên đăng nhập hoặc email đã tồn tại"
                    });
                }
                else
                {
                    return new JsonResult(new
                    {
                        isSuccess = false,
                        message = "Cập nhật thất bại"
                    });
                }
            }
            catch (Exception ex)
            {
                LogHelper.InsertLogTelegram("UpSert - UserController: " + ex);
                return new JsonResult(new
                {
                    isSuccess = false,
                    message = ex.Message.ToString()
                });
            }
        }
        public async Task<IActionResult> AddOrUpdate(int Id, bool IsClone = false)
        {
            try
            {
                var model = new TenantViewModel();
                if (Id != 0)
                {
                    var detail = await _UserRepository.GetDetailTenantByTenantId(Id);

                    return View(detail);
                }
               
                return View(model);
            }
            catch (Exception ex)
            {
                LogHelper.InsertLogTelegram("AddOrUpdate - UserController: " + ex);
                return Content("");
            }

        }
        public async Task<IActionResult> GetDetail(int Id)
        {
            try
            {
              var  model= await _UserRepository.GetDetailTenantByTenantId(Id);
                return PartialView(model);
            }
            catch (Exception ex)
            {
                LogHelper.InsertLogTelegram("GetDetail - UserController: " + ex);
                ViewBag.IsMFAEnabled = false;
            }
            return PartialView();
        }
        public async Task<IActionResult> ChangeStatus(int id)
        {
            try
            {
                var detail = await _UserRepository.GetDetailTenantByTenantId(id);
                var model = new UserViewModel();
                model.Id=id;
                if (detail.Status == 0)
                {
                    model.Status = 1;
                }
                else
                {
                    model.Status = 0;
                }
                var rs = await _UserRepository.UpdateTenant(model);
                if (rs != -1)
                {
                    return new JsonResult(new
                    {
                        isSuccess = true,
                        message = "Cập nhật thành công",
                        status = rs
                    });
                }
                else
                {
                    return new JsonResult(new
                    {
                        isSuccess = false,
                        message = "Cập nhật thất bại"
                    });
                }
            }
            catch (Exception ex)
            {
                LogHelper.InsertLogTelegram("ChangeUserStatus - UserController: " + ex);
                return new JsonResult(new
                {
                    isSuccess = false,
                    message = ex.Message.ToString()
                });
            }
        }
    }
}
