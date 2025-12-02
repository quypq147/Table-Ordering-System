using Application.Abstractions;

namespace Application.Tables.Commands;

public sealed record DeleteTableCommand(Guid Id) : ICommand<Unit>;

public readonly struct Unit
{
    public static readonly Unit Value = new();
}