using System.Diagnostics;
using System.Text.Json;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using AdminWeb.Models;
using AdminWeb.Services;

namespace AdminWeb.Controllers;

[Authorize(Policy = "RequireSignedIn")]
public class HomeController(IBackendApiClient api) : Controller
{
    private readonly IBackendApiClient _api = api;

    public async Task<IActionResult> Index()
    {
        var vm = await _api.GetDashboardAsync();
        var json = JsonSerializer.Serialize(vm, new JsonSerializerOptions { PropertyNamingPolicy = null });
        ViewBag.DashboardJson = json;
        return View();
    }
}
