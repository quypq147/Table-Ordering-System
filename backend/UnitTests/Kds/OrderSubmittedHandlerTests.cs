using Application.Abstractions;
using Application.Kds.Queries;
using Application.Orders.Events.Handlers;
using Domain.Entities;
using Domain.Enums;
using Domain.Events;
using Domain.Repositories;
using Domain.ValueObjects;
using FluentAssertions;
using Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging.Abstractions;
using Xunit;

namespace UnitTests.Kds;

public sealed class OrderSubmittedHandlerTests
{
    private sealed class TestOrderRepository : IOrderRepository
    {
        private readonly Order _order;
        public TestOrderRepository(Order order) => _order = order;
        public Task<Order?> GetByIdAsync(Guid id, CancellationToken ct = default) => Task.FromResult(id == _order.Id ? _order : null);
        public Task AddAsync(Order order) => Task.CompletedTask;
        public void Update(Order order) { }
        public Task<Order?> GetActiveOrderByTableIdAsync(Guid tableId, CancellationToken ct = default)
            => Task.FromResult(_order.TableId == tableId ? _order : null);
    }

    private sealed class NoOpNotifier : IKitchenTicketNotifier
    {
        public Task TicketBatchCreatedAsync(IEnumerable<KitchenTicketDto> tickets, CancellationToken ct = default) => Task.CompletedTask;
        public Task TicketChangedAsync(KitchenTicketDto ticket, CancellationToken ct = default) => Task.CompletedTask;
    }

    [Fact]
    public async Task HandleAsync_CreatesKitchenTickets_ForEachOrderItem()
    {
        // Arrange
        var options = new DbContextOptionsBuilder<TableOrderingDbContext>()
        .UseInMemoryDatabase("kds_test_db")
        .Options;
        var db = new TableOrderingDbContext(options, new NoOpDispatcher());
        var order = Order.Start(Guid.NewGuid(), Guid.NewGuid(), "TEST");
        order.AddItem(Guid.NewGuid(), "Pho Bo", new Money(50000, "VND"), new Quantity(2));
        order.AddItem(Guid.NewGuid(), "Tra Da", new Money(5000, "VND"), new Quantity(1));
        order.Submit();
        var repo = new TestOrderRepository(order);
        var handler = new OrderSubmittedHandler(new NullLogger<OrderSubmittedHandler>(), repo, db, new NoOpNotifier());
        var ev = new OrderSubmitted(order.Id);

        // Act
        await handler.HandleAsync(ev, CancellationToken.None);

        // Assert
        db.KitchenTickets.Count().Should().Be(2);
        var first = db.KitchenTickets.First(t => t.ItemName == "Pho Bo");
        first.Status.Should().Be(KitchenTicketStatus.New);
    }

    private sealed class NoOpDispatcher : IDomainEventDispatcher
    {
        public Task DispatchAsync(IEnumerable<Domain.Abstractions.IDomainEvent> domainEvents, CancellationToken ct = default) => Task.CompletedTask;
    }
}
