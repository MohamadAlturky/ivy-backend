namespace Ivy.Core.Entities;

public class Patient
{
    public int UserId { get; set; }
    public User User { get; set; } = null!;
    public List<MedicalHistory> MedicalHistories { get; set; } = [];
}
