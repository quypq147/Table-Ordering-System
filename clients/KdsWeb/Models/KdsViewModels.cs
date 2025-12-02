namespace KdsWeb.Models;

public class KitchenTicketVm
{
    public Guid TicketId { get; set; }
    public Guid OrderId { get; set; }
    public string OrderCode { get; set; } = default!;
    public string TableName { get; set; } = default!;
    public string ItemName { get; set; } = default!;
    public int Quantity { get; set; }
    public string Status { get; set; } = default!;
    public DateTime Time { get; set; }
}

public class OrderTicketVm
{
    public Guid OrderId { get; set; }
    public string OrderCode { get; set; } = default!;
    public string TableName { get; set; } = default!;
    public List<KitchenTicketVm> Tickets { get; set; } = new();
}

