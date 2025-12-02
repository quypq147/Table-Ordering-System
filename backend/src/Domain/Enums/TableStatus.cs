namespace Domain.Enums;

public enum TableStatus
{
    // Bàn trống, chưa có order đang hoạt động
    Available = 0,
    // Bàn đang có khách / có order (gộp các trạng thái Occupied/Reserved trước đây)
    InUse = 1
}
