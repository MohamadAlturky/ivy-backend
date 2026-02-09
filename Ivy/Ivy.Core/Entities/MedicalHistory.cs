namespace Ivy.Core.Entities;

public class MedicalHistory : BaseEntity<int>
{
    public int PatientId { get; set; }
    public Patient Patient { get; set; } = null!;
    public int CreatedByUserId { get; set; }
    public User CreatedByUser { get; set; } = null!;
    public MedicalHistoryType Type { get; set; } = MedicalHistoryType.VisitNotes;
    public ICollection<MedicalHistoryItem> MedicalHistoryItems { get; set; } = [];
}

public class MedicalHistoryItem : BaseEntity<int>
{
    public int MedicalHistoryId { get; set; }
    public MedicalHistory MedicalHistory { get; set; } = null!;
    public string MediaUrl { get; set; } = string.Empty;
    public MediaType MediaType { get; set; } = MediaType.Image;
}

public enum MedicalHistoryType
{
    VisitNotes = 1,
    Diagnoses = 2,
    Prescriptions = 3,
}
