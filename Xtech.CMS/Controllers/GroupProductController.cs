using Entities.ConfigModels;
using Entities.Models;
using Entities.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Repositories.IRepositories;
using Ultilities.RedisWorker;
using Utilities;
using Utilities.Contants;
using WEB.CMS.Customize;

namespace WEB.CMS.Controllers
{
    [CustomAuthorize]
    public class GroupProductController : Controller
    {
        private readonly IGroupProductRepository _GroupProductRepository;
        private readonly IAllCodeRepository _AllCodeRepository;
        private readonly IPositionRepository _positionRepository;
        private readonly IWebHostEnvironment _WebHostEnvironment;
        private readonly string _UrlStaticImage;
        private readonly IConfiguration _configuration;
        private readonly RedisConn _redisService;

        public GroupProductController(IGroupProductRepository groupProductRepository,
               IWebHostEnvironment hostEnvironment, IPositionRepository positionRepository,
               RedisConn redisService, IAllCodeRepository allCodeRepository, IOptions<DomainConfig> domainConfig, IConfiguration configuration)
        {
            _GroupProductRepository = groupProductRepository;
            _WebHostEnvironment = hostEnvironment;

            _AllCodeRepository = allCodeRepository;
            _UrlStaticImage = domainConfig.Value.ImageStatic;
            _configuration = configuration;
            _redisService = redisService;
            _positionRepository = positionRepository;
        }

        public IActionResult Index()
        {
            return View();
        }

        public async Task<string> Search(string Name, int Status = -1)
        {
            return await _GroupProductRepository.GetListTreeView(Name, Status);
        }

        /// <summary>
        /// Add Or Update GroupProduct
        /// </summary>
        /// <param name="id"></param>
        /// <param name="type">
        /// 0: Add child
        /// 1: Edit itseft
        /// </param>
        /// <returns></returns>
        public async Task<IActionResult> AddOrUpdate(int id, int type)
        {
            var model = new GroupProductDetailModel();
            try
            {
                if (type == 0)
                {
                    model = new GroupProductDetailModel()
                    {
                        Id = 0,
                        Status = 0,
                        OrderNo = 0,
                        ParentId = id
                    };
                }
                else
                {
                    var entity = await _GroupProductRepository.GetById(id);
                    model = new GroupProductDetailModel()
                    {
                        Id = entity.Id,
                        Name = entity.Name,
                        ImagePath = !string.IsNullOrEmpty(entity.ImagePath) ? _UrlStaticImage + entity.ImagePath : entity.ImagePath,
                        OrderNo = entity.OrderNo,
                        ParentId = entity.ParentId,
                        Status = entity.Status,
                       
                        PositionId = entity.PositionId,
                        Description = entity.Description,
                        IsShowFooter=entity.IsShowFooter,
                        IsShowHeader=entity.IsShowHeader,
                        Code=entity.Code
                    };
                }
                _redisService.clear(CacheName.ARTICLE_B2C_CATEGORY_MENU, 0);
            }
            catch
            {

            }

            ViewBag.PositionList = await _positionRepository.GetAll();
            return View(model);
        }

        /// <summary>
        /// public async Task<IActionResult> UpSert(IFormFile imageFile, string imageSize, GroupProductUpsertModel model)
        /// </summary>
        /// <param name="imageFile"></param>
        /// <param name="imageSize"></param>
        /// <param name="model"></param>
        /// <returns></returns>
        public async Task<IActionResult> UpSert(GroupProductUpsertModel model)
        {
            try
            {
                var upsertModel = new GroupProduct()
                {
                    Id = model.Id,
                    Name = model.Name,
                    OrderNo = model.OrderNo,
                    ParentId = model.ParentId,
                    Description = model.Description,
                    PositionId = model.PositionId,
                    Status = model.Status,
                    ImagePath = await UpLoadHelper.UploadBase64Src(model.ImageBase64, _configuration["Setting:DomainStatic"], _configuration["DataBaseConfig:key_api:Static"]),
                    IsShowHeader=model.IsShowHeader,
                    IsShowFooter=model.IsShowFooter,
                    ModifiedOn=DateTime.Now,
                    Code=model.Code
                    
                };
                var rs = await _GroupProductRepository.UpSert(upsertModel);
                if (rs > 0)
                {
                    _redisService.clear(CacheName.ARTICLE_B2C_CATEGORY_MENU, 0);

                    return new JsonResult(new
                    {
                        isSuccess = true,
                        message = "Cập nhật thành công",
                        modelId = rs,
                    });
                }
                else if (rs == -1)
                {
                    return new JsonResult(new
                    {
                        isSuccess = false,
                        message = "Tồn tại nhóm hàng cùng cấp"
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
                return new JsonResult(new
                {
                    isSuccess = false,
                    message = ex.Message.ToString()
                });
            }
        }



        public async Task<IActionResult> Delete(int id)
        {
            try
            {
                var rootParentId = await _GroupProductRepository.GetRootParentId(id);
                var rs = await _GroupProductRepository.Delete(id);

                if (rs > 0)
                {
                    _redisService.clear(CacheName.ARTICLE_B2C_CATEGORY_MENU, 0);


                    return new JsonResult(new
                    {
                        isSuccess = true,
                        message = "Xóa thành công."
                    });
                }
                else if (rs == -1)
                {
                    return new JsonResult(new
                    {
                        isSuccess = false,
                        message = "Nhóm hàng đang được sử dụng. Bạn không thể xóa."
                    });
                }
                else if (rs == -2)
                {
                    return new JsonResult(new
                    {
                        isSuccess = false,
                        message = "Nhóm hàng đang có cấp con. Bạn không thể xóa."
                    });
                }
                else
                {
                    return new JsonResult(new
                    {
                        isSuccess = false,
                        message = "Xóa thất bại."
                    });
                }
            }
            catch (Exception ex)
            {
                return new JsonResult(new
                {
                    isSuccess = false,
                    message = ex.Message
                });
            }
        }

       
        public IActionResult AddCampaign()
        {
            return View();
        }

       




    }
}