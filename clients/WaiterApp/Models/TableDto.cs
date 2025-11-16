namespace WaiterApp.Models;

public class TableDto
{
    public Guid Id { get; set; }
    public string Code { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty; // Available, InUse, ...
    public int Seats { get; set; }
}
