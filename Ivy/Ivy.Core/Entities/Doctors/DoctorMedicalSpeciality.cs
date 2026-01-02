namespace Ivy.Core.Entities;

public class DoctorMedicalSpeciality : BaseEntity<int>
{
    public int DoctorDynamicProfileHistoryId { get; set; }
    public DoctorDynamicProfileHistory DoctorDynamicProfileHistory { get; set; } = null!;
    public int MedicalSpecialityId { get; set; }
    public MedicalSpeciality MedicalSpeciality { get; set; } = null!;
}
