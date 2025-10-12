using Microsoft.AspNetCore.Mvc;

namespace Api.Controller
{
    public class OrdersController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
