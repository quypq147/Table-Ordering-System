using Application.Abstractions;
using Application.Dtos;

namespace Application.Categories.Commands;

public sealed record ChangeCategorySortOrderCommand(Guid Id, int SortOrder) : ICommand<CategoryDto>;
