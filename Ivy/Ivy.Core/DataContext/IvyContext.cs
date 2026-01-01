using System.Reflection;
using Ivy.Core.Entities;
using Microsoft.EntityFrameworkCore;

namespace Ivy.Core.DataContext;

public class IvyContext : DbContext
{
    public IvyContext(DbContextOptions<IvyContext> options)
        : base(options) { }

    public DbSet<City> Cities { get; set; } = null!;
    public DbSet<Governorate> Governorates { get; set; } = null!;
    public DbSet<Location> Locations { get; set; } = null!;
    public DbSet<Clinic> Clinics { get; set; } = null!;
    public DbSet<ClinicMedia> ClinicImages { get; set; } = null!;
    public DbSet<MedicalSpeciality> MedicalSpecialities { get; set; } = null!;
    public DbSet<User> Users { get; set; } = null!;
    public DbSet<Patient> Patients { get; set; } = null!;
    public DbSet<Admin> Admins { get; set; } = null!;
    public DbSet<Doctor> Doctors { get; set; } = null!;
    public DbSet<DoctorClinic> DoctorClinics { get; set; } = null!;

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(Assembly.GetExecutingAssembly());
    }
}
