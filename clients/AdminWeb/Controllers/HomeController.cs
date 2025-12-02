using System.Diagnostics;
using System.Text.Json;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using AdminWeb.Models;
using AdminWeb.Services;
using AdminWeb.Services.Models; // for DashboardVm

namespace AdminWeb.Controllers;

[Authorize(Policy = "RequireSignedIn")]
public class HomeController(IBackendApiClient api) : Controller
{
    private readonly IBackendApiClient _api = api;

    public async Task<IActionResult> Index()
    {
        DashboardVm vm;
        try
        {
            vm = await _api.GetDashboardAsync();
        }
        catch (Exception ex)
        {
            // Graceful fallback so the page still renders
            vm = new DashboardVm();
            ViewBag.DashboardError = ex.Message;
        }

        var json = JsonSerializer.Serialize(vm, new JsonSerializerOptions { PropertyNamingPolicy = null });
        ViewBag.DashboardJson = json;
        return View();
    }
}
