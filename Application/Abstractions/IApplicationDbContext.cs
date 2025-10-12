// Application/Common/Interfaces/IApplicationDbContext.cs
using Domain.Entities;
using Microsoft.EntityFrameworkCore; // Pragmatic: chấp nhận DbSet trong Application

namespace Application.Common.Interfaces;

public interface IApplicationDbContext
{
    DbSet<MenuItem> MenuItems { get; }
    DbSet<Order> Orders { get; }
    DbSet<RestaurantTable> Tables { get; }
    DbSet<Voucher> Vouchers { get; }
    Task<int> SaveChangesAsync(CancellationToken ct = default);
}

