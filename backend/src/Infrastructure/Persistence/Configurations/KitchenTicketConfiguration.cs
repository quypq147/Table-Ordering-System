using Domain.Entities;
using Domain.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Persistence.Configurations;

public sealed class KitchenTicketConfiguration : IEntityTypeConfiguration<KitchenTicket>
{
    public void Configure(EntityTypeBuilder<KitchenTicket> b)
    {
        b.ToTable("KitchenTickets");
        b.HasKey(x => x.Id);
        b.Property(x => x.ItemName).IsRequired().HasMaxLength(200);
        b.Property(x => x.Quantity).IsRequired();
        b.Property(x => x.Status).HasConversion<string>().HasMaxLength(16).HasDefaultValue(KitchenTicketStatus.New).IsRequired();
        b.Property(x => x.CreatedAtUtc).IsRequired();
        b.Property(x => x.StartedAtUtc);
        b.Property(x => x.ReadyAtUtc);
        b.Property(x => x.ServedAtUtc);
        b.HasIndex(x => new { x.Status, x.CreatedAtUtc });
        b.HasIndex(x => x.OrderId);
    }
}
