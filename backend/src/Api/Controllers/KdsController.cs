using Application.Common.CQRS;
using Application.Kds.Commands;
using Application.Kds.Queries;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace Api.Controllers;

[ApiController]
[Route("api/kds")]
public sealed class KdsController(ISender sender, ILogger<KdsController> logger) : ControllerBase
{
    [HttpGet("tickets")]
    [AllowAnonymous]
    public async Task<ActionResult<IReadOnlyList<KitchenTicketDto>>> ListTickets([FromQuery] string? status, CancellationToken ct)
    {
        logger.LogInformation("[KDS] ListTickets called with status='{Status}'", status);
        var result = await sender.Send(new ListKitchenTicketsQuery(status), ct);
        logger.LogInformation("[KDS] ListTickets returned {Count} tickets", result?.Count ?? 0);
        return Ok(result);
    }

    // Avoid route value name 'action' to prevent MVC reserved value conflicts
    [HttpPost("tickets/{id}/{op}")]
    [AllowAnonymous]
    public async Task<ActionResult<KitchenTicketDto>> ChangeStatus(Guid id, string op, CancellationToken ct)
    {
        logger.LogInformation("[KDS] ChangeStatus request: ticket={TicketId}, op='{Op}'", id, op);
        try
        {
            var dto = await sender.Send(new ChangeTicketStatusCommand(id, op), ct);
            logger.LogInformation("[KDS] ChangeStatus success: ticket={TicketId}, newStatus='{Status}'", id, dto.Status);
            return Ok(dto);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "[KDS] ChangeStatus failed for ticket={TicketId}, op='{Op}'", id, op);
            // Surface error details for easier client debugging
            return Problem(detail: ex.Message, statusCode: 400, title: "KDS change status failed");
        }
    }
}
