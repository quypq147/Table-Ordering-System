using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Persistence.Configurations;

public sealed class CategoryConfiguration : IEntityTypeConfiguration<Category>
{
    public void Configure(EntityTypeBuilder<Category> b)
    {
        b.ToTable("Categories");
        b.HasKey(c => c.Id);

        b.Property(c => c.Code)
            .IsRequired()
            .HasMaxLength(32);

        b.HasIndex(c => c.Code).IsUnique();

        b.Property(c => c.Name)
            .IsRequired()
            .HasMaxLength(200);

        b.Property(c => c.IsActive)
            .HasDefaultValue(true);

        // map sequence to Number
        b.Property(c => c.Number)
            .HasDefaultValueSql("NEXT VALUE FOR CategoryNoSeq")
            .ValueGeneratedOnAdd();
    }
}

