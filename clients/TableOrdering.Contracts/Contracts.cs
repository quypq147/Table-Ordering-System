namespace TableOrdering.Contracts;

// Shared primitives
public record Paginated<T>(List<T> Items, int Page, int PageSize, int Total);

// Categories
public record CategoryDto(Guid Id, string Name, int DisplayOrder, bool IsActive);
public record CreateCategoryRequest(string Name, int DisplayOrder);
public record UpdateCategoryRequest(string Name, int DisplayOrder, bool IsActive);
public record RenameCategoryRequest(string Name);

// Menu Items
public record MenuItemDto(Guid Id, Guid CategoryId, string Name, string Sku, decimal Price, string Currency, bool IsActive, string? AvatarImageUrl, string? BackgroundImageUrl);
public record CreateMenuItemRequest(Guid CategoryId, string Name, string Sku, decimal Price, string Currency, string? AvatarImageUrl, string? BackgroundImageUrl);
public record UpdateMenuItemImagesRequest(Guid Id, string? AvatarImageUrl, string? BackgroundImageUrl);

// Orders
public record OrderItemRow(string Name, int Qty, decimal UnitPrice, decimal LineTotal, string? Note);
public record OrderSummaryDto(Guid Id, string Code, string Status, decimal Total, DateTime CreatedAt);
public record OrderDetailDto(Guid Id, string Code, string Status, decimal Subtotal, decimal Discount, decimal Total, DateTime CreatedAt, List<OrderItemRow> Items);

// KDS
public record KitchenTicketDto(Guid Id, Guid OrderId, string OrderCode, string TableName, Guid StationId, string Status, string ItemName, int Qty, DateTime CreatedAt, string? TableCode);

// Tables
public record DiningTableDto(Guid Id, string Code, int Seats, TableStatus Status);
public record CreateTableRequest(string Code, int Seats);
public record UpdateTableRequest(string Code, int Seats, TableStatus Status);

// Public Cart (Customer)
public record CartItemDto(int Id, Guid MenuItemId, string Name, decimal UnitPrice, int Quantity, string? Note, decimal LineTotal);
public record CartDto(Guid OrderId, string TableCode, string Status, IReadOnlyList<CartItemDto> Items, decimal Subtotal, decimal ServiceCharge, decimal Tax, decimal Total);
