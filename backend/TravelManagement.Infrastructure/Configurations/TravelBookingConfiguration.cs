using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TravelManagement.Domain.Entities;

namespace TravelManagement.Infrastructure.Persistence.Configurations;

public class TravelBookingConfiguration : IEntityTypeConfiguration<TravelBooking>
{
    public void Configure(EntityTypeBuilder<TravelBooking> builder)
    {
        builder.ToTable("TravelBookings");

        builder.HasKey(tb => tb.TravelBookingId);

        builder.Property(tb => tb.BookingStatus)
            .IsRequired()
            .HasMaxLength(50);

        builder.Property(tb => tb.Notes)
            .HasMaxLength(1000);

        builder.HasOne(tb => tb.TravelRequest)
            .WithMany(tr => tr.TravelBookings)
            .HasForeignKey(tb => tb.TravelRequestId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}