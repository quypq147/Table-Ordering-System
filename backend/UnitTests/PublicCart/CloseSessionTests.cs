using Application.Abstractions;
using Application.Orders.Commands;
using Application.Orders.Queries;
using Application.Tables.Commands;
using Domain.Entities;
using Domain.Enums;
using Domain.ValueObjects;
using FluentAssertions;
using Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace UnitTests.PublicCart;

public sealed class CloseSessionTests
{
    private sealed class NoOpDispatcher : IDomainEventDispatcher
    {
        public Task DispatchAsync(IEnumerable<Domain.Abstractions.IDomainEvent> domainEvents, CancellationToken ct = default) => Task.CompletedTask;
    }

    [Fact]
    public async Task CloseSession_DraftOrder_CancelsOrder_AndFreesTable()
    {
        var options = new DbContextOptionsBuilder<TableOrderingDbContext>()
            .UseInMemoryDatabase("public_cart_close_session_draft_db")
            .Options;

        await using var db = new TableOrderingDbContext(options, new NoOpDispatcher());

        var table = new Table(Guid.NewGuid(), "T01", 4);
        table.MarkInUse();
        var sessionBefore = table.CurrentSessionId;
        db.Tables.Add(table);

        var order = Order.Start(Guid.NewGuid(), table.Id, "ORD-1");
        db.Orders.Add(order);
        await db.SaveChangesAsync();

        // Simulate controller logic: if Draft -> cancel + mark available
        await new CancelOrderHandler(db)
            .Handle(new CancelOrderCommand(order.Id), CancellationToken.None);

        await new MarkTableAvailableHandler(db)
            .Handle(new MarkTableAvailableCommand(table.Id), CancellationToken.None);

        var reloadedTable = await db.Tables.FirstAsync(t => t.Id == table.Id);
        reloadedTable.Status.Should().Be(TableStatus.Available);
        reloadedTable.CurrentSessionId.Should().BeNull();
        sessionBefore.Should().NotBeNull();

        var reloadedOrder = await db.Orders.FirstAsync(o => o.Id == order.Id);
        reloadedOrder.OrderStatus.Should().Be(OrderStatus.Cancelled);
    }

    [Fact]
    public async Task CloseSession_NonDraftOrder_ShouldBeRejectedByPolicy()
    {
        // Policy-level test: a non-draft order is not eligible to be closed by customer.
        // Here we assert domain/state expectation used by controller logic.

        var order = Order.Start(Guid.NewGuid(), Guid.NewGuid(), "ORD-1");
        order.AddItem(Guid.NewGuid(), "Pho", new Money(50000, "VND"), new Quantity(1));
        order.Submit();

        order.OrderStatus.Should().NotBe(OrderStatus.Draft);
    }
}
