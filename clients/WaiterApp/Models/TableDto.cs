namespace WaiterApp.Models;

using TableOrdering.Contracts;

public class TableDto
{
    public Guid Id { get; set; }
    public string Code { get; set; } = string.Empty;
    public TableStatus Status { get; set; } // Available, InUse, ...
    public int Seats { get; set; }

    // UI helpers
    public string StatusText => Status switch
    {
        TableStatus.Available => "Tr?ng",
        TableStatus.InUse => "?ang ph?c v?",
        _ => "Không rõ"
    };

    public Microsoft.Maui.Graphics.Color StatusColor => Status switch
    {
        TableStatus.Available => Microsoft.Maui.Graphics.Colors.Green,
        TableStatus.InUse => Microsoft.Maui.Graphics.Colors.Orange,
        _ => Microsoft.Maui.Graphics.Colors.LightGray
    };
}
