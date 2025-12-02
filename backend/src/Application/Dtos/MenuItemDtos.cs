// Application/Dtos/MenuItemDto.cs
namespace Application.Dtos;

public sealed record MenuItemDto(
    Guid Id,
    Guid? CategoryId,
    string Sku,
    string Name,
    decimal Price,
    string Currency,
    bool IsActive,
    string? AvatarImageUrl,
    string? BackgroundImageUrl
)
{

}

