using Application.Abstractions;
using Application.Dtos;

namespace Application.Categories.Commands;

public sealed record ChangeCategoryDescriptionCommand(Guid Id, string? Description) : ICommand<CategoryDto>;
