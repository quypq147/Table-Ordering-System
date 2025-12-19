using Application.Abstractions;
using Application.Orders.Commands;
using Domain.Entities;
using Domain.Enums;
using Domain.ValueObjects;
using FluentAssertions;
using Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace UnitTests.Orders;

public sealed class MarkServedHandlerTests
{
    private sealed class NoOpNotifier : IKitchenTicketNotifier
    {
        public Task TicketBatchCreatedAsync(IEnumerable<Application.Kds.Queries.KitchenTicketDto> tickets, CancellationToken ct = default) => Task.CompletedTask;
        public Task TicketChangedAsync(Application.Kds.Queries.KitchenTicketDto ticket, CancellationToken ct = default) => Task.CompletedTask;
    }

    [Fact]
    public async Task MarkServed_MarksAllKitchenTicketsServed_WhenNotCancelled()
    {
        var options = new DbContextOptionsBuilder<TableOrderingDbContext>()
            .UseInMemoryDatabase("orders_mark_served_db")
            .Options;

        var db = new TableOrderingDbContext(options, new NoOpDispatcher());

        var orderId = Guid.NewGuid();
        var tableId = Guid.NewGuid();
        var order = Order.Start(orderId, tableId, "ORD-1");
        order.AddItem(Guid.NewGuid(), "Pho", new Money(50000, "VND"), new Quantity(1));
        order.Submit();
        order.MarkInProgress();
        order.MarkReady();

        db.Orders.Add(order);

        var t1 = new KitchenTicket(Guid.NewGuid(), orderId, 1, "Pho", 1); // New
        var t2 = new KitchenTicket(Guid.NewGuid(), orderId, 2, "Bun", 1);
        t2.Start(); // InProgress
        var t3 = new KitchenTicket(Guid.NewGuid(), orderId, 3, "Com", 1);
        t3.Start();
        t3.MarkReady(); // Ready
        var t4 = new KitchenTicket(Guid.NewGuid(), orderId, 4, "Tra", 1);
        t4.Cancel("no"); // Cancelled

        db.KitchenTickets.AddRange(t1, t2, t3, t4);
        await db.SaveChangesAsync();

        var handler = new MarkServedHandler(db, new NoOpNotifier());

        await handler.Handle(new MarkServedCommand(orderId), CancellationToken.None);

        var tickets = await db.KitchenTickets.Where(x => x.OrderId == orderId).ToListAsync();
        tickets.Single(x => x.Id == t1.Id).Status.Should().Be(KitchenTicketStatus.Served);
        tickets.Single(x => x.Id == t2.Id).Status.Should().Be(KitchenTicketStatus.Served);
        tickets.Single(x => x.Id == t3.Id).Status.Should().Be(KitchenTicketStatus.Served);
        tickets.Single(x => x.Id == t4.Id).Status.Should().Be(KitchenTicketStatus.Cancelled);
    }

    private sealed class NoOpDispatcher : IDomainEventDispatcher
    {
        public Task DispatchAsync(IEnumerable<Domain.Abstractions.IDomainEvent> domainEvents, CancellationToken ct = default) => Task.CompletedTask;
    }
}
