using Ivy.Core.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Ivy.Core.Configurations;

public class DoctorConfiguration : IEntityTypeConfiguration<Doctor>
{
    public void Configure(EntityTypeBuilder<Doctor> builder)
    {
        builder.HasKey(d => d.UserId);

        builder.Property(d => d.ProfileImageUrl).HasMaxLength(500).IsRequired();

        builder.Property(d => d.IsProfileCompleted).IsRequired().HasDefaultValue(false);

        // Configure relationship with User
        builder
            .HasOne(d => d.User)
            .WithOne()
            .HasForeignKey<Doctor>(d => d.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        builder
            .HasMany(d => d.DoctorMedicalSpecialities)
            .WithOne(dms => dms.Doctor)
            .HasForeignKey(dms => dms.DoctorId)
            .OnDelete(DeleteBehavior.Cascade);

        // Configure table name
        builder.ToTable("Doctors");
    }
}
