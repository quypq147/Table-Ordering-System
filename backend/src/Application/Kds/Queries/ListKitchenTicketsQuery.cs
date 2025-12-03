using Application.Abstractions;
using Domain.Entities;
using Domain.Enums;
using Microsoft.EntityFrameworkCore;

namespace Application.Kds.Queries;

public sealed record ListKitchenTicketsQuery(string? Status) : IQuery<IReadOnlyList<KitchenTicketDto>>;

// Align shape with public contract: include OrderCode and TableName and StationId and TableCode
public sealed record KitchenTicketDto(Guid Id, Guid OrderId, string OrderCode, string TableName, Guid StationId, string Status, string ItemName, int Qty, DateTime CreatedAt, string? TableCode);

public static class KitchenTicketMappings
{
    public static KitchenTicketDto ToDto(this KitchenTicket t, string? orderCode, string? tableName, string? tableCode, Guid stationId = default) =>
        new(t.Id, t.OrderId, orderCode ?? string.Empty, tableName ?? string.Empty, stationId, t.Status.ToString(), t.ItemName, t.Quantity, t.CreatedAtUtc, tableCode);
}

public sealed class ListKitchenTicketsHandler(IApplicationDbContext db) : IQueryHandler<ListKitchenTicketsQuery, IReadOnlyList<KitchenTicketDto>>
{
    public async Task<IReadOnlyList<KitchenTicketDto>> Handle(ListKitchenTicketsQuery query, CancellationToken ct)
    {
        var tickets = db.KitchenTickets.AsNoTracking();
        var orders = db.Orders.AsNoTracking();
        var tables = db.Tables.AsNoTracking();

        var q = from t in tickets
                join o in orders on t.OrderId equals o.Id into gj
                from o in gj.DefaultIfEmpty()
                join tab in tables on o != null ? o.TableId : Guid.Empty equals tab.Id into tj
                from tab in tj.DefaultIfEmpty()
                select new { Ticket = t, OrderCode = o != null ? o.Code : string.Empty, TableCode = tab != null ? tab.Code : string.Empty };

        if (!string.IsNullOrWhiteSpace(query.Status) && Enum.TryParse<KitchenTicketStatus>(query.Status, true, out var status))
        {
            q = q.Where(x => x.Ticket.Status == status);
        }

        var list = await q.OrderBy(x => x.Ticket.CreatedAtUtc)
            .Select(x => x.Ticket.ToDto(x.OrderCode, x.TableCode, x.TableCode))
            .ToListAsync(ct);

        return list;
    }
}
