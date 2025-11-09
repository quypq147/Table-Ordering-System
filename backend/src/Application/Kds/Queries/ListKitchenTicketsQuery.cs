using Application.Abstractions;
using Domain.Entities;
using Domain.Enums;
using Microsoft.EntityFrameworkCore;

namespace Application.Kds.Queries;

public sealed record ListKitchenTicketsQuery(string? Status) : IQuery<IReadOnlyList<KitchenTicketDto>>;

public sealed record KitchenTicketDto(Guid Id, Guid OrderId, int OrderItemId, string ItemName, int Quantity, string Status, DateTime CreatedAtUtc, DateTime? StartedAtUtc, DateTime? ReadyAtUtc, DateTime? ServedAtUtc);

public static class KitchenTicketMappings
{
    public static KitchenTicketDto ToDto(this KitchenTicket t) => new(t.Id, t.OrderId, t.OrderItemId, t.ItemName, t.Quantity, t.Status.ToString(), t.CreatedAtUtc, t.StartedAtUtc, t.ReadyAtUtc, t.ServedAtUtc);
}

public sealed class ListKitchenTicketsHandler(IApplicationDbContext db) : IQueryHandler<ListKitchenTicketsQuery, IReadOnlyList<KitchenTicketDto>>
{
    public async Task<IReadOnlyList<KitchenTicketDto>> Handle(ListKitchenTicketsQuery query, CancellationToken ct)
    {
        IQueryable<KitchenTicket> q = db.KitchenTickets.AsNoTracking();
        if (!string.IsNullOrWhiteSpace(query.Status) && Enum.TryParse<KitchenTicketStatus>(query.Status, true, out var status))
        {
            q = q.Where(t => t.Status == status);
        }
        var list = await q.OrderBy(t => t.CreatedAtUtc)
            .Select(t => t.ToDto())
            .ToListAsync(ct);
        return list;
    }
}
