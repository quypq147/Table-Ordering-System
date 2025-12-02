using Application.Abstractions;
using Application.Dtos;

namespace Application.Categories.Commands;

public sealed record DeactivateCategoryCommand(Guid Id) : ICommand<CategoryDto>;
