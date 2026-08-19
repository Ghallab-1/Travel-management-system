using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TravelManagement.Domain.Entities;

namespace TravelManagement.Infrastructure.Persistence.Configurations;

public class VisaConfiguration : IEntityTypeConfiguration<Visa>
{
    public void Configure(EntityTypeBuilder<Visa> builder)
    {
        builder.ToTable("Visas");

        builder.HasKey(v => v.VisaId);

        builder.Property(v => v.VisaType)
            .IsRequired()
            .HasMaxLength(50);

        builder.Property(v => v.Status)
            .IsRequired()
            .HasMaxLength(50);

        builder.HasOne(v => v.TravelBooking)
            .WithMany(tb => tb.Visas)
            .HasForeignKey(v => v.TravelBookingId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(v => v.Country)
            .WithMany(c => c.Visas)
            .HasForeignKey(v => v.CountryId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}