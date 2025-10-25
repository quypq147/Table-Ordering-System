using Application.Abstractions;
using Application.Dtos;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Orders.Commands
{
    public sealed record CancelOrderCommand(string OrderId) : ICommand<OrderDto>;
}
