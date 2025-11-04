namespace CustomerWeb.Dtos;

public record CategoryDto(Guid Id, string Name, string? Description, bool IsActive, int SortOrder);
public record MenuItemDto(Guid Id, Guid CategoryId, string Name, string Sku, decimal Price, bool IsActive);

// Cart DTOs (align with your backend contracts as needed)
public record CartItemDto(
    Guid Id,
    Guid MenuItemId,
    string Name,
    decimal UnitPrice,
    int Quantity,
    string? Note,
    decimal LineTotal
);

public record CartDto(
    Guid OrderId,
    string TableCode,
    string Status,
    IReadOnlyList<CartItemDto> Items,
    decimal Subtotal,
    decimal ServiceCharge,
    decimal Tax,
    decimal Total
);