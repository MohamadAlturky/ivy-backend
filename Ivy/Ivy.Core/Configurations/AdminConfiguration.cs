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
        builder.Property(a => a.Email).IsRequired().HasMaxLength(255);

        builder.Property(a => a.Password).IsRequired().HasMaxLength(255);

        // Create indexes
        builder.HasIndex(a => a.Email).IsUnique().HasDatabaseName("IX_Admins_Email");

        builder.HasIndex(a => a.IsDeleted).HasDatabaseName("IX_Admins_IsDeleted");

        builder
            .HasMany(a => a.AdminRolesLinks)
            .WithOne(arl => arl.Admin)
            .HasForeignKey(arl => arl.AdminId)
            .OnDelete(DeleteBehavior.Cascade);

        // Global Query Filter for soft delete
        builder.HasQueryFilter(a => !a.IsDeleted);
    }
}

public class AdminRolesLinkConfiguration : IEntityTypeConfiguration<AdminRolesLink>
{
    public void Configure(EntityTypeBuilder<AdminRolesLink> builder)
    {
        builder.ToTable("AdminRolesLinks");
        builder.HasKey(arl => arl.Id);
        builder.Property(arl => arl.Id).ValueGeneratedOnAdd();
        builder.Property(arl => arl.AdminId).IsRequired();
        builder.Property(arl => arl.RoleId).IsRequired();
        builder
            .HasOne(arl => arl.Admin)
            .WithMany(a => a.AdminRolesLinks)
            .HasForeignKey(arl => arl.AdminId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}

public class AdminRoleConfiguration : IEntityTypeConfiguration<AdminRole>
{
    public void Configure(EntityTypeBuilder<AdminRole> builder)
    {
        builder.ToTable("AdminRoles");
        builder.HasKey(ar => ar.Id);
        builder.Property(ar => ar.Id).ValueGeneratedOnAdd();
        builder.Property(ar => ar.NameAr).IsRequired().HasMaxLength(100);
        builder.Property(ar => ar.NameEn).IsRequired().HasMaxLength(100);
        builder.Property(ar => ar.DescriptionAr).IsRequired().HasMaxLength(500);
        builder.Property(ar => ar.DescriptionEn).IsRequired().HasMaxLength(500);
        builder
            .HasMany(ar => ar.RolePermissionsLinks)
            .WithOne(rp => rp.Role)
            .HasForeignKey(rp => rp.RoleId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}

public class RolePermissionsLinkConfiguration : IEntityTypeConfiguration<RolePermissionsLink>
{
    public void Configure(EntityTypeBuilder<RolePermissionsLink> builder)
    {
        builder.ToTable("RolePermissionsLinks");
        builder.HasKey(rp => rp.Id);
        builder.Property(rp => rp.Id).ValueGeneratedOnAdd();
        builder.Property(rp => rp.RoleId).IsRequired();
        builder.Property(rp => rp.PermissionId).IsRequired();
        builder
            .HasOne(rp => rp.Role)
            .WithMany(r => r.RolePermissionsLinks)
            .HasForeignKey(rp => rp.RoleId)
            .OnDelete(DeleteBehavior.Cascade);
        builder
            .HasOne(rp => rp.Permission)
            .WithMany()
            .HasForeignKey(rp => rp.PermissionId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}

public class AdminPermissionConfiguration : IEntityTypeConfiguration<AdminPermission>
{
    public void Configure(EntityTypeBuilder<AdminPermission> builder)
    {
        builder.ToTable("AdminPermissions");
        builder.HasKey(ap => ap.Id);
        builder.Property(ap => ap.Id).ValueGeneratedOnAdd();
        builder.Property(ap => ap.NameAr).IsRequired().HasMaxLength(100);
        builder.Property(ap => ap.NameEn).IsRequired().HasMaxLength(100);
        builder.Property(ap => ap.DescriptionAr).IsRequired().HasMaxLength(500);
        builder.Property(ap => ap.DescriptionEn).IsRequired().HasMaxLength(500);
    }
}