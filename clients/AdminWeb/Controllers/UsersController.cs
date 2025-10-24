using Microsoft.AspNetCore.Mvc;

namespace AdminWeb.Controllers
{
    public class UsersController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
