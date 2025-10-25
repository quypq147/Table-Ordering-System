namespace AdminWeb.Dtos
{
    public record MenuItemDto(Guid Id, Guid CategoryId, string Name, string Sku, decimal Price, bool IsActive);
    public record CreateMenuItemRequest(Guid CategoryId, string Name, string Sku, decimal Price);
    public record Paginated<T>(List<T> Items = null!, int Page = 1, int PageSize = 20, int Total = 0);
    public record OrderSummaryDto(Guid Id, string Code, string Status, decimal Total, DateTime CreatedAt);
    public record OrderDetailDto(Guid Id, string Code, string Status, decimal Subtotal, decimal Discount, decimal Total, DateTime CreatedAt, List<OrderItemRow> Items);
    public record OrderItemRow(string Name, int Qty, decimal UnitPrice, decimal LineTotal, string? Note);
    public record KitchenTicketDto(Guid Id, Guid OrderId, Guid StationId, string Status, string ItemName, int Qty, DateTime CreatedAt);
    public record DiningTableDto(Guid Id, string Code, string Name, int SeatCount, bool IsActive, string? QrToken);
    public record CreateTableRequest(string Code, string Name, int SeatCount, bool IsActive);
    public record UpdateTableRequest(string Code, string Name, int SeatCount, bool IsActive);

    public record CategoryDto(Guid Id, Guid RestaurantId, string Name, int DisplayOrder, bool IsActive);
    

}
