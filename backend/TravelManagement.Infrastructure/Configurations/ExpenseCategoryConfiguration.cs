using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TravelManagement.Domain.Entities;

namespace TravelManagement.Infrastructure.Persistence.Configurations;

public class ExpenseCategoryConfiguration : IEntityTypeConfiguration<ExpenseCategory>
{
    public void Configure(EntityTypeBuilder<ExpenseCategory> builder)
    {
        builder.ToTable("ExpenseCategories");

        builder.HasKey(ec => ec.ExpenseCategoryId);

        builder.Property(ec => ec.CategoryName)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(ec => ec.Description)
            .HasMaxLength(500);

        builder.Property(ec => ec.IsActive)
            .HasDefaultValue(true);

        builder.HasIndex(ec => ec.CategoryName)
            .IsUnique();
    }
}