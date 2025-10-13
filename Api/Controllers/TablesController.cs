using Microsoft.AspNetCore.Mvc;

namespace Api.Controllers
{

    public class TablesController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
