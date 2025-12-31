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

public class DoctorClinicConfiguration : IEntityTypeConfiguration<DoctorClinic>
{
    public void Configure(EntityTypeBuilder<DoctorClinic> builder)
    {
        builder.ToTable("DoctorClinics");
        builder.HasKey(dc => dc.Id);
        builder.Property(dc => dc.Id).ValueGeneratedOnAdd();
        builder.Property(dc => dc.DoctorId).IsRequired();
        builder.Property(dc => dc.ClinicId).IsRequired();
        builder
            .HasOne(dc => dc.Doctor)
            .WithMany()
            .HasForeignKey(dc => dc.DoctorId)
            .OnDelete(DeleteBehavior.Cascade);
        builder
            .HasOne(dc => dc.Clinic)
            .WithMany()
            .HasForeignKey(dc => dc.ClinicId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}

public class DoctorWorkingTimesConfiguration : IEntityTypeConfiguration<DoctorWorkingTimes>
{
    public void Configure(EntityTypeBuilder<DoctorWorkingTimes> builder)
    {
        builder.ToTable("DoctorWorkingTimes");
        builder.HasKey(dw => dw.Id);
        builder.Property(dw => dw.Id).ValueGeneratedOnAdd();
        builder.Property(dw => dw.DoctorClinicId).IsRequired();
        builder.Property(dw => dw.StartTime).IsRequired();
        builder.Property(dw => dw.EndTime).IsRequired();
        builder
            .HasOne(dw => dw.DoctorClinic)
            .WithMany(dc => dc.DoctorWorkingTimes)
            .HasForeignKey(dw => dw.DoctorClinicId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}

public class DoctorBusinessTimesConfiguration : IEntityTypeConfiguration<DoctorBusinessTimes>
{
    public void Configure(EntityTypeBuilder<DoctorBusinessTimes> builder)
    {
        builder.ToTable("DoctorBusinessTimes");
        builder.HasKey(dbh => dbh.Id);
        builder.Property(dbh => dbh.Id).ValueGeneratedOnAdd();
        builder.Property(dbh => dbh.DoctorClinicId).IsRequired();
        builder.Property(dbh => dbh.StartTime).IsRequired();
        builder.Property(dbh => dbh.EndTime).IsRequired();
        builder
            .HasOne(dbh => dbh.DoctorClinic)
            .WithMany(dc => dc.DoctorBusinessTimes)
            .HasForeignKey(dbh => dbh.DoctorClinicId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
