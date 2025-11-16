namespace WaiterApp.Models;

public class KitchenTicketDto
{
    public Guid Id { get; set; }
    public Guid TableId { get; set; }
    public string ItemName { get; set; } = string.Empty;
    public int Quantity { get; set; }
    public string Status { get; set; } = string.Empty; // Pending/Done/...
    public DateTime CreatedAt { get; set; }
}
