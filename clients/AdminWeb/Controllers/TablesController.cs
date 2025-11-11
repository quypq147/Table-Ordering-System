using AdminWeb.Dtos;
using AdminWeb.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TableOrdering.Contracts;
using AdminWeb.Models; // added

namespace AdminWeb.Controllers
{
    [Authorize(Policy = "RequireSignedIn")]
    public class TablesController : Controller
    {
        private readonly IBackendApiClient _api;
        public TablesController(IBackendApiClient api) => _api = api;

        // LIST
        public async Task<IActionResult> Index()
        {
            var tables = await _api.GetTablesAsync();
            Paginated<OrderSummaryDto> orders;
            try { orders = await _api.GetOrdersAsync(1,200); }
            catch { orders = new Paginated<OrderSummaryDto>(new List<OrderSummaryDto>(),1,200,0); }

            var activeStatuses = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
            {
                "Pending","Active","Open","InProgress","Processing"
            };

            var activeOrdersByTable = orders.Items
                .Where(o => activeStatuses.Contains(o.Status))
                .GroupBy(o => o.Code) // assuming order Code maps to table Code
                .ToDictionary(g => g.Key, g => g.OrderByDescending(x => x.CreatedAt).First());

            var vm = new TablesIndexVm(tables, activeOrdersByTable);
            return View(vm);
        }

        // CREATE (GET)
        public IActionResult Create()
            => View(new CreateTableRequest("",4)); // mặc định capacity=4, status removed

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

        // UPDATE (GET)
        public async Task<IActionResult> Edit(Guid id)
        {
            var table = await _api.GetTableAsync(id);
            if (table == null) return NotFound();
            var req = new UpdateTableRequest(table.Code, table.Seats, table.Status);
            ViewData["TableId"] = id;
            return View(req); // need to add Edit.cshtml separately
        }

        // UPDATE (POST)
        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(Guid id, UpdateTableRequest req)
        {
            if (!ModelState.IsValid)
            {
                ViewData["TableId"] = id;
                return View(req);
            }
            var res = await _api.UpdateTableAsync(id, req);
            if (!res.IsSuccessStatusCode)
            {
                TempData["Error"] = $"Update failed: {(int)res.StatusCode}";
                ViewData["TableId"] = id;
                return View(req);
            }
            TempData["Success"] = "Updated";
            return RedirectToAction(nameof(Index));
        }

        // DELETE
        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(Guid id)
        {
            var res = await _api.DeleteTableAsync(id);
            if (!res.IsSuccessStatusCode)
                TempData["Error"] = $"Delete failed: {(int)res.StatusCode}";
            else
                TempData["Success"] = "Deleted";
            return RedirectToAction(nameof(Index));
        }
    }
}



