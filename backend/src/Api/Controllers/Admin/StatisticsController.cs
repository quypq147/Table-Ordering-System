using Application.Abstractions;
using Application.Dashboard;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Api.Controllers.Admin;

[ApiController]
[Route("api/admin/statistics")]
[Authorize(Policy = "RequireStaffOrAdmin")]
public sealed class StatisticsController : ControllerBase
{
    private readonly IQueryHandler<ListStatisticsQuery, StatisticsDto> _handler;

    public StatisticsController(IQueryHandler<ListStatisticsQuery, StatisticsDto> handler)
        => _handler = handler;

    [HttpGet]
    public async Task<ActionResult<StatisticsDto>> Get(
        [FromQuery] DateTime? fromUtc,
        [FromQuery] DateTime? toUtc,
        [FromQuery] int top = 5,
        CancellationToken ct = default)
    {
        var dto = await _handler.Handle(new ListStatisticsQuery(fromUtc, toUtc, top), ct);
        return Ok(dto);
    }
}
