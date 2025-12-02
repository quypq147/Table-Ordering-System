using Application.Abstractions;
using Application.Dashboard;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Api.Controllers.Admin;

[ApiController]
[Route("api/admin/dashboard")]
[Authorize(Policy = "RequireStaffOrAdmin")]
public sealed class DashboardController : ControllerBase
{
    private readonly IQueryHandler<ListDashboardMetricsQuery, DashboardMetricsDto> _handler;

    public DashboardController(IQueryHandler<ListDashboardMetricsQuery, DashboardMetricsDto> handler)
    => _handler = handler;

    [HttpGet]
    public async Task<ActionResult<DashboardMetricsDto>> Get(CancellationToken ct)
    {
        var dto = await _handler.Handle(new ListDashboardMetricsQuery(), ct);
        return Ok(dto);
    }
}
