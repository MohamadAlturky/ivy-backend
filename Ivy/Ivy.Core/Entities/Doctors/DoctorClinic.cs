namespace Ivy.Core.Entities;

public class DoctorClinic : BaseEntity<int>
{
    public int DoctorId { get; set; }
    public Doctor Doctor { get; set; } = null!;
    public int ClinicId { get; set; }
    public Clinic Clinic { get; set; } = null!;
    public ICollection<DoctorWorkingTimes> DoctorWorkingTimes { get; set; } = [];

    public ICollection<DoctorAppointmentBusinessTimes> DoctorAppointmentBusinessTimes { get; set; } =
        [];

    public ICollection<DoctorBusinessTimes> DoctorBusinessTimes { get; set; } = [];
}