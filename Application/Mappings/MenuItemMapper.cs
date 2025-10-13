﻿// Application/Mappings/MenuItemMapper.cs
using Application.Dtos;
using Domain.Entities;
namespace Application.Mappings;
public static class MenuItemMapper
{
    public static MenuItemDto ToDto(MenuItem m)
        => new(m.Id, m.Name, m.Price.Amount, m.Price.Currency, m.IsActive);
}

