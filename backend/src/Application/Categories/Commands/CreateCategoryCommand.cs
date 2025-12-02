using Application.Abstractions;
using Application.Dtos;

namespace Application.Categories.Commands;

public sealed record CreateCategoryCommand(
    string Name,
    string? Description,
    int SortOrder = 0
) : ICommand<CategoryDto>;
