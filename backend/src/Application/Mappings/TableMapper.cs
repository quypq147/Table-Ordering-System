// Application/Mappings/TableMapper.cs
using Application.Dtos;
using Domain.Entities;

namespace Application.Mappings;

public static class TableMapper
{
    public static TableDto ToDto(Table t)
        => new(t.Id, t.Code, t.Seats, t.Status);
}

