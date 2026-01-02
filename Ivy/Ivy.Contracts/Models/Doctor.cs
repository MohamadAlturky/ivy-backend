using System.ComponentModel.DataAnnotations;
using Ivy.Core.Entities;

namespace Ivy.Contracts.Models;

public class DoctorDto
{
    public int Id { get; set; }
    public string DisplayNameAr { get; set; } = string.Empty;
    public string DisplayNameEn { get; set; } = string.Empty;
    public string FirstName { get; set; } = string.Empty;
    public string MiddleName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string PhoneNumber { get; set; } = string.Empty;
    public string ProfileImageUrl { get; set; } = string.Empty;
    public Gender Gender { get; set; }
    public DateTime DateOfBirth { get; set; }
    public List<MedicalSpecialityDto> Specialities { get; set; } = [];
    public required double Rating { get; set; }
}

public class CreateDoctorDto
{
    public string FirstName { get; set; } = string.Empty;
    public string MiddleName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string UserName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
    public string PhoneNumber { get; set; } = string.Empty;
    public Gender Gender { get; set; }
    public DateTime DateOfBirth { get; set; }
    public string ProfileImageUrl { get; set; } = string.Empty;
    public string DisplayNameAr { get; set; } = string.Empty;
    public string DisplayNameEn { get; set; } = string.Empty;

    // List of Medical Speciality IDs to link
    public List<int> MedicalSpecialityIds { get; set; } = new();
}

public class UpdateDoctorDto
{
    public string FirstName { get; set; } = string.Empty;
    public string MiddleName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string PhoneNumber { get; set; } = string.Empty;
    public string ProfileImageUrl { get; set; } = string.Empty;
    public List<int> MedicalSpecialityIds { get; set; } = new();
    public string DisplayNameAr { get; set; } = string.Empty;
    public string DisplayNameEn { get; set; } = string.Empty;
}

public class DoctorLocalizedDto
{
    public int Id { get; set; }
    public string FullName { get; set; } = string.Empty;
    public List<MedicalSpecialityLocalizedDto> Specialities { get; set; } = [];
    public string ProfileImageUrl { get; set; } = string.Empty;
    public required double Rating { get; set; }
}
