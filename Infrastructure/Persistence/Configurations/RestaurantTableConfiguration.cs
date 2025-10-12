﻿using Domain.Entities;
using Domain.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Persistence.Configurations;

public class RestaurantTableConfiguration : IEntityTypeConfiguration<RestaurantTable>
{
    public void Configure(EntityTypeBuilder<RestaurantTable> b)
    {
        b.ToTable("RestaurantTables");
        b.HasKey(x => x.Id);
        b.Property(x => x.Id).HasMaxLength(64);

        b.Property(x => x.Code).HasMaxLength(16).IsRequired();
        b.HasIndex(x => x.Code).IsUnique();

        b.Property(x => x.Seats).IsRequired();

        // Lưu enum dạng string cho dễ đọc/log (có thể đổi sang int nếu ưu tiên hiệu năng)
        b.Property(x => x.Status)
         .HasConversion<string>()
         .HasMaxLength(32)
         .HasDefaultValue(TableStatus.Available);
    }
}

