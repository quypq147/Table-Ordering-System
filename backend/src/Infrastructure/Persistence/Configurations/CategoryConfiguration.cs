using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Persistence.Configurations;

public sealed class CategoryConfiguration : IEntityTypeConfiguration<Category>
{
    public void Configure(EntityTypeBuilder<Category> b)
    {
        b.ToTable("Categories");
        b.HasKey(x => x.Id);

        b.Property(x => x.Code)
            .IsRequired()
            .HasMaxLength(64);

        b.Property(x => x.Name)
            .IsRequired()
            .HasMaxLength(120);

        b.Property(x => x.Description)
            .HasMaxLength(512);

        b.Property(x => x.IsActive)
            .HasDefaultValue(true);

        b.Property(x => x.SortOrder)
            .HasDefaultValue(0);

        b.HasIndex(x => x.Code).IsUnique();
        b.HasIndex(x => x.Name);
    }
}

