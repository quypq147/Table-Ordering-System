namespace Domain.Entities;

public sealed class ChatMessage
{
    public long Id { get; set; }
    public string TableKey { get; set; } = string.Empty; // Có th? là mã bàn ho?c GUID tu? client g?i
    public string Sender { get; set; } = string.Empty;   // "customer" | "staff"
    public string Message { get; set; } = string.Empty;
    public DateTime SentAtUtc { get; set; } = DateTime.UtcNow;
}
