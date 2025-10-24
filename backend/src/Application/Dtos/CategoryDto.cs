using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

// Application/Dtos/CategoryDto.cs
namespace Application.Dtos;

public sealed record CategoryDto(
    string Id,
    string Name,
    string? Description,
    bool IsActive,
    int SortOrder
);
