using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using Api.Hubs;
using Application.Abstractions;
using Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Api.Controllers;

[ApiController]
[Route("api/chat")]
public sealed class ChatController : ControllerBase
{
    private readonly IHubContext<CustomerHub> _hub;
    private readonly IApplicationDbContext _db;
    public ChatController(IHubContext<CustomerHub> hub, IApplicationDbContext db)
    { _hub = hub; _db = db; }

    public sealed record SendRequest(string TableCode, string Sender, string Message);

    [HttpPost]
    public async Task<IActionResult> Send([FromBody] SendRequest req)
    {
        if (string.IsNullOrWhiteSpace(req.TableCode) || string.IsNullOrWhiteSpace(req.Message))
            return BadRequest("TableCode và Message là b?t bu?c");

        var entity = new ChatMessage
        {
            TableKey = req.TableCode,
            Sender = string.IsNullOrWhiteSpace(req.Sender) ? "customer" : req.Sender.Trim(),
            Message = req.Message.Trim(),
            SentAtUtc = DateTime.UtcNow
        };
        _db.ChatMessages.Add(entity);
        await _db.SaveChangesAsync();

        var payload = new { tableCode = entity.TableKey, sender = entity.Sender, message = entity.Message, sentAtUtc = entity.SentAtUtc };
        await _hub.Clients.Group($"table-{req.TableCode}").SendAsync("chatMessage", payload);
        return Ok(payload);
    }

    [HttpGet("history")]
    public async Task<IActionResult> History([FromQuery] string tableCode, [FromQuery] int take = 50)
    {
        if (string.IsNullOrWhiteSpace(tableCode)) return BadRequest("tableCode là b?t bu?c");
        var msgs = await _db.ChatMessages
            .Where(c => c.TableKey == tableCode)
            .OrderByDescending(c => c.SentAtUtc)
            .Take(Math.Clamp(take, 10, 200))
            .Select(c => new { c.Id, tableCode = c.TableKey, c.Sender, c.Message, c.SentAtUtc })
            .ToListAsync();
        return Ok(msgs.OrderBy(m => m.SentAtUtc));
    }
}
