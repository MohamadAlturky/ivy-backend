namespace Ivy.Core.Entities;

public class MedicalSpeciality : BaseEntity<int>
{
    public string NameAr { get; set; } = string.Empty;
    public string NameEn { get; set; } = string.Empty;
    public string DescriptionAr { get; set; } = string.Empty;
    public string DescriptionEn { get; set; } = string.Empty;
    public ICollection<DoctorMedicalSpeciality> DoctorMedicalSpecialities { get; set; } =
        new List<DoctorMedicalSpeciality>();
}
