using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TravelManagement.Domain.Entities;

namespace TravelManagement.Infrastructure.Persistence.Configurations;

public class FlightConfiguration : IEntityTypeConfiguration<Flight>
{
    public void Configure(EntityTypeBuilder<Flight> builder)
    {
        builder.ToTable("Flights");

        builder.HasKey(f => f.FlightId);

        builder.Property(f => f.FlightNumber)
            .IsRequired()
            .HasMaxLength(20);

        builder.Property(f => f.BookingReference)
            .HasMaxLength(50);

        builder.Property(f => f.DepartureAirport)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(f => f.ArrivalAirport)
            .IsRequired()
            .HasMaxLength(100);

        builder.HasOne(f => f.TravelBooking)
            .WithMany(tb => tb.Flights)
            .HasForeignKey(f => f.TravelBookingId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(f => f.Airline)
            .WithMany(a => a.Flights)
            .HasForeignKey(f => f.AirlineId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}