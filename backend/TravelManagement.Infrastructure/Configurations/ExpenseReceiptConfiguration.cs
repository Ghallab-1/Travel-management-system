using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TravelManagement.Domain.Entities;

namespace TravelManagement.Infrastructure.Persistence.Configurations;

public class ExpenseReceiptConfiguration : IEntityTypeConfiguration<ExpenseReceipt>
{
    public void Configure(EntityTypeBuilder<ExpenseReceipt> builder)
    {
        builder.ToTable("ExpenseReceipts");

        builder.HasKey(er => er.ExpenseReceiptId);

        builder.Property(er => er.FileName)
            .IsRequired()
            .HasMaxLength(255);

        builder.Property(er => er.FilePath)
            .IsRequired()
            .HasMaxLength(500);

        builder.Property(er => er.FileType)
            .IsRequired()
            .HasMaxLength(50);

        builder.Property(er => er.UploadDate)
            .HasDefaultValueSql("CURRENT_TIMESTAMP");

        builder.HasOne(er => er.Expense)
            .WithMany(e => e.ExpenseReceipts)
            .HasForeignKey(er => er.ExpenseId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}