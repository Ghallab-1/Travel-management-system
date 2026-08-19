using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TravelManagement.Domain.Entities;

namespace TravelManagement.Infrastructure.Persistence.Configurations;

public class InsuranceConfiguration : IEntityTypeConfiguration<Insurance>
{
    public void Configure(EntityTypeBuilder<Insurance> builder)
    {
        builder.ToTable("Insurances");

        builder.HasKey(i => i.InsuranceId);

        builder.Property(i => i.ProviderName)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(i => i.PolicyNumber)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(i => i.CoverageDetails)
            .HasMaxLength(1000);

        builder.Property(i => i.Status)
            .IsRequired()
            .HasMaxLength(50);

        builder.HasOne(i => i.TravelBooking)
            .WithMany(tb => tb.Insurances)
            .HasForeignKey(i => i.TravelBookingId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}