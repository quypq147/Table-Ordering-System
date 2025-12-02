using Application.Abstractions;
using Application.Dtos;

namespace Application.Categories.Commands;

public sealed record RenameCategoryCommand(Guid Id, string NewName) : ICommand<CategoryDto>;
