// Controllers/DiagnosticsController.cs
using AdminWeb.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AdminWeb.Controllers
{
    [Authorize(Policy = "RequireSignedIn")]
    public class DiagnosticsController(IBackendApiClient api) : Controller
    {
        private readonly IBackendApiClient _api = api;
        

        public async Task<IActionResult> Index()
        {
            var result = new Dictionary<string, object?>();
            try { result["tables"] = (await _api.GetTablesAsync())?.Count; } catch (Exception ex) { result["tables_error"] = ex.Message; }
            try { result["menu"] = (await _api.GetMenuAsync())?.Count; } catch (Exception ex) { result["menu_error"] = ex.Message; }
            try { result["orders"] = (await _api.GetOrdersAsync(1, 5))?.Items?.Count; } catch (Exception ex) { result["orders_error"] = ex.Message; }
            return Json(result);
        }
    }
}