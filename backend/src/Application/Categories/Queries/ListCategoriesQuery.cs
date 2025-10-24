// List (search + onlyActive + paging + sort)
using System.Collections.Generic;
using Application.Abstractions;
using Application.Dtos;

namespace Application.Categories.Queries;

public sealed record ListCategoriesQuery(
    string? Search = null,
    bool? OnlyActive = null,
    int Page = 1,
    int PageSize = 20
) : IQuery<IReadOnlyList<CategoryDto>>;

