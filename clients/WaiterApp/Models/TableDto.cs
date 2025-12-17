namespace WaiterApp.Models;

using TableOrdering.Contracts;

public class TableDto
{
    public Guid Id { get; set; }
    public string Code { get; set; } = string.Empty;
    public TableStatus Status { get; set; } // Available, InUse, ...
    public int Seats { get; set; }
    public DateTimeOffset? TimeSeated { get; set; }

    public string? SessionId { get; set; }

    // UI helpers
    public string StatusText => Status switch
    {
        TableStatus.Available => "Trống",
        TableStatus.InUse => "Đang phục vụ",
        TableStatus.Reserved => "Đã Phục vụ"
    };

    public Microsoft.Maui.Graphics.Color StatusColor => Status switch
    {
        TableStatus.Available => Microsoft.Maui.Graphics.Colors.Green,
        TableStatus.InUse => Microsoft.Maui.Graphics.Colors.Orange,
        TableStatus.Reserved => Microsoft.Maui.Graphics.Colors.LightGray
    };
}
