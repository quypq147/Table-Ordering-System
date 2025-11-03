using Application.Abstractions;
using Application.Dtos;
using Application.Mappings;
using Application.Common;
using Microsoft.EntityFrameworkCore;

namespace Application.Categories.Commands;

public sealed record ChangeCategorySortOrderCommand(Guid Id, int SortOrder) : ICommand<CategoryDto>;
