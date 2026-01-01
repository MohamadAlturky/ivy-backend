namespace Ivy.Core.Entities;

public class Appointment : BaseEntity<int>
{
    /// <summary>
    /// navigation properties
    /// </summary>
    public int DoctorId { get; set; }
    public Doctor Doctor { get; set; } = null!;
    public int PatientId { get; set; }
    public Patient Patient { get; set; } = null!;
    public int ClinicId { get; set; }
    public Clinic Clinic { get; set; } = null!;

    /// <summary>
    /// properties
    /// </summary>
    public DateTime AppointmentDateStart { get; set; }
    public DateTime AppointmentDateEnd { get; set; }
    public AppointmentStatus Status { get; set; } = AppointmentStatus.Pending;
    public string Notes { get; set; } = string.Empty;
}

public enum AppointmentStatus
{
    Pending = 1,
    Confirmed = 2,
    InProgress = 3,
    Cancelled = 4,
    Completed = 5,
}

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
