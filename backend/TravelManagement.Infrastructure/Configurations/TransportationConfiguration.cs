using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TravelManagement.Domain.Entities;

namespace TravelManagement.Infrastructure.Persistence.Configurations;

public class TransportationConfiguration : IEntityTypeConfiguration<Transportation>
{
    public void Configure(EntityTypeBuilder<Transportation> builder)
    {
        builder.ToTable("Transportations");

        builder.HasKey(t => t.TransportationId);

        builder.Property(t => t.TransportationType)
            .IsRequired()
            .HasMaxLength(50);

        builder.Property(t => t.ProviderName)
            .HasMaxLength(100);

        builder.Property(t => t.DriverName)
            .HasMaxLength(100);

        builder.Property(t => t.DriverPhone)
            .HasMaxLength(30);

        builder.Property(t => t.PickupLocation)
            .HasMaxLength(150);

        builder.Property(t => t.DropOffLocation)
            .HasMaxLength(150);

        builder.HasOne(t => t.TravelBooking)
            .WithMany(tb => tb.Transportations)
            .HasForeignKey(t => t.TravelBookingId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}