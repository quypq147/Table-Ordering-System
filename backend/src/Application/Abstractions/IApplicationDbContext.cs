﻿// Application/Common/Interfaces/IApplicationDbContext.cs
using Domain.Entities;
using Microsoft.EntityFrameworkCore; // Pragmatic: chấp nhận DbSet trong Application

namespace Application.Abstractions;

public interface IApplicationDbContext
{
    DbSet<MenuItem> MenuItems { get; }
    DbSet<Category> Categories { get; }
    DbSet<Order> Orders { get; }
    DbSet<Table> Tables { get; }
    DbSet<Voucher> Vouchers { get; }
    Task<int> SaveChangesAsync(CancellationToken ct = default);
}

