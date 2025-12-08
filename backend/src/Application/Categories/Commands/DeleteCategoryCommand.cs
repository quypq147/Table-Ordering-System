using Application.Abstractions;

namespace Application.Categories.Commands;

public sealed record DeleteCategoryCommand(Guid Id) : ICommand<bool>;
