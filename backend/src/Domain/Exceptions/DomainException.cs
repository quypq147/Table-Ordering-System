namespace Domain.Exceptions;

public class DomainException : Exception
{
    public string Code { get; }
    public int? HttpStatusOverride { get; }

    public DomainException(string code, string message, int? httpStatus = null) : base(message)
    {
        Code = code;
        HttpStatusOverride = httpStatus;
    }

    // Generic factories
    public static DomainException NotReadyForPayment() => new("ORDER_NOT_READY_FOR_PAYMENT", "Đơn chưa đủ điều kiện thanh toán.", 409);
    public static DomainException AlreadyPaid() => new("ORDER_ALREADY_PAID", "Đơn đã thanh toán.", 409);
    public static DomainException Cancelled() => new("ORDER_CANCELLED", "Đơn đã huỷ.", 409);
    public static DomainException InvalidState(string message) => new("ORDER_INVALID_STATE", message, 400);
    public static DomainException NotFound(string entity, Guid id) => new("NOT_FOUND", $"Không tìm thấy {entity} với Id={id}.", 404);
}
