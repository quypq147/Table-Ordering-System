using Microsoft.AspNetCore.SignalR;
using Application.Abstractions;
using Domain.Entities;

namespace Api.Hubs;

public sealed class CustomerHub : Hub
{
    private readonly IApplicationDbContext _db;
    public CustomerHub(IApplicationDbContext db)
    {
        _db = db;
    }

    public Task JoinOrderGroup(string orderId) => Groups.AddToGroupAsync(Context.ConnectionId, $"order-{orderId}");
    public Task LeaveOrderGroup(string orderId) => Groups.RemoveFromGroupAsync(Context.ConnectionId, $"order-{orderId}");
    public Task JoinTableGroup(string tableId) => Groups.AddToGroupAsync(Context.ConnectionId, $"table-{tableId}");
    public Task LeaveTableGroup(string tableId) => Groups.RemoveFromGroupAsync(Context.ConnectionId, $"table-{tableId}");

    // Nhóm nhân viên (phuc v\u / bep)
    public Task JoinStaffGroup() => Groups.AddToGroupAsync(Context.ConnectionId, "staff");
    public Task LeaveStaffGroup() => Groups.RemoveFromGroupAsync(Context.ConnectionId, "staff");

    public async Task SendChatMessage(string tableId, string sender, string message)
    {
        if (string.IsNullOrWhiteSpace(message)) return;
        var entity = new ChatMessage
        {
            TableKey = tableId,
            Sender = string.IsNullOrWhiteSpace(sender) ? "customer" : sender.Trim(),
            Message = message.Trim(),
            SentAtUtc = DateTime.UtcNow
        };
        _db.ChatMessages.Add(entity);
        await _db.SaveChangesAsync();

        var payload = new { tableId, sender = entity.Sender, message = entity.Message, sentAtUtc = entity.SentAtUtc };
        await Clients.Group($"table-{tableId}").SendAsync("chatMessage", payload);
    }
}
