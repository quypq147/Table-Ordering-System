using Application.Abstractions;
using Application.Dtos;

namespace Application.Tables.Commands
{
    public sealed record CreateTableCommand(Guid Id, string Code, int Seats) : ICommand<TableDto>;
}
