using AdminWeb.Dtos;
using AdminWeb.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TableOrdering.Contracts;

namespace AdminWeb.Controllers
{
    [Authorize(Policy = "RequireSignedIn")]
    public class TablesController : Controller
    {
        private readonly IBackendApiClient _api;
        public TablesController(IBackendApiClient api) => _api = api;

        // LIST
        public async Task<IActionResult> Index()
            => View(await _api.GetTablesAsync());

        // CREATE (GET)
        public IActionResult Create()
            => View(new CreateTableRequest("", 4, 1)); // mặc định capacity=4, status=1 (Active)

        // CREATE (POST)
        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(CreateTableRequest req)
        {
            if (!ModelState.IsValid) return View(req);

            var res = await _api.CreateTableAsync(req);
            if (!res.IsSuccessStatusCode)
            {
                TempData["Error"] = $"Create failed: {(int)res.StatusCode}";
                return View(req);
            }

            TempData["Success"] = "Created";
            return RedirectToAction(nameof(Index));
        }
    }
}



