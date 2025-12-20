using Application;
using Application.Abstractions;
using Application.Public.Cart;
using Domain.Entities;
using Domain.Enums;
using FluentAssertions;
using Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace UnitTests.PublicCart;

public sealed class StartCartByTableCodeHandlerTests
{
    private sealed class FakeCodeGen : IOrderCodeGenerator
    {
        public Task<string> GenerateAsync(Guid tableId, string tableCode, CancellationToken ct = default)
            => Task.FromResult($"ORD-{tableCode}-001");
    }

    private sealed class NoOpDispatcher : IDomainEventDispatcher
    {
        public Task DispatchAsync(IEnumerable<Domain.Abstractions.IDomainEvent> domainEvents, CancellationToken ct = default)
            => Task.CompletedTask;
    }

    [Fact]
    public async Task StartCart_Twice_ReturnsSameDraftOrder_AndDoesNotRotateSession()
    {
        var options = new DbContextOptionsBuilder<TableOrderingDbContext>()
            .UseInMemoryDatabase("public_cart_start_idempotent_db")
            .Options;

        await using var db = new TableOrderingDbContext(options, new NoOpDispatcher());

        var table = new Table(Guid.NewGuid(), "T01", 4);
        db.Tables.Add(table);
        await db.SaveChangesAsync();

        var handler = new StartCartByTableCodeHandler(db, new FakeCodeGen());

        var first = await handler.Handle(new StartCartByTableCodeCommand("t01", null), CancellationToken.None);
        var sessionAfterFirst = table.CurrentSessionId;

        var second = await handler.Handle(new StartCartByTableCodeCommand("T01", sessionAfterFirst), CancellationToken.None);
        var sessionAfterSecond = table.CurrentSessionId;

        first.OrderId.Should().Be(second.OrderId);
        table.Status.Should().Be(TableStatus.InUse);
        sessionAfterFirst.Should().NotBeNull();
        sessionAfterSecond.Should().Be(sessionAfterFirst);

        (await db.Orders.CountAsync()).Should().Be(1);
    }
}
