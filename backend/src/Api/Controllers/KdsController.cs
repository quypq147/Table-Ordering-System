using Application.Common.CQRS;
using Application.Kds.Commands;
using Application.Kds.Queries;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Api.Controllers;

[ApiController]
[Route("api/kds")]
public sealed class KdsController(ISender sender) : ControllerBase
{
    [HttpGet("tickets")]
    [AllowAnonymous]
    public async Task<ActionResult<IReadOnlyList<KitchenTicketDto>>> ListTickets([FromQuery] string? status, CancellationToken ct)
    {
        var result = await sender.Send(new ListKitchenTicketsQuery(status), ct);
        return Ok(result);
    }

    [HttpPost("tickets/{id}/{action}")]
    [Authorize(Policy = "RequireStaffOrAdmin")]
    public async Task<ActionResult<KitchenTicketDto>> ChangeStatus(Guid id, string action, CancellationToken ct)
    {
        var dto = await sender.Send(new ChangeTicketStatusCommand(id, action), ct);
        return Ok(dto);
    }
}
