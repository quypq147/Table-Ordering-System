using Application.Common.CQRS;
using Application.Dtos;
using Application.Tables.Commands;
using Application.Tables.Queries;
using Domain.Enums;
using Microsoft.AspNetCore.Mvc;
namespace Api.Controllers;


[ApiController]
[Route("api/[controller]")]
public class TablesController : ControllerBase
{
    private readonly ISender _sender;
    public TablesController(ISender sender) => _sender = sender;

    // ===== Queries =====

    // GET /api/tables/{id} 

    [HttpGet]
    public async Task<ActionResult<List<TableDto>>> GetAllTablesAsync(CancellationToken cancellationToken)
    {
        var query = new GetAllTablesQuery();
        var result = await _sender.Send(query, cancellationToken);
        return Ok(result);
    }
    [HttpGet("{id}")]
    public Task<TableDto?> GetById(Guid id, CancellationToken ct)
    => _sender.Send(new GetTableByIdQuery(id), ct);

    [HttpGet("by-status/{status}")]
    public Task<IReadOnlyList<TableDto>> ListByStatus(TableStatus status, CancellationToken ct)
        => _sender.Send(new ListTablesByStatusQuery(status), ct);

    [HttpPost]
    public Task<TableDto> Create([FromBody] CreateTableCommand cmd, CancellationToken ct)
        => _sender.Send(cmd, ct);

    [HttpPost("{id}/reserve")]
    public Task<TableDto> Reserve(Guid id, CancellationToken ct)
        => _sender.Send(new MarkTableReservedCommand(id), ct);

    [HttpPost("{id}/occupy")]
    public Task<TableDto> Occupy(Guid id, CancellationToken ct)
        => _sender.Send(new MarkTableOccupiedCommand(id), ct);

    [HttpPost("{id}/available")]
    public Task<TableDto> Available(Guid id, CancellationToken ct)
        => _sender.Send(new MarkTableAvailableCommand(id), ct);


    // Delete table
    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(Guid id, CancellationToken ct)
    {
        await _sender.Send(new DeleteTableCommand(id), ct);
        return NoContent();
    }

}
