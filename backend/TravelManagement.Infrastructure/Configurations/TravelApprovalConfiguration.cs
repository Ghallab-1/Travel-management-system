using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TravelManagement.Domain.Entities;

namespace TravelManagement.Infrastructure.Persistence.Configurations;

public class TravelApprovalConfiguration : IEntityTypeConfiguration<TravelApproval>
{
    public void Configure(EntityTypeBuilder<TravelApproval> builder)
    {
        builder.ToTable("TravelApprovals");

        builder.HasKey(ta => ta.TravelApprovalId);

        builder.Property(ta => ta.ApprovalLevel)
            .IsRequired();

        builder.Property(ta => ta.Decision)
            .IsRequired()
            .HasMaxLength(50);

        builder.Property(ta => ta.Comments)
            .HasMaxLength(1000);

        builder.Property(ta => ta.ActionDate)
            .HasDefaultValueSql("CURRENT_TIMESTAMP");

        builder.HasOne(ta => ta.TravelRequest)
            .WithMany(tr => tr.TravelApprovals)
            .HasForeignKey(ta => ta.TravelRequestId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(ta => ta.Approver)
            .WithMany(u => u.ApprovalsGiven)
            .HasForeignKey(ta => ta.ApproverId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}