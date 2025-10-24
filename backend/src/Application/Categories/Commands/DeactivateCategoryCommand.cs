using Application.Abstractions;
using Application.Dtos;
using Application.Mappings;
using Microsoft.EntityFrameworkCore;

namespace Application.Categories.Commands;
public sealed record DeactivateCategoryCommand(string Id) : ICommand<CategoryDto>;
