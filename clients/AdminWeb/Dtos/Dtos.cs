using System.ComponentModel.DataAnnotations;

namespace AdminWeb.Dtos
{
    public record MenuItemDto(Guid Id, Guid CategoryId, string Name, string Sku, decimal Price, bool IsActive);

    public record CreateMenuItemRequest(
        [Required, GuidNotEmpty] Guid CategoryId,
        [Required, StringLength(200, MinimumLength = 1)] string Name,
        [Required, StringLength(64, MinimumLength = 1)] string Sku,
        [Range(0.01, 1_000_000)] decimal Price,
        [Required, StringLength(3, MinimumLength = 3)] string Currency
    );

    public record Paginated<T>(List<T> Items = null!, int Page = 1, int PageSize = 20, int Total = 0);
    public record OrderSummaryDto(Guid Id, string Code, string Status, decimal Total, DateTime CreatedAt);
    public record OrderDetailDto(Guid Id, string Code, string Status, decimal Subtotal, decimal Discount, decimal Total, DateTime CreatedAt, List<OrderItemRow> Items);
    public record OrderItemRow(string Name, int Qty, decimal UnitPrice, decimal LineTotal, string? Note);
    public record KitchenTicketDto(Guid Id, Guid OrderId, Guid StationId, string Status, string ItemName, int Qty, DateTime CreatedAt);

    public record DiningTableDto(Guid Id, string Code, int Seats, int Status);
    public record CreateTableRequest(string Code, int Seats, int Status);
    public record UpdateTableRequest(string Code, int Seats, int Status);

    public record CategoryDto(Guid Id ,string Name, int DisplayOrder, bool IsActive);
    public record CreateCategoryRequest(string Name, int DisplayOrder);
    public record UpdateCategoryRequest(string Name, int DisplayOrder, bool IsActive);
    public record RenameCategoryRequest(
        [Required, StringLength(128, MinimumLength = 1)] string Name
    );

    // Validation attribute to prevent Guid.Empty
    public sealed class GuidNotEmptyAttribute : ValidationAttribute
    {
        protected override ValidationResult? IsValid(object? value, ValidationContext _)
            => value is Guid g && g != Guid.Empty
               ? ValidationResult.Success
               : new ValidationResult("Vui lòng chọn danh mục hợp lệ.");
    }
}
