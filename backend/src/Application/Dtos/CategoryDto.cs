// Application/Dtos/CategoryDto.cs
namespace Application.Dtos;

public sealed record CategoryDto(
    Guid Id,
    string Name,
    string? Description,
    bool IsActive,
    int SortOrder
);
