using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Infrastructure.Persistence;

namespace Api.Controllers
{
    public class MenuItemsController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
