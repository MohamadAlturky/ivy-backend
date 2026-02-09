using Ivy.Core.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Ivy.Core.Configurations;

public class MedicalHistoryConfiguration : IEntityTypeConfiguration<MedicalHistory>
{
    public void Configure(EntityTypeBuilder<MedicalHistory> builder)
    {
        builder.ToTable("MedicalHistories");

        // Primary Key
        builder.HasKey(mh => mh.Id);
        builder.Property(mh => mh.Id).ValueGeneratedOnAdd();

        // Base Entity Properties
        builder.Property(mh => mh.CreatedAt).IsRequired();
        builder.Property(mh => mh.DeletedAt).IsRequired(false);
        builder.Property(mh => mh.IsDeleted).IsRequired().HasDefaultValue(false);
        builder.Property(mh => mh.IsActive).IsRequired().HasDefaultValue(true);

        // MedicalHistory-specific Properties
        builder.Property(mh => mh.PatientId).IsRequired();
        builder.Property(mh => mh.CreatedByUserId).IsRequired();

        // Relationship Configuration
        builder
            .HasOne(mh => mh.Patient)
            .WithMany(p => p.MedicalHistories)
            .HasForeignKey(mh => mh.PatientId)
            .OnDelete(DeleteBehavior.Restrict)
            .HasConstraintName("FK_MedicalHistories_Patients_PatientId");

        builder
            .HasOne(mh => mh.CreatedByUser)
            .WithMany()
            .HasForeignKey(mh => mh.CreatedByUserId)
            .OnDelete(DeleteBehavior.Restrict)
            .HasConstraintName("FK_MedicalHistories_Users_CreatedByUserId");

        builder
            .HasMany(mh => mh.MedicalHistoryItems)
            .WithOne(mhi => mhi.MedicalHistory)
            .HasForeignKey(mhi => mhi.MedicalHistoryId)
            .OnDelete(DeleteBehavior.Cascade);

        // Indexes
        builder.HasIndex(mh => mh.PatientId).HasDatabaseName("IX_MedicalHistories_PatientId");
        builder
            .HasIndex(mh => mh.CreatedByUserId)
            .HasDatabaseName("IX_MedicalHistories_CreatedByUserId");
        builder.HasIndex(mh => mh.Type).HasDatabaseName("IX_MedicalHistories_Type");
        builder.HasIndex(mh => mh.IsDeleted).HasDatabaseName("IX_MedicalHistories_IsDeleted");
        builder.HasIndex(mh => mh.CreatedAt).HasDatabaseName("IX_MedicalHistories_CreatedAt");

        // Global Query Filter for soft delete
        builder.HasQueryFilter(mh => !mh.IsDeleted);
    }
}

public class MedicalHistoryItemConfiguration : IEntityTypeConfiguration<MedicalHistoryItem>
{
    public void Configure(EntityTypeBuilder<MedicalHistoryItem> builder)
    {
        builder.ToTable("MedicalHistoryItems");

        // Primary Key
        builder.HasKey(mhi => mhi.Id);
        builder.Property(mhi => mhi.Id).ValueGeneratedOnAdd();

        // Base Entity Properties
        builder.Property(mhi => mhi.CreatedAt).IsRequired();
        builder.Property(mhi => mhi.DeletedAt).IsRequired(false);
        builder.Property(mhi => mhi.IsDeleted).IsRequired().HasDefaultValue(false);
        builder.Property(mhi => mhi.IsActive).IsRequired().HasDefaultValue(true);

        // MedicalHistoryItem-specific Properties
        builder.Property(mhi => mhi.MedicalHistoryId).IsRequired();
        builder.Property(mhi => mhi.MediaUrl).IsRequired().HasMaxLength(500);
        builder.Property(mhi => mhi.MediaType).IsRequired();

        // Indexes
        builder
            .HasIndex(mhi => mhi.MedicalHistoryId)
            .HasDatabaseName("IX_MedicalHistoryItems_MedicalHistoryId");
        builder.HasIndex(mhi => mhi.MediaType).HasDatabaseName("IX_MedicalHistoryItems_MediaType");
        builder.HasIndex(mhi => mhi.IsDeleted).HasDatabaseName("IX_MedicalHistoryItems_IsDeleted");

        // Global Query Filter for soft delete
        builder.HasQueryFilter(mhi => !mhi.IsDeleted);
    }
}
