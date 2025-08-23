using Ivy.Core.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Ivy.Core.Configurations;

public class UserConfiguration : IEntityTypeConfiguration<User>
{
    public void Configure(EntityTypeBuilder<User> builder)
    {
        builder.HasKey(u => u.Id);

        builder.Property(u => u.FirstName)
            .IsRequired()
            .HasMaxLength(50);

        builder.Property(u => u.MiddleName)
            .HasMaxLength(50);

        builder.Property(u => u.LastName)
            .IsRequired()
            .HasMaxLength(50);

        builder.Property(u => u.UserName)
            .IsRequired()
            .HasMaxLength(50);

        builder.Property(u => u.Password)
            .IsRequired()
            .HasMaxLength(255);

        builder.Property(u => u.PhoneNumber)
            .IsRequired()
            .HasMaxLength(20);

        builder.Property(u => u.Gender)
            .IsRequired();

        builder.Property(u => u.IsPhoneVerified)
            .HasDefaultValue(false);

        builder.Property(u => u.DateOfBirth)
            .IsRequired();

        // Create indexes
        builder.HasIndex(u => u.UserName)
            .IsUnique();

        builder.HasIndex(u => u.PhoneNumber)
            .IsUnique();

        // Add filter for soft deletes
        builder.HasQueryFilter(u => !u.IsDeleted);
    }
}
