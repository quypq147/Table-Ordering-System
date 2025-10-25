using Microsoft.AspNetCore.Mvc;
using AdminWeb.Dtos;
using AdminWeb.Services;

namespace AdminWeb.Controllers
{
    public class TablesController(IBackendApiClient api) : Controller
    {
        private readonly IBackendApiClient _api = api;

        // LIST
        public async Task<IActionResult> Index()
        {
            var tables = await _api.GetTablesAsync();
            return View(tables);
        }

        // CREATE (GET)
        public IActionResult Create()
        {
            return View(new CreateTableRequest("", "", 4, true));
        }

        // CREATE (POST)
        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(CreateTableRequest req)
        {
            var res = await _api.CreateTableAsync(req);
            if (!res.IsSuccessStatusCode)
            {
                TempData["Error"] = "Co loi khi tao ban";
                return View(req);
            }
            TempData["Success"] = "Created";
            return RedirectToAction(nameof(Index));
        }
    }
}


