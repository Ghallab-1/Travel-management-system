using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TravelManagement.Domain.Entities;

namespace TravelManagement.Infrastructure.Persistence.Configurations;

public class AirlineConfiguration : IEntityTypeConfiguration<Airline>
{
    public void Configure(EntityTypeBuilder<Airline> builder)
    {
        builder.ToTable("Airlines");

        builder.HasKey(a => a.AirlineId);

        builder.Property(a => a.AirlineName)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(a => a.AirlineCode)
            .IsRequired()
            .HasMaxLength(10);

        builder.Property(a => a.IsActive)
            .HasDefaultValue(true);

        builder.HasIndex(a => a.AirlineCode)
            .IsUnique();
    }
}