using Application.Abstractions;
using Application.Dtos;

namespace Application.Tables.Commands;

public sealed record UpdateTableCommand(Guid Id, string Code, int Seats) : ICommand<TableDto>;