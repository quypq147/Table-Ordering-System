// Application/Dtos/MenuItemDto.cs
namespace Application.Dtos;
public sealed record MenuItemDto(string Id, string Name, decimal Price, string Currency, bool IsActive);

