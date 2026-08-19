using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TravelManagement.Domain.Entities;

namespace TravelManagement.Infrastructure.Persistence.Configurations;

public class CountryConfiguration : IEntityTypeConfiguration<Country>
{
    public void Configure(EntityTypeBuilder<Country> builder)
    {
        builder.ToTable("Countries");

        builder.HasKey(c => c.CountryId);

        builder.Property(c => c.CountryName)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(c => c.CountryCode)
            .IsRequired()
            .HasMaxLength(10);

        builder.HasIndex(c => c.CountryCode)
            .IsUnique();

        builder.HasIndex(c => c.CountryName)
            .IsUnique();
    }
}