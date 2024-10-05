using Entities.ViewModels;
using Microsoft.AspNetCore.Mvc;

namespace Xtech.CMS.ViewComponents
{
    public class PagingNewViewComponent : ViewComponent
    {
        public IViewComponentResult Invoke(Paging pageModel)
        {
            return View(pageModel);
        }
    }
}
