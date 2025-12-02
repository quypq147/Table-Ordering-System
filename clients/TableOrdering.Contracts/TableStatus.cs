namespace TableOrdering.Contracts;

public enum TableStatus
{
    // Bàn trống, chưa có order đang hoạt động
    Available = 0,
    // Bàn đang có khách / có order
    InUse = 1
}
