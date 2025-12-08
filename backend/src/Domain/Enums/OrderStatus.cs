namespace Domain.Enums;

public enum OrderStatus
{
    Draft = 0,
    Submitted = 1,
    InProgress = 2,
    Ready = 3,
    Served = 4,
    Paid = 5,
    Cancelled = 6,
    WaitingForPayment = 7
}
