namespace WaiterApp.Models;

public class OrderItemDto
{
    public Guid Id { get; set; }
    public string ItemName { get; set; } = string.Empty;
    public int Quantity { get; set; }
    public string? Note { get; set; }
    public string Status { get; set; } = string.Empty; // Pending, Preparing, Done, Served...
}
