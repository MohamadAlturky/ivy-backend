namespace Ivy.Core.Entities;

public class DoctorWorkingTimes : BaseEntity<int>
{
    public int DoctorClinicId { get; set; }
    public DoctorClinic DoctorClinic { get; set; } = null!;
    public DateTime StartTime { get; set; }
    public DateTime EndTime { get; set; }
}