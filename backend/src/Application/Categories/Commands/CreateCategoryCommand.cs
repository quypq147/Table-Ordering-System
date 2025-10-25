using Application.Abstractions;
using Application.Dtos;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Categories.Commands;

public sealed record CreateCategoryCommand(
    string Id, string Name, string? Description = null, int SortOrder = 0
) : ICommand<CategoryDto>;
