// Get by Id
using Application.Abstractions;
using Application.Dtos;

namespace Application.Categories.Queries;

public sealed record GetCategoryByIdQuery(string Id) : IQuery<CategoryDto?>;

