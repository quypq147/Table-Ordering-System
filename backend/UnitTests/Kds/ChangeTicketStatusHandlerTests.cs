using Application.Abstractions;
using Application.Kds.Commands;
using Application.Kds.Queries;
using Domain.Entities;
using Domain.Enums;
using FluentAssertions;
using Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace UnitTests.Kds;

public sealed class ChangeTicketStatusHandlerTests
{
    private sealed class NoOpNotifier : IKitchenTicketNotifier
    {
        public Task TicketBatchCreatedAsync(IEnumerable<KitchenTicketDto> tickets, CancellationToken ct = default) => Task.CompletedTask;
        public Task TicketChangedAsync(KitchenTicketDto ticket, CancellationToken ct = default) => Task.CompletedTask;
    }

    private sealed class NoOpCustomerNotifier : ICustomerNotifier
    {
        public Task OrderStatusChangedAsync(Guid orderId, string status, CancellationToken ct = default) => Task.CompletedTask;
        public Task OrderPaidAsync(Guid orderId, decimal amount, string currency, string method, DateTime paidAtUtc, CancellationToken ct = default) => Task.CompletedTask;
    }

    [Fact]
    public async Task ChangeStatus_TransitionsCorrectly()
    {
        var options = new DbContextOptionsBuilder<TableOrderingDbContext>()
        .UseInMemoryDatabase("kds_change_status_db")
        .Options;
        var db = new TableOrderingDbContext(options, new NoOpDispatcher());
        var ticket = new KitchenTicket(Guid.NewGuid(), Guid.NewGuid(), 1, "Pho", 1);
        db.KitchenTickets.Add(ticket);
        await db.SaveChangesAsync();
        var handler = new ChangeTicketStatusHandler(db, new NoOpNotifier(), new NoOpCustomerNotifier());

        var startDto = await handler.Handle(new ChangeTicketStatusCommand(ticket.Id, "start"), CancellationToken.None);
        startDto.Status.Should().Be(KitchenTicketStatus.InProgress.ToString());
        var doneDto = await handler.Handle(new ChangeTicketStatusCommand(ticket.Id, "done"), CancellationToken.None);
        doneDto.Status.Should().Be(KitchenTicketStatus.Ready.ToString());
        var servedDto = await handler.Handle(new ChangeTicketStatusCommand(ticket.Id, "served"), CancellationToken.None);
        servedDto.Status.Should().Be(KitchenTicketStatus.Served.ToString());
    }

    private sealed class NoOpDispatcher : IDomainEventDispatcher
    {
        public Task DispatchAsync(IEnumerable<Domain.Abstractions.IDomainEvent> domainEvents, CancellationToken ct = default) => Task.CompletedTask;
    }
}
