using Application.Public.Tables;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace Api.Controllers.Public;

[ApiController]
[Route("api/public/tables")]
public class TablesPublicController : ControllerBase
{
    private readonly ISender _sender;
    public TablesPublicController(ISender sender) => _sender = sender;

    // GET /api/public/tables/by-code/{code}
    [HttpGet("by-code/{code}")]
    public async Task<ActionResult<object>> GetByCode(string code, CancellationToken ct)
    {
        var id = await _sender.Send(new GetTableByCodeQuery(code), ct);
        if (id is null) return NotFound();
        return Ok(new { Id = id.Value, Code = code.Trim() });
    }
}