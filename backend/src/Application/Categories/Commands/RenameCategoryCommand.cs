using Application.Abstractions;
using Application.Dtos;
using Application.Mappings;
using Application.Common;
using Microsoft.EntityFrameworkCore;

namespace Application.Categories.Commands;

public sealed record RenameCategoryCommand(Guid Id, string NewName) : ICommand<CategoryDto>;
