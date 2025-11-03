using Application.Abstractions;
using Application.Dtos;

namespace Application.Categories.Commands;

public sealed record ActivateCategoryCommand(Guid Id) : ICommand<CategoryDto>;
