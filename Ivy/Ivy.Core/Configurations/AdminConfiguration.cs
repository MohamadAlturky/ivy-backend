using Ivy.Core.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Ivy.Core.Configurations;

public class AdminConfiguration : IEntityTypeConfiguration<Admin>
{
    public void Configure(EntityTypeBuilder<Admin> builder)
    {
        builder.ToTable("Admins");

        // Primary Key
        builder.HasKey(a => a.Id);
        builder.Property(a => a.Id).ValueGeneratedOnAdd();

        // Base Entity Properties
        builder.Property(a => a.CreatedAt).IsRequired();

        builder.Property(a => a.UpdatedAt).IsRequired();

        builder.Property(a => a.DeletedAt).IsRequired(false);

        builder.Property(a => a.IsDeleted).IsRequired().HasDefaultValue(false);

        builder.Property(a => a.IsActive).IsRequired().HasDefaultValue(true);

        // Admin-specific Properties
        builder.Property(a => a.Email)
            .IsRequired()
            .HasMaxLength(255);

        builder.Property(a => a.Password)
            .IsRequired()
            .HasMaxLength(255);

        // Create indexes
        builder.HasIndex(a => a.Email)
            .IsUnique()
            .HasDatabaseName("IX_Admins_Email");

        builder.HasIndex(a => a.IsDeleted)
            .HasDatabaseName("IX_Admins_IsDeleted");

        // Global Query Filter for soft delete
        builder.HasQueryFilter(a => !a.IsDeleted);
    }
}
