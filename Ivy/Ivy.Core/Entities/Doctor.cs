namespace Ivy.Core.Entities;

public class Doctor
{
    public int UserId { get; set; }
    public User User { get; set; } = null!;
    public string ProfileImageUrl { get; set; } = null!;
    public bool IsProfileCompleted { get; set; }

    public ICollection<DoctorMedicalSpeciality> DoctorMedicalSpecialities { get; set; } =
        new List<DoctorMedicalSpeciality>();
}

public class DoctorClinic : BaseEntity<int>
{
    public int DoctorId { get; set; }
    public Doctor Doctor { get; set; } = null!;
    public int ClinicId { get; set; }
    public Clinic Clinic { get; set; } = null!;
    public ICollection<DoctorWorkingTimes> DoctorWorkingTimes { get; set; } = [];
    public ICollection<DoctorBusinessTimes> DoctorBusinessTimes { get; set; } = [];
}

public class DoctorWorkingTimes : BaseEntity<int>
{
    public int DoctorClinicId { get; set; }
    public DoctorClinic DoctorClinic { get; set; } = null!;
    public DateTime StartTime { get; set; }
    public DateTime EndTime { get; set; }
}

public class DoctorBusinessTimes : BaseEntity<int>
{
    public int DoctorClinicId { get; set; }
    public DoctorClinic DoctorClinic { get; set; } = null!;
    public DateTime StartTime { get; set; }
    public DateTime EndTime { get; set; }
}
