namespace Ivy.Core.Entities;

public class DoctorMedicalSpeciality : BaseEntity<int>
{
    public int DoctorId { get; set; }
    public Doctor Doctor { get; set; } = null!;
    public int MedicalSpecialityId { get; set; }
    public MedicalSpeciality MedicalSpeciality { get; set; } = null!;
}
