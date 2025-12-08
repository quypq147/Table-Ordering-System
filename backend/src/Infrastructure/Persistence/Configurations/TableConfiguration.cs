using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Persistence.Configurations;

public class TableConfiguration : IEntityTypeConfiguration<Table>
{
    public void Configure(EntityTypeBuilder<Table> b)
    {
        b.ToTable("Tables");
        b.HasKey(x => x.Id);
        b.Property(x => x.Id).HasMaxLength(64);

        b.Property(x => x.Code).HasMaxLength(20).IsRequired();
        b.HasIndex(x => x.Code).IsUnique();

        b.Property(x => x.Seats).IsRequired();

        // Lưu enum dạng string cho dễ đọc/log (có thể đổi sang int nếu ưu tiên hiệu năng)
        b.Property(x => x.Status).HasConversion<string>().HasMaxLength(16).IsRequired();

        // Seats >=1
        b.ToTable(x => x.HasCheckConstraint("CK_Tables_Seats_Positive", "[Seats] >=1"));

        // sequence for Number
        b.Property(x => x.Number)
            .HasDefaultValueSql("NEXT VALUE FOR TableNoSeq")
            .ValueGeneratedOnAdd();

        // Session Id (nullable)
        b.Property(x => x.CurrentSessionId).IsRequired(false);
    }
}

