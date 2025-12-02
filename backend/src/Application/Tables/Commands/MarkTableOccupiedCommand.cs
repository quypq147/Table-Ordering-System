using Application.Abstractions;
using Application.Dtos;

namespace Application.Tables.Commands
{
    public sealed record MarkTableOccupiedCommand(Guid Id) : ICommand<TableDto>;
}
