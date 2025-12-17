using Microsoft.AspNetCore.SignalR;
using Application.Abstractions;
using Domain.Entities;
using Microsoft.Extensions.Logging;

namespace Api.Hubs;

public sealed class CustomerHub : Hub
{
    private readonly IApplicationDbContext _db;
    private readonly ILogger<CustomerHub> _logger;

    public CustomerHub(IApplicationDbContext db, ILogger<CustomerHub> logger)
    {
        _db = db;
        _logger = logger;
    }

    public override Task OnConnectedAsync()
    {
        _logger.LogInformation("[CustomerHub] Connected: {ConnectionId}", Context.ConnectionId);
        return base.OnConnectedAsync();
    }

    public override Task OnDisconnectedAsync(Exception? exception)
    {
        _logger.LogInformation("[CustomerHub] Disconnected: {ConnectionId}. Error={Error}", Context.ConnectionId, exception?.Message);
        return base.OnDisconnectedAsync(exception);
    }

    public Task JoinOrderGroup(string orderId)
    {
        _logger.LogInformation("[CustomerHub] JoinOrderGroup: conn={ConnectionId}, order={OrderId}", Context.ConnectionId, orderId);
        return Groups.AddToGroupAsync(Context.ConnectionId, $"order-{orderId}");
    }

    public Task LeaveOrderGroup(string orderId)
    {
        _logger.LogInformation("[CustomerHub] LeaveOrderGroup: conn={ConnectionId}, order={OrderId}", Context.ConnectionId, orderId);
        return Groups.RemoveFromGroupAsync(Context.ConnectionId, $"order-{orderId}");
    }

    public Task JoinTableGroup(string tableId)
    {
        _logger.LogInformation("[CustomerHub] JoinTableGroup: conn={ConnectionId}, table={TableId}", Context.ConnectionId, tableId);
        return Groups.AddToGroupAsync(Context.ConnectionId, $"table-{tableId}");
    }

    public Task LeaveTableGroup(string tableId)
    {
        _logger.LogInformation("[CustomerHub] LeaveTableGroup: conn={ConnectionId}, table={TableId}", Context.ConnectionId, tableId);
        return Groups.RemoveFromGroupAsync(Context.ConnectionId, $"table-{tableId}");
    }

    // Nhóm nhân viên (phuc vu / bep)
    public Task JoinStaffGroup()
    {
        _logger.LogInformation("[CustomerHub] JoinStaffGroup: conn={ConnectionId}", Context.ConnectionId);
        return Groups.AddToGroupAsync(Context.ConnectionId, "staff");
    }

    public Task LeaveStaffGroup()
    {
        _logger.LogInformation("[CustomerHub] LeaveStaffGroup: conn={ConnectionId}", Context.ConnectionId);
        return Groups.RemoveFromGroupAsync(Context.ConnectionId, "staff");
    }

    public async Task SendChatMessage(string tableId, string sender, string message)
    {
        if (string.IsNullOrWhiteSpace(message))
        {
            _logger.LogInformation("[CustomerHub] SendChatMessage: empty message ignored. table={TableId}, sender={Sender}", tableId, sender);
            return;
        }

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

        _logger.LogInformation("[CustomerHub] SendChatMessage: table={TableId}, sender={Sender}, messageLength={Length}", tableId, entity.Sender, entity.Message.Length);

        // G?i cho khách/thi?t b? ?ang join theo bàn
        await Clients.Group($"table-{tableId}").SendAsync("chatMessage", payload);

        // G?i thêm cho t?t c? nhân viên (WaiterApp ?ã JoinStaffGroup)
        await Clients.Group("staff").SendAsync("chatMessage", payload);
    }
}
