using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TravelManagement.Domain.Entities;

namespace TravelManagement.Infrastructure.Persistence.Configurations;

public class CurrencyConfiguration : IEntityTypeConfiguration<Currency>
{
    public void Configure(EntityTypeBuilder<Currency> builder)
    {
        builder.ToTable("Currencies");

        builder.HasKey(c => c.CurrencyId);

        builder.Property(c => c.CurrencyCode)
            .IsRequired()
            .HasMaxLength(10);

        builder.Property(c => c.CurrencyName)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(c => c.ExchangeRate)
            .HasPrecision(18, 6);

        builder.Property(c => c.LastUpdated)
            .HasDefaultValueSql("CURRENT_TIMESTAMP");

        builder.HasIndex(c => c.CurrencyCode)
            .IsUnique();
    }
}