using Ivy.Core.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Ivy.Core.Configurations;

public class ClinicConfiguration : IEntityTypeConfiguration<Clinic>
{
    public void Configure(EntityTypeBuilder<Clinic> builder)
    {
        builder.ToTable("Clinics");

        // Primary Key
        builder.HasKey(c => c.Id);
        builder.Property(c => c.Id).ValueGeneratedOnAdd();

        // Base Entity Properties
        builder.Property(c => c.CreatedAt).IsRequired();

        builder.Property(c => c.UpdatedAt).IsRequired();

        builder.Property(c => c.DeletedAt).IsRequired(false);

        builder.Property(c => c.IsDeleted).IsRequired().HasDefaultValue(false);

        builder.Property(c => c.IsActive).IsRequired().HasDefaultValue(true);

        // Clinic-specific Properties
        builder.Property(c => c.NameAr).IsRequired().HasMaxLength(200);

        builder.Property(c => c.NameEn).IsRequired().HasMaxLength(200);

        builder.Property(c => c.DescriptionAr).IsRequired().HasMaxLength(1000);

        builder.Property(c => c.DescriptionEn).IsRequired().HasMaxLength(1000);

        builder.Property(c => c.ContactPhoneNumber).IsRequired().HasMaxLength(20);

        builder.Property(c => c.ContactEmail).IsRequired().HasMaxLength(255);

        builder.Property(c => c.LocationId).IsRequired();

        // Relationship Configuration
        builder
            .HasOne(c => c.Location)
            .WithMany()
            .HasForeignKey(c => c.LocationId)
            .OnDelete(DeleteBehavior.Restrict)
            .HasConstraintName("FK_Clinics_Locations_LocationId");

        builder
            .HasMany(c => c.ClinicMedias)
            .WithOne()
            .HasForeignKey(ci => ci.ClinicId)
            .OnDelete(DeleteBehavior.Cascade)
            .HasConstraintName("FK_ClinicImages_Clinics_ClinicId");

        // Indexes
        builder.HasIndex(c => c.NameAr).HasDatabaseName("IX_Clinics_NameAr");

        builder.HasIndex(c => c.NameEn).HasDatabaseName("IX_Clinics_NameEn");

        builder.HasIndex(c => c.LocationId).HasDatabaseName("IX_Clinics_LocationId");

        builder.HasIndex(c => c.IsDeleted).HasDatabaseName("IX_Clinics_IsDeleted");

        builder
            .HasIndex(c => c.ContactPhoneNumber)
            .HasDatabaseName("IX_Clinics_ContactPhoneNumber");

        // Composite Index for unique clinic name per location
        builder
            .HasIndex(c => new { c.NameAr, c.LocationId })
            .IsUnique()
            .HasDatabaseName("IX_Clinics_NameAr_LocationId_Unique");

        builder
            .HasIndex(c => new { c.NameEn, c.LocationId })
            .IsUnique()
            .HasDatabaseName("IX_Clinics_NameEn_LocationId_Unique");

        // Unique constraint for contact email
        builder
            .HasIndex(c => c.ContactEmail)
            .IsUnique()
            .HasDatabaseName("IX_Clinics_ContactEmail_Unique");

        // Global Query Filter for soft delete
        builder.HasQueryFilter(c => !c.IsDeleted);
    }
}

public class ClinicImagesConfiguration : IEntityTypeConfiguration<ClinicMedia>
{
    public void Configure(EntityTypeBuilder<ClinicMedia> builder)
    {
        builder.ToTable("ClinicMedias");

        // Primary Key
        builder.HasKey(ci => ci.Id);
        builder.Property(ci => ci.Id).ValueGeneratedOnAdd();

        // Base Entity Properties
        builder.Property(ci => ci.CreatedAt).IsRequired();

        builder.Property(ci => ci.UpdatedAt).IsRequired();

        builder.Property(ci => ci.DeletedAt).IsRequired(false);

        builder.Property(ci => ci.IsDeleted).IsRequired().HasDefaultValue(false);

        builder.Property(ci => ci.IsActive).IsRequired().HasDefaultValue(true);

        // ClinicImages-specific Properties
        builder.Property(ci => ci.ClinicId).IsRequired();

        builder.Property(ci => ci.MediaUrl).IsRequired().HasMaxLength(500);

        builder.Property(ci => ci.MediaType).IsRequired();

        // Indexes
        builder.HasIndex(ci => ci.ClinicId).HasDatabaseName("IX_ClinicMedias_ClinicId");

        builder.HasIndex(ci => ci.IsDeleted).HasDatabaseName("IX_ClinicMedias_IsDeleted");

        // Global Query Filter for soft delete
        builder.HasQueryFilter(ci => !ci.IsDeleted);
    }
}