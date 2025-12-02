using Application.Abstractions;
using Application.Dtos;

namespace Application.Tables.Commands
{
    public sealed record MarkTableAvailableCommand(Guid Id) : ICommand<TableDto>;
}
