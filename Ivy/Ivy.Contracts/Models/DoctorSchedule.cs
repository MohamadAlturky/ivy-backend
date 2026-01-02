namespace Ivy.Contracts.Models;

public class TimeRangeDto
{
    public DateTime StartTime { get; set; }
    public DateTime EndTime { get; set; }
}

public class DoctorWorkingTimesDto
{
    public int Id { get; set; }
    public int DoctorId { get; set; }
    public int ClinicId { get; set; }
    public string DoctorName { get; set; } = string.Empty;
    public string ClinicName { get; set; } = string.Empty;
    public DateTime StartTime { get; set; }
    public DateTime EndTime { get; set; }
}
