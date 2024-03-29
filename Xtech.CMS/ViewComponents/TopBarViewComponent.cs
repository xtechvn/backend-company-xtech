using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace WEB.CMS.ViewComponents
{
    public class TopBarViewComponent : ViewComponent
    {
        public async Task<IViewComponentResult> InvokeAsync()
        {
            var _UserName = string.Empty;
            var _UserId = string.Empty;
            try
            {

                if (HttpContext.User.FindFirst(ClaimTypes.Name) != null)
                {
                    _UserName = HttpContext.User.FindFirst(ClaimTypes.Name).Value;
                    _UserId = HttpContext.User.FindFirst(ClaimTypes.NameIdentifier).Value;
                }

            }
            catch
            {

            }

            ViewBag.UserId = _UserId;
            ViewBag.UserName = _UserName;
            return View();
        }
    }
}
