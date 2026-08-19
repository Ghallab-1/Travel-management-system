using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TravelManagement.Domain.Entities;

namespace TravelManagement.Infrastructure.Persistence.Configurations;

public class TravelPolicyConfiguration : IEntityTypeConfiguration<TravelPolicy>
{
    public void Configure(EntityTypeBuilder<TravelPolicy> builder)
    {
        builder.ToTable("TravelPolicies");

        builder.HasKey(tp => tp.TravelPolicyId);

        builder.Property(tp => tp.PolicyName)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(tp => tp.Description)
            .HasMaxLength(1000);

        builder.Property(tp => tp.TravelType)
            .IsRequired()
            .HasMaxLength(50);

        builder.Property(tp => tp.MaximumBudget)
            .HasPrecision(18, 2);

        builder.Property(tp => tp.MaximumHotelAllowance)
            .HasPrecision(18, 2);

        builder.Property(tp => tp.MaximumMealAllowance)
            .HasPrecision(18, 2);

        builder.Property(tp => tp.IsActive)
            .HasDefaultValue(true);

        builder.HasIndex(tp => tp.PolicyName)
            .IsUnique();
    }
}