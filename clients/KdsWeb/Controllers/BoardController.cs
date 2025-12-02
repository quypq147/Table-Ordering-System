using Microsoft.AspNetCore.Mvc;

namespace KdsWeb.Controllers
{
    public class BoardController(BackendApiClient api, IConfiguration cfg) : Controller
    {
        public async Task<IActionResult> Index()
        {
            var tickets = await api.GetTicketsAsync();  
            return View(tickets);
        }
    }
}
