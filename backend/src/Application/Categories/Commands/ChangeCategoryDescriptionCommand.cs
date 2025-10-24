using Application.Abstractions;
using Application.Dtos;

namespace Application.Categories.Commands;

public sealed record ChangeCategoryDescriptionCommand(string Id, string? Description) : ICommand<CategoryDto>;
