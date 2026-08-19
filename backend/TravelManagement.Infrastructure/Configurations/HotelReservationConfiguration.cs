using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TravelManagement.Domain.Entities;

namespace TravelManagement.Infrastructure.Persistence.Configurations;

public class HotelReservationConfiguration : IEntityTypeConfiguration<HotelReservation>
{
    public void Configure(EntityTypeBuilder<HotelReservation> builder)
    {
        builder.ToTable("HotelReservations");

        builder.HasKey(hr => hr.HotelReservationId);

        builder.Property(hr => hr.BookingReference)
            .HasMaxLength(50);

        builder.HasOne(hr => hr.TravelBooking)
            .WithMany(tb => tb.HotelReservations)
            .HasForeignKey(hr => hr.TravelBookingId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(hr => hr.Hotel)
            .WithMany(h => h.HotelReservations)
            .HasForeignKey(hr => hr.HotelId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}