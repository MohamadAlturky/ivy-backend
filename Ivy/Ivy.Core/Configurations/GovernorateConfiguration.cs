using Ivy.Core.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Ivy.Core.Configurations;

public class GovernorateConfiguration : IEntityTypeConfiguration<Governorate>
{
    public void Configure(EntityTypeBuilder<Governorate> builder)
    {
        builder.ToTable("Governorates");

        // Primary Key
        builder.HasKey(g => g.Id);
        builder.Property(g => g.Id).ValueGeneratedOnAdd();

        // Base Entity Properties
        builder.Property(g => g.CreatedAt).IsRequired();

        builder.Property(g => g.UpdatedAt).IsRequired();

        builder.Property(g => g.DeletedAt).IsRequired(false);

        builder.Property(g => g.IsDeleted).IsRequired().HasDefaultValue(false);

        builder.Property(g => g.IsActive).IsRequired().HasDefaultValue(true);

        // Governorate-specific Properties
        builder.Property(g => g.NameAr).IsRequired().HasMaxLength(100);

        builder.Property(g => g.NameEn).IsRequired().HasMaxLength(100);

        // Indexes
        builder.HasIndex(g => g.NameAr).HasDatabaseName("IX_Governorates_NameAr");

        builder.HasIndex(g => g.NameEn).HasDatabaseName("IX_Governorates_NameEn");

        builder.HasIndex(g => g.IsDeleted).HasDatabaseName("IX_Governorates_IsDeleted");

        // Global Query Filter for soft delete
        builder.HasQueryFilter(g => !g.IsDeleted);
    }
}