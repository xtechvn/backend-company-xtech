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
        public IActionResult Search(string userName, string strRoleId, int status = -1, int currentPage = 1, int pageSize = 20)
        {
            var model = new GenericViewModel<UserGridModel>();
            try
            {
                model = _UserRepository.GetPagingList(userName, strRoleId, status, currentPage, pageSize);
            }
            catch (Exception ex)
            {
                LogHelper.InsertLogTelegram("Search - UserController: " + ex);
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


                if (model.Id != 0)
                {
                    rs = await _UserRepository.Update(model);
                }
                else
                {
                     rs = await _UserRepository.InsertTenant(model);
                    //model.TenantId = 0;
                    //rs = await _UserRepository.UpsertUserTenant(model);

                    if (rs <= 0)
                    {
                        return new JsonResult(new
                        {
                            isSuccess = false,
                            message = "Tên đăng nhập hoặc email đã tồn tại"
                        });
                    }

                    //rs = await _UserRepository.Create(model);

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
                var model = new User();
                ViewBag.UserRoleList = null;
                if (Id != 0)
                {

                    model = await _UserRepository.FindById(Id);
                    if (IsClone)
                    {
                        model = new User
                        {
                            FullName = model.FullName,
                            UserName = model.UserName,
                            Email = model.Email,
                            Address = model.Address,
                            BirthDay = model.BirthDay,
                            Gender = model.Gender,
                            Status = model.Status,
                            Note = model.Note,
                            DepartmentId = model.DepartmentId,
                            Phone = model.Phone,
                        };
                    }
                    var list_role_active = await _UserRepository.GetUserActiveRoleList(model.Id);
                    if (list_role_active != null && list_role_active.Count > 0)
                    {
                        ViewBag.UserRoleList = list_role_active.Select(x => x.Id).ToList();
                    }
                }
                else
                {
                    model.Gender = 1;
                }

                ViewBag.DepartmentList = await _DepartmentRepository.GetAll(String.Empty);
                ViewBag.RoleList = await _RoleRepository.GetAll();
                ViewBag.UserPosition = _UserRepository.GetUserPositions();
                return View(model);
            }
            catch (Exception ex)
            {
                LogHelper.InsertLogTelegram("AddOrUpdate - UserController: " + ex);
                return Content("");
            }

        }
    }
}
