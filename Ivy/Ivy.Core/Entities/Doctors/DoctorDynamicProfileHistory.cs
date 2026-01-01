namespace Ivy.Core.Entities;

public class DoctorDynamicProfileHistory : BaseEntity<int>
{
    public int DoctorId { get; set; }
    public Doctor Doctor { get; set; } = null!;
    public bool IsLatest { get; set; } = true;
    public string JsonData { get; set; } = "{}";
}
