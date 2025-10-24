using Application.Abstractions;
using Application.Dtos;

namespace Application.Categories.Commands;

public sealed record ActivateCategoryCommand(string Id) : ICommand<CategoryDto>;
