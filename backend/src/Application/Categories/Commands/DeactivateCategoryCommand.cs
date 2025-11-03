using Application.Abstractions;
using Application.Dtos;
using Application.Mappings;
using Microsoft.EntityFrameworkCore;

namespace Application.Categories.Commands;
public sealed record DeactivateCategoryCommand(Guid Id) : ICommand<CategoryDto>;
