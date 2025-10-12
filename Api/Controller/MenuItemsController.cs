using Microsoft.AspNetCore.Mvc;

namespace Api.Controller
{
    public class MenuItemsController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
