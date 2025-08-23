namespace Ivy.Core.Entities;

public class Doctor
{
    public int UserId { get; set; }
    public User User { get; set; } = null!;
    public int SpecialityId { get; set; }
    public MedicalSpeciality Speciality { get; set; } = null!;
}
