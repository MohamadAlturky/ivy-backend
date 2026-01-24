using Ivy.Contracts.Models;
using Ivy.Core.DataContext;
using Ivy.Core.Entities;
using IvyBackend;
using Microsoft.EntityFrameworkCore;

namespace Ivy.Contracts.Services;

public interface ICustomerAppointmentService
{
    /// <summary>
    /// Get paginated list of appointments for a specific patient
    /// </summary>
    Task<Result<PaginatedResult<AppointmentResponseDto>>> GetPatientAppointmentsAsync(
        int patientId,
        CustomerAppointmentFilterDto filter
    );

    /// <summary>
    /// Get a specific appointment by ID (only if it belongs to the patient)
    /// </summary>
    Task<Result<AppointmentResponseDto>> GetPatientAppointmentByIdAsync(
        int patientId,
        int appointmentId
    );

    /// <summary>
    /// Book a new appointment for a patient
    /// </summary>
    Task<Result<AppointmentResponseDto>> BookAppointmentAsync(
        int patientId,
        CustomerCreateAppointmentDto dto
    );

    /// <summary>
    /// Cancel an appointment (if not within X minutes of start time)
    /// </summary>
    Task<Result<AppointmentResponseDto>> CancelAppointmentAsync(
        int patientId,
        int appointmentId,
        int minutesBeforeStart = 60
    );

    /// <summary>
    /// Reschedule an appointment (if not within X minutes of start time)
    /// </summary>
    Task<Result<AppointmentResponseDto>> RescheduleAppointmentAsync(
        int patientId,
        int appointmentId,
        RescheduleAppointmentDto dto,
        int minutesBeforeStart = 60
    );
}

public class CustomerAppointmentService : ICustomerAppointmentService
{
    private readonly IvyContext _context;
    private const int DEFAULT_DURATION_MINUTES = 30;

    public CustomerAppointmentService(IvyContext context)
    {
        _context = context;
    }

    public async Task<Result<PaginatedResult<AppointmentResponseDto>>> GetPatientAppointmentsAsync(
        int patientId,
        CustomerAppointmentFilterDto filter
    )
    {
        try
        {
            // Validate pagination parameters
            if (filter.Page < 1)
                filter.Page = 1;
            if (filter.PageSize < 1)
                filter.PageSize = 10;
            if (filter.PageSize > 100)
                filter.PageSize = 100;

            var query = _context
                .Set<Appointment>()
                .Include(a => a.Doctor)
                .ThenInclude(d => d.User)
                .Include(a => a.Patient)
                .ThenInclude(p => p.User)
                .Include(a => a.Clinic)
                .Where(a => a.PatientId == patientId)
                .AsQueryable();

            // Apply filters
            if (filter.DoctorId.HasValue)
            {
                query = query.Where(a => a.DoctorId == filter.DoctorId.Value);
            }

            if (filter.ClinicId.HasValue)
            {
                query = query.Where(a => a.ClinicId == filter.ClinicId.Value);
            }

            if (filter.Status.HasValue)
            {
                query = query.Where(a => a.Status == filter.Status.Value);
            }

            if (filter.StartDateFrom.HasValue)
            {
                query = query.Where(a => a.AppointmentDateStart >= filter.StartDateFrom.Value);
            }

            if (filter.StartDateTo.HasValue)
            {
                query = query.Where(a => a.AppointmentDateStart <= filter.StartDateTo.Value);
            }

            // Get total count
            var totalCount = await query.CountAsync();

            // Apply pagination and ordering
            var appointments = await query
                .OrderByDescending(a => a.AppointmentDateStart)
                .Skip((filter.Page - 1) * filter.PageSize)
                .Take(filter.PageSize)
                .ToListAsync();

            // Map to response DTOs
            var appointmentDtos = appointments.Select(MapToResponseDto).ToList();

            var paginatedResult = PaginatedResult<AppointmentResponseDto>.Create(
                appointmentDtos,
                totalCount,
                filter.Page,
                filter.PageSize
            );

            return Result<PaginatedResult<AppointmentResponseDto>>.Ok(
                CustomerAppointmentServiceMessageCodes.APPOINTMENTS_RETRIEVED_SUCCESS,
                paginatedResult
            );
        }
        catch (Exception)
        {
            var emptyResult = new PaginatedResult<AppointmentResponseDto>();
            return Result<PaginatedResult<AppointmentResponseDto>>.Error(
                CustomerAppointmentServiceMessageCodes.APPOINTMENTS_RETRIEVAL_FAILED,
                emptyResult
            );
        }
    }

    public async Task<Result<AppointmentResponseDto>> GetPatientAppointmentByIdAsync(
        int patientId,
        int appointmentId
    )
    {
        var appointment = await _context
            .Set<Appointment>()
            .Include(a => a.Doctor)
            .ThenInclude(d => d.User)
            .Include(a => a.Patient)
            .ThenInclude(p => p.User)
            .Include(a => a.Clinic)
            .FirstOrDefaultAsync(a => a.Id == appointmentId && a.PatientId == patientId);

        if (appointment == null)
        {
            return Result<AppointmentResponseDto>.Error(
                CustomerAppointmentServiceMessageCodes.APPOINTMENT_NOT_FOUND,
                null!
            );
        }

        var response = MapToResponseDto(appointment);

        return Result<AppointmentResponseDto>.Ok(
            CustomerAppointmentServiceMessageCodes.SUCCESS,
            response
        );
    }

    public async Task<Result<AppointmentResponseDto>> BookAppointmentAsync(
        int patientId,
        CustomerCreateAppointmentDto dto
    )
    {
        // Validate that doctor exists and is active
        var doctor = await _context
            .Set<Doctor>()
            .Include(d => d.User)
            .FirstOrDefaultAsync(d => d.UserId == dto.DoctorId);

        if (doctor == null || !doctor.User.IsActive)
        {
            return Result<AppointmentResponseDto>.Error(
                CustomerAppointmentServiceMessageCodes.DOCTOR_NOT_FOUND,
                null!
            );
        }

        // Validate that patient exists and is active
        var patient = await _context
            .Set<Patient>()
            .Include(p => p.User)
            .FirstOrDefaultAsync(p => p.UserId == patientId);

        if (patient == null || !patient.User.IsActive)
        {
            return Result<AppointmentResponseDto>.Error(
                CustomerAppointmentServiceMessageCodes.PATIENT_NOT_FOUND,
                null!
            );
        }

        // Validate that clinic exists and is active
        var clinic = await _context
            .Set<Clinic>()
            .FirstOrDefaultAsync(c => c.Id == dto.ClinicId && c.IsActive);

        if (clinic == null)
        {
            return Result<AppointmentResponseDto>.Error(
                CustomerAppointmentServiceMessageCodes.CLINIC_NOT_FOUND,
                null!
            );
        }

        // Validate that doctor works at this clinic
        var doctorClinic = await _context
            .Set<DoctorClinic>()
            .FirstOrDefaultAsync(dc => dc.DoctorId == dto.DoctorId && dc.ClinicId == dto.ClinicId);

        if (doctorClinic == null)
        {
            return Result<AppointmentResponseDto>.Error(
                CustomerAppointmentServiceMessageCodes.DOCTOR_NOT_WORK_AT_CLINIC,
                null!
            );
        }

        // Check for time slot conflicts
        var appointmentEnd = dto.AppointmentDateStart.AddMinutes(DEFAULT_DURATION_MINUTES);

        var hasConflict = await _context
            .Set<Appointment>()
            .AnyAsync(a =>
                a.DoctorId == dto.DoctorId
                && a.ClinicId == dto.ClinicId
                && a.Status != AppointmentStatus.Cancelled
                && a.Status != AppointmentStatus.Completed
                && (
                    (
                        a.AppointmentDateStart < appointmentEnd
                        && a.AppointmentDateEnd > dto.AppointmentDateStart
                    )
                )
            );

        if (hasConflict)
        {
            return Result<AppointmentResponseDto>.Error(
                CustomerAppointmentServiceMessageCodes.TIME_SLOT_CONFLICT,
                null!
            );
        }

        // Create the appointment
        var appointment = new Appointment
        {
            DoctorId = dto.DoctorId,
            PatientId = patientId,
            ClinicId = dto.ClinicId,
            AppointmentDateStart = dto.AppointmentDateStart,
            AppointmentDateEnd = appointmentEnd,
            Status = AppointmentStatus.Pending,
            Notes = dto.Notes,
        };

        _context.Set<Appointment>().Add(appointment);
        await _context.SaveChangesAsync();

        // Load related entities for response
        await _context.Entry(appointment).Reference(a => a.Doctor).LoadAsync();
        await _context.Entry(appointment.Doctor).Reference(d => d.User).LoadAsync();
        await _context.Entry(appointment).Reference(a => a.Patient).LoadAsync();
        await _context.Entry(appointment.Patient).Reference(p => p.User).LoadAsync();
        await _context.Entry(appointment).Reference(a => a.Clinic).LoadAsync();

        var response = MapToResponseDto(appointment);

        return Result<AppointmentResponseDto>.Ok(
            CustomerAppointmentServiceMessageCodes.APPOINTMENT_BOOKED_SUCCESS,
            response
        );
    }

    public async Task<Result<AppointmentResponseDto>> CancelAppointmentAsync(
        int patientId,
        int appointmentId,
        int minutesBeforeStart = 60
    )
    {
        var appointment = await _context
            .Set<Appointment>()
            .Include(a => a.Doctor)
            .ThenInclude(d => d.User)
            .Include(a => a.Patient)
            .ThenInclude(p => p.User)
            .Include(a => a.Clinic)
            .FirstOrDefaultAsync(a => a.Id == appointmentId && a.PatientId == patientId);

        if (appointment == null)
        {
            return Result<AppointmentResponseDto>.Error(
                CustomerAppointmentServiceMessageCodes.APPOINTMENT_NOT_FOUND,
                null!
            );
        }

        if (appointment.Status == AppointmentStatus.Cancelled)
        {
            return Result<AppointmentResponseDto>.Error(
                CustomerAppointmentServiceMessageCodes.APPOINTMENT_ALREADY_CANCELLED,
                null!
            );
        }

        if (appointment.Status == AppointmentStatus.Completed)
        {
            return Result<AppointmentResponseDto>.Error(
                CustomerAppointmentServiceMessageCodes.APPOINTMENT_ALREADY_COMPLETED,
                null!
            );
        }

        // Check if appointment is within X minutes of start time
        var timeUntilAppointment = appointment.AppointmentDateStart - DateTime.Now;
        if (timeUntilAppointment.TotalMinutes < minutesBeforeStart)
        {
            return Result<AppointmentResponseDto>.Error(
                CustomerAppointmentServiceMessageCodes.CANCELLATION_TOO_LATE,
                null!
            );
        }

        appointment.Status = AppointmentStatus.Cancelled;
        await _context.SaveChangesAsync();

        var response = MapToResponseDto(appointment);

        return Result<AppointmentResponseDto>.Ok(
            CustomerAppointmentServiceMessageCodes.APPOINTMENT_CANCELLED_SUCCESS,
            response
        );
    }

    public async Task<Result<AppointmentResponseDto>> RescheduleAppointmentAsync(
        int patientId,
        int appointmentId,
        RescheduleAppointmentDto dto,
        int minutesBeforeStart = 60
    )
    {
        var appointment = await _context
            .Set<Appointment>()
            .Include(a => a.Doctor)
            .ThenInclude(d => d.User)
            .Include(a => a.Patient)
            .ThenInclude(p => p.User)
            .Include(a => a.Clinic)
            .FirstOrDefaultAsync(a => a.Id == appointmentId && a.PatientId == patientId);

        if (appointment == null)
        {
            return Result<AppointmentResponseDto>.Error(
                CustomerAppointmentServiceMessageCodes.APPOINTMENT_NOT_FOUND,
                null!
            );
        }

        if (appointment.Status == AppointmentStatus.Cancelled)
        {
            return Result<AppointmentResponseDto>.Error(
                CustomerAppointmentServiceMessageCodes.CANNOT_RESCHEDULE_CANCELLED,
                null!
            );
        }

        if (appointment.Status == AppointmentStatus.Completed)
        {
            return Result<AppointmentResponseDto>.Error(
                CustomerAppointmentServiceMessageCodes.CANNOT_RESCHEDULE_COMPLETED,
                null!
            );
        }

        // Check if appointment is within X minutes of start time
        var timeUntilAppointment = appointment.AppointmentDateStart - DateTime.Now;
        if (timeUntilAppointment.TotalMinutes < minutesBeforeStart)
        {
            return Result<AppointmentResponseDto>.Error(
                CustomerAppointmentServiceMessageCodes.RESCHEDULE_TOO_LATE,
                null!
            );
        }

        // Check for time slot conflicts with the new time
        var newAppointmentEnd = dto.NewAppointmentDateStart.AddMinutes(DEFAULT_DURATION_MINUTES);

        var hasConflict = await _context
            .Set<Appointment>()
            .AnyAsync(a =>
                a.Id != appointmentId // Exclude current appointment
                && a.DoctorId == appointment.DoctorId
                && a.ClinicId == appointment.ClinicId
                && a.Status != AppointmentStatus.Cancelled
                && a.Status != AppointmentStatus.Completed
                && (
                    (
                        a.AppointmentDateStart < newAppointmentEnd
                        && a.AppointmentDateEnd > dto.NewAppointmentDateStart
                    )
                )
            );

        if (hasConflict)
        {
            return Result<AppointmentResponseDto>.Error(
                CustomerAppointmentServiceMessageCodes.TIME_SLOT_CONFLICT,
                null!
            );
        }

        appointment.AppointmentDateStart = dto.NewAppointmentDateStart;
        appointment.AppointmentDateEnd = newAppointmentEnd;
        appointment.Status = AppointmentStatus.Pending; // Reset to pending

        await _context.SaveChangesAsync();

        var response = MapToResponseDto(appointment);

        return Result<AppointmentResponseDto>.Ok(
            CustomerAppointmentServiceMessageCodes.APPOINTMENT_RESCHEDULED_SUCCESS,
            response
        );
    }

    private AppointmentResponseDto MapToResponseDto(Appointment appointment)
    {
        return new AppointmentResponseDto
        {
            Id = appointment.Id,
            DoctorId = appointment.DoctorId,
            DoctorName = $"{appointment.Doctor.User.FirstName} {appointment.Doctor.User.LastName}",
            PatientId = appointment.PatientId,
            PatientName =
                $"{appointment.Patient.User.FirstName} {appointment.Patient.User.LastName}",
            ClinicId = appointment.ClinicId,
            ClinicName = appointment.Clinic.NameEn,
            AppointmentDateStart = appointment.AppointmentDateStart,
            AppointmentDateEnd = appointment.AppointmentDateEnd,
            Status = appointment.Status,
            Notes = appointment.Notes,
        };
    }
}

public static class CustomerAppointmentServiceMessageCodes
{
    public const string SUCCESS = "CUSTOMER_APPOINTMENT_SUCCESS";
    public const string APPOINTMENTS_RETRIEVED_SUCCESS = "CUSTOMER_APPOINTMENTS_RETRIEVED_SUCCESS";
    public const string APPOINTMENT_BOOKED_SUCCESS = "CUSTOMER_APPOINTMENT_BOOKED_SUCCESS";
    public const string APPOINTMENT_CANCELLED_SUCCESS = "CUSTOMER_APPOINTMENT_CANCELLED_SUCCESS";
    public const string APPOINTMENT_RESCHEDULED_SUCCESS =
        "CUSTOMER_APPOINTMENT_RESCHEDULED_SUCCESS";

    // Error codes
    public const string APPOINTMENTS_RETRIEVAL_FAILED = "CUSTOMER_APPOINTMENTS_RETRIEVAL_FAILED";
    public const string APPOINTMENT_NOT_FOUND = "CUSTOMER_APPOINTMENT_NOT_FOUND";
    public const string DOCTOR_NOT_FOUND = "CUSTOMER_DOCTOR_NOT_FOUND";
    public const string PATIENT_NOT_FOUND = "CUSTOMER_PATIENT_NOT_FOUND";
    public const string CLINIC_NOT_FOUND = "CUSTOMER_CLINIC_NOT_FOUND";
    public const string DOCTOR_NOT_WORK_AT_CLINIC = "CUSTOMER_DOCTOR_NOT_WORK_AT_CLINIC";
    public const string TIME_SLOT_CONFLICT = "CUSTOMER_TIME_SLOT_CONFLICT";
    public const string APPOINTMENT_ALREADY_CANCELLED = "CUSTOMER_APPOINTMENT_ALREADY_CANCELLED";
    public const string APPOINTMENT_ALREADY_COMPLETED = "CUSTOMER_APPOINTMENT_ALREADY_COMPLETED";
    public const string CANCELLATION_TOO_LATE = "CUSTOMER_CANCELLATION_TOO_LATE";
    public const string RESCHEDULE_TOO_LATE = "CUSTOMER_RESCHEDULE_TOO_LATE";
    public const string CANNOT_RESCHEDULE_CANCELLED = "CUSTOMER_CANNOT_RESCHEDULE_CANCELLED";
    public const string CANNOT_RESCHEDULE_COMPLETED = "CUSTOMER_CANNOT_RESCHEDULE_COMPLETED";
}
