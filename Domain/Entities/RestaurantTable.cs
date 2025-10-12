using Domain.Abstractions;
using Domain.Enums;

namespace Domain.Entities;

public class RestaurantTable : Entity<string>
{
    public string Code { get; private set; } = default!; // e.g., "T01"
    public int Seats { get; private set; }
    public TableStatus Status { get; private set; } = TableStatus.Available;

    private RestaurantTable() { }

    public RestaurantTable(string id, string code, int seats) : base(id)
    {
        Code = string.IsNullOrWhiteSpace(code) ? throw new ArgumentNullException(nameof(code)) : code.Trim();
        Seats = seats > 0 ? seats : throw new ArgumentOutOfRangeException(nameof(seats));
    }

    public void MarkReserved() => Status = TableStatus.Reserved;
    public void MarkOccupied() => Status = TableStatus.Occupied;
    public void MarkAvailable() => Status = TableStatus.Available;
}
