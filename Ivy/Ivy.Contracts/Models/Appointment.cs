using System.ComponentModel.DataAnnotations;
using Ivy.Core.Entities;

namespace Ivy.Contracts.Models;

// Appointment DTOs
public class AppointmentDto
{
    public int Id { get; set; }
    public int DoctorId { get; set; }
    public int PatientId { get; set; }
    public int ClinicId { get; set; }
    public DateTime AppointmentDateStart { get; set; }
    public DateTime AppointmentDateEnd { get; set; }
    public AppointmentStatus Status { get; set; }
    public string Notes { get; set; } = string.Empty;
}

public class CreateAppointmentDto
{
    [Required]
    public int DoctorId { get; set; }

    [Required]
    public int PatientId { get; set; }

    [Required]
    public int ClinicId { get; set; }

    [Required]
    public DateTime AppointmentDateStart { get; set; }

    public string Notes { get; set; } = string.Empty;
}

public class RescheduleAppointmentDto
{
    [Required]
    public DateTime NewAppointmentDateStart { get; set; }
}

public class AppointmentResponseDto
{
    public int Id { get; set; }
    public int DoctorId { get; set; }
    public string DoctorName { get; set; } = string.Empty;
    public int PatientId { get; set; }
    public string PatientName { get; set; } = string.Empty;
    public int ClinicId { get; set; }
    public string ClinicName { get; set; } = string.Empty;
    public DateTime AppointmentDateStart { get; set; }
    public DateTime AppointmentDateEnd { get; set; }
    public AppointmentStatus Status { get; set; }
    public string Notes { get; set; } = string.Empty;
}

public class AppointmentFilterDto
{
    /// <summary>
    /// Page number (default: 1)
    /// </summary>
    public int Page { get; set; } = 1;

    /// <summary>
    /// Page size (default: 10, max: 100)
    /// </summary>
    public int PageSize { get; set; } = 10;

    /// <summary>
    /// Filter by doctor ID
    /// </summary>
    public int? DoctorId { get; set; }

    /// <summary>
    /// Filter by patient ID
    /// </summary>
    public int? PatientId { get; set; }

    /// <summary>
    /// Filter by clinic ID
    /// </summary>
    public int? ClinicId { get; set; }

    /// <summary>
    /// Filter by appointment status
    /// </summary>
    public AppointmentStatus? Status { get; set; }

    /// <summary>
    /// Filter by appointment start date (from)
    /// </summary>
    public DateTime? StartDateFrom { get; set; }

    /// <summary>
    /// Filter by appointment start date (to)
    /// </summary>
    public DateTime? StartDateTo { get; set; }

    /// <summary>
    /// Search term for patient name or doctor name
    /// </summary>
    public string? SearchTerm { get; set; }
}