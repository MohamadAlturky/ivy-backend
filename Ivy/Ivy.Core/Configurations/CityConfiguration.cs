using Ivy.Core.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Ivy.Core.Configurations;

public class CityConfiguration : IEntityTypeConfiguration<City>
{
    public void Configure(EntityTypeBuilder<City> builder)
    {
        builder.ToTable("Cities");

        // Primary Key
        builder.HasKey(c => c.Id);
        builder.Property(c => c.Id).ValueGeneratedOnAdd();

        // Base Entity Properties
        builder.Property(c => c.CreatedAt).IsRequired();

        builder.Property(c => c.UpdatedAt).IsRequired();

        builder.Property(c => c.DeletedAt).IsRequired(false);

        builder.Property(c => c.IsDeleted).IsRequired().HasDefaultValue(false);

        builder.Property(c => c.IsActive).IsRequired().HasDefaultValue(true);

        // City-specific Properties
        builder.Property(c => c.NameAr).IsRequired().HasMaxLength(100);

        builder.Property(c => c.NameEn).IsRequired().HasMaxLength(100);

        builder.Property(c => c.GovernorateId).IsRequired();

        // Relationship Configuration
        builder
            .HasOne(c => c.Governorate)
            .WithMany(g => g.Cities)
            .HasForeignKey(c => c.GovernorateId)
            .OnDelete(DeleteBehavior.Restrict)
            .HasConstraintName("FK_Cities_Governorates_GovernorateId");

        // Indexes
        builder.HasIndex(c => c.NameAr).HasDatabaseName("IX_Cities_NameAr");

        builder.HasIndex(c => c.NameEn).HasDatabaseName("IX_Cities_NameEn");

        builder.HasIndex(c => c.GovernorateId).HasDatabaseName("IX_Cities_GovernorateId");

        builder.HasIndex(c => c.IsDeleted).HasDatabaseName("IX_Cities_IsDeleted");

        // Composite Index for unique city name per governorate
        builder
            .HasIndex(c => new { c.NameAr, c.GovernorateId })
            .IsUnique()
            .HasDatabaseName("IX_Cities_NameAr_GovernorateId_Unique");

        builder
            .HasIndex(c => new { c.NameEn, c.GovernorateId })
            .IsUnique()
            .HasDatabaseName("IX_Cities_NameEn_GovernorateId_Unique");

        // Global Query Filter for soft delete
        builder.HasQueryFilter(c => !c.IsDeleted);
    }
}
