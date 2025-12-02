using Application.Abstractions;
using Application.Dtos;

namespace Application.Tables.Commands
{
    public sealed record MarkTableReservedCommand(Guid Id) : ICommand<TableDto>;
}
