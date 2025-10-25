using AdminWeb.Services;
using Microsoft.AspNetCore.Mvc;

namespace AdminWeb.Controllers
{
    public class UsersController(IBackendApiClient api) : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
