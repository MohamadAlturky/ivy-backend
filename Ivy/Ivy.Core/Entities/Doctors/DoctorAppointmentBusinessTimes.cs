namespace Ivy.Core.Entities;

public class DoctorAppointmentBusinessTimes : BaseEntity<int>
{
    public int AppointmentId { get; set; }
    public Appointment Appointment { get; set; } = null!;
    public int DoctorClinicId { get; set; }
    public DoctorClinic DoctorClinic { get; set; } = null!;
    public DateTime StartTime { get; set; }
    public DateTime EndTime { get; set; }
    public AppointmentBusinessTimesStatus Status { get; set; } =
        AppointmentBusinessTimesStatus.Pending;
}

public enum AppointmentBusinessTimesStatus
{
    Pending = 1,
    Declined = 2,
    Accepted = 3,
}
