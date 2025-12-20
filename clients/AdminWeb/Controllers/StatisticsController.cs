using AdminWeb.Services;
using AdminWeb.Services.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Text.Json;

namespace AdminWeb.Controllers;

[Authorize(Policy = "RequireSignedIn")]
public sealed class StatisticsController(IBackendApiClient api) : Controller
{
    private readonly IBackendApiClient _api = api;

    [HttpGet]
    public async Task<IActionResult> Index([FromQuery] DateTime? from, [FromQuery] DateTime? to, [FromQuery] int top = 5)
    {
        // Default: last 7 days (UTC)
        var toUtc = (to ?? DateTime.UtcNow).ToUniversalTime();
        var fromUtc = (from ?? toUtc.AddDays(-6).Date).ToUniversalTime();

        StatisticsVm vm;
        try
        {
            vm = await _api.GetStatisticsAsync(fromUtc, toUtc, top);
        }
        catch (Exception ex)
        {
            vm = new StatisticsVm { FromUtc = fromUtc, ToUtc = toUtc };
            ViewBag.StatsError = ex.Message;
        }

        ViewBag.StatsJson = JsonSerializer.Serialize(vm, new JsonSerializerOptions { PropertyNamingPolicy = null });
        return View(vm);
    }
}
