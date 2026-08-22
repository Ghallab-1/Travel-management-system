using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TravelManagement.Domain.Entities;

namespace TravelManagement.Infrastructure.Persistence.Configurations;

public class TravelRequestConfiguration : IEntityTypeConfiguration<TravelRequest>
{
    public void Configure(EntityTypeBuilder<TravelRequest> builder)
    {
        builder.ToTable("TravelRequests");

        builder.HasKey(tr => tr.TravelRequestId);

        builder.Property(tr => tr.Purpose)
            .IsRequired()
            .HasMaxLength(500);

        builder.Property(tr => tr.Project)
            .HasMaxLength(200);

        builder.Property(tr => tr.TravelType)
            .IsRequired()
            .HasMaxLength(50);

        builder.Property(tr => tr.EstimatedBudget)
            .HasPrecision(18, 2);

        builder.Property(tr => tr.RequiredDocumentNotes)
            .HasMaxLength(2000);

        builder.Property(tr => tr.RequiredDocumentFileName)
            .HasMaxLength(255);

        builder.Property(tr => tr.RequiredDocumentFileContentType)
            .HasMaxLength(100);

        builder.Property(tr => tr.RequiredDocumentFileBase64)
            .HasColumnType("text");

        builder.Property(tr => tr.CoordinatorNotes)
            .HasMaxLength(2000);

        builder.Property(tr => tr.PerDiemAmount)
            .HasPrecision(18, 2);

        builder.Property(tr => tr.PerDiemStatus)
            .IsRequired()
            .HasMaxLength(50)
            .HasDefaultValue("Not Submitted");

        builder.Property(tr => tr.PerDiemComments)
            .HasMaxLength(1000);

        builder.Property(tr => tr.Status)
            .IsRequired()
            .HasMaxLength(50);

        builder.Property(tr => tr.CurrentApprovalLevel)
            .HasDefaultValue(2);

        builder.Property(tr => tr.CreatedDate)
            .HasDefaultValueSql("CURRENT_TIMESTAMP");

        builder.HasOne(tr => tr.User)
            .WithMany(u => u.TravelRequests)
            .HasForeignKey(tr => tr.UserId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(tr => tr.Department)
            .WithMany(d => d.TravelRequests)
            .HasForeignKey(tr => tr.DepartmentId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(tr => tr.TravelPolicy)
            .WithMany(tp => tp.TravelRequests)
            .HasForeignKey(tr => tr.TravelPolicyId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(tr => tr.DestinationCity)
            .WithMany(c => c.TravelRequests)
            .HasForeignKey(tr => tr.DestinationCityId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(tr => tr.EstimatedBudgetSetBy)
            .WithMany()
            .HasForeignKey(tr => tr.EstimatedBudgetSetById)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(tr => tr.PerDiemApprovedBy)
            .WithMany()
            .HasForeignKey(tr => tr.PerDiemApprovedById)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
