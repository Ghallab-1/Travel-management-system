using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TravelManagement.Domain.Entities;

namespace TravelManagement.Infrastructure.Persistence.Configurations;

public class HotelConfiguration : IEntityTypeConfiguration<Hotel>
{
    public void Configure(EntityTypeBuilder<Hotel> builder)
    {
        builder.ToTable("Hotels");

        builder.HasKey(h => h.HotelId);

        builder.Property(h => h.HotelName)
            .IsRequired()
            .HasMaxLength(150);

        builder.Property(h => h.Address)
            .HasMaxLength(250);

        builder.Property(h => h.Phone)
            .HasMaxLength(30);

        builder.Property(h => h.IsActive)
            .HasDefaultValue(true);

        builder.HasOne(h => h.City)
            .WithMany(c => c.Hotels)
            .HasForeignKey(h => h.CityId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}