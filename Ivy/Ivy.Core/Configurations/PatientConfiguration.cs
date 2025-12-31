using Ivy.Core.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Ivy.Core.Configurations;

public class PatientConfiguration : IEntityTypeConfiguration<Patient>
{
    public void Configure(EntityTypeBuilder<Patient> builder)
    {
        builder.HasKey(p => p.UserId);

        builder
            .HasOne(p => p.User)
            .WithOne()
            .HasForeignKey<Patient>(p => p.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        // Add filter for soft deletes (through User relationship)
        builder.HasQueryFilter(p => !p.User.IsDeleted);
    }
}

public class AppointmentConfiguration : IEntityTypeConfiguration<Appointment>
{
    public void Configure(EntityTypeBuilder<Appointment> builder)
    {
        builder.ToTable("Appointments");
        builder.HasKey(a => a.Id);
        builder.Property(a => a.Id).ValueGeneratedOnAdd();
        builder.Property(a => a.DoctorId).IsRequired();
        builder.Property(a => a.PatientId).IsRequired();
        builder.Property(a => a.ClinicId).IsRequired();
        builder.Property(a => a.AppointmentDateStart).IsRequired();
        builder.Property(a => a.AppointmentDateEnd).IsRequired();
        builder.Property(a => a.Status).IsRequired();
        builder.Property(a => a.Notes).HasMaxLength(500);
        builder
            .HasOne(a => a.Doctor)
            .WithMany()
            .HasForeignKey(a => a.DoctorId)
            .OnDelete(DeleteBehavior.Restrict);
        builder
            .HasOne(a => a.Patient)
            .WithMany()
            .HasForeignKey(a => a.PatientId)
            .OnDelete(DeleteBehavior.Restrict);
        builder
            .HasOne(a => a.Clinic)
            .WithMany()
            .HasForeignKey(a => a.ClinicId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
