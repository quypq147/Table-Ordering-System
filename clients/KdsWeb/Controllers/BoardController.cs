using Microsoft.AspNetCore.Mvc;

namespace KdsWeb.Controllers
{
    public class BoardController(BackendApiClient api, IConfiguration cfg) : Controller
    {
        private Guid StationId => Guid.Parse(cfg["Demo:StationId"] ?? "00000000-0000-0000-0000-000000000001");

        public async Task<IActionResult> Index()
        {
            var tickets = await api.GetTicketsAsync(StationId);
            return View(tickets);
        }
    }

}
