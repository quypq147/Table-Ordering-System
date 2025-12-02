namespace WaiterApp.Models;

public class KitchenTicketDto
{
    public Guid Id { get; set; }
    public Guid OrderId { get; set; }
    public string OrderCode { get; set; } = string.Empty;
    public string TableName { get; set; } = string.Empty;
    public Guid StationId { get; set; }
    public string Status { get; set; } = string.Empty;
    public string ItemName { get; set; } = string.Empty;
    public int Qty { get; set; }
    public DateTime CreatedAt { get; set; }
    public string? TableCode { get; set; }
}
