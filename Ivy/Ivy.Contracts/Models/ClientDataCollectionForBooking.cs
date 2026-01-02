using Ivy.Core.Entities;

namespace Ivy.Contracts.Models;

// Clinic DTOs
public class ClinicForBookingLocalizedDto
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string ContactPhoneNumber { get; set; } = string.Empty;
    public string ContactEmail { get; set; } = string.Empty;
    public LocationLocalizedDto Location { get; set; } = null!;
}

// Doctor DTOs

public class DoctorForBookingLocalizedDto
{
    public int Id { get; set; }
    public string DisplayName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string PhoneNumber { get; set; } = string.Empty;
    public string ProfileImageUrl { get; set; } = string.Empty;
    public Gender Gender { get; set; }
    public DateTime DateOfBirth { get; set; }
    public List<MedicalSpecialityLocalizedDto> Specialities { get; set; } = [];
    public double Rating { get; set; }
}
