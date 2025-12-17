using System;

namespace WaiterApp.Models; // Hoặc namespace WaiterApp.Services tùy cấu trúc của bạn

public sealed class ChatMessagePayload
{
    public string TableId { get; set; } = string.Empty;
    public string Sender { get; set; } = string.Empty; // "customer" hoặc "staff"
    public string Message { get; set; } = string.Empty;
    public string ReceiverId { get; set; } = string.Empty;
    public DateTime SentAtUtc { get; set; }
}