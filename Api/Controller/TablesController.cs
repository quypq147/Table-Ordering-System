using Microsoft.AspNetCore.Mvc;

namespace Api.Controller
{

    public class TablesController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
