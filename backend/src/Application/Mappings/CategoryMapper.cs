// Application/Mappings/CategoryMapper.cs
using Application.Dtos;
using Domain.Entities;

namespace Application.Mappings;

public static class CategoryMapper
{
    public static CategoryDto ToDto(Category c)
        => new(c.Id, c.Name, c.Description, c.IsActive, c.SortOrder);
}

