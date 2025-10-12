using System.Threading;
using System.Threading.Tasks;
using Application.Abstractions;
using Application.Dtos;
using Application.Mappings;
using Domain.Entities;
using Domain.Repositories;

namespace Application.Orders.Commands;

public record StartOrderCommand(string OrderId, string TableId) : ICommand<OrderDto>;

public class StartOrderHandler : ICommandHandler<StartOrderCommand, OrderDto>
{
    private readonly IOrderRepository _orders;
    private readonly ITableRepository _tables;
    private readonly IUnitOfWork _uow;

    public StartOrderHandler(IOrderRepository orders, ITableRepository tables, IUnitOfWork uow)
    {
        _orders = orders;
        _tables = tables;
        _uow = uow;
    }

    public async Task<OrderDto> Handle(StartOrderCommand command, CancellationToken ct)
    {
        var table = await _tables.GetByIdAsync(command.TableId);
        if (table is null) throw new InvalidOperationException("Table not found.");
        var order = Order.Start(command.OrderId, command.TableId);
        await _orders.AddAsync(order);
        await _uow.SaveChangesAsync(ct);
        return OrderMapper.ToDto(order);
    }
}