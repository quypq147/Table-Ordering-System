using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;

namespace Api.Hubs;

[Authorize(Policy = "RequireStaffOrAdmin")]
public sealed class KdsHub : Hub
{
    // Optionally allow clients to join groups by status
    public Task JoinStatusGroup(string status) => Groups.AddToGroupAsync(Context.ConnectionId, status);
    public Task LeaveStatusGroup(string status) => Groups.RemoveFromGroupAsync(Context.ConnectionId, status);
}
