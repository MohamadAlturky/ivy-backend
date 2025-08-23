using Ivy.Core.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Ivy.Core.Configurations;

public class MedicalSpecialityConfiguration : IEntityTypeConfiguration<MedicalSpeciality>
{
    public void Configure(EntityTypeBuilder<MedicalSpeciality> builder)
    {
        builder.ToTable("MedicalSpecialities");

        // Primary Key
        builder.HasKey(ms => ms.Id);
        builder.Property(ms => ms.Id).ValueGeneratedOnAdd();

        // Base Entity Properties
        builder.Property(ms => ms.CreatedAt).IsRequired();

        builder.Property(ms => ms.UpdatedAt).IsRequired();

        builder.Property(ms => ms.DeletedAt).IsRequired(false);

        builder.Property(ms => ms.IsDeleted).IsRequired().HasDefaultValue(false);

        builder.Property(ms => ms.IsActive).IsRequired().HasDefaultValue(true);

        // MedicalSpeciality-specific Properties
        builder.Property(ms => ms.NameAr).IsRequired().HasMaxLength(100);

        builder.Property(ms => ms.NameEn).IsRequired().HasMaxLength(100);

        builder.Property(ms => ms.DescriptionAr).HasMaxLength(500);

        builder.Property(ms => ms.DescriptionEn).HasMaxLength(500);

        // Indexes
        builder.HasIndex(ms => ms.NameAr).HasDatabaseName("IX_MedicalSpecialities_NameAr");

        builder.HasIndex(ms => ms.NameEn).HasDatabaseName("IX_MedicalSpecialities_NameEn");

        builder.HasIndex(ms => ms.IsDeleted).HasDatabaseName("IX_MedicalSpecialities_IsDeleted");

        // Global Query Filter for soft delete
        builder.HasQueryFilter(ms => !ms.IsDeleted);
    }
}
