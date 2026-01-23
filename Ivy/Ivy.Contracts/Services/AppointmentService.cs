using Ivy.Contracts.Models;
using Ivy.Core.DataContext;
using Ivy.Core.Entities;
using IvyBackend;
using Microsoft.EntityFrameworkCore;

namespace Ivy.Contracts.Services;

public interface IAppointmentService
{
    /// <summary>
    /// Get paginated list of appointments with filters
    /// </summary>
    Task<Result<PaginatedResult<AppointmentResponseDto>>> GetAllAppointmentsAsync(
        AppointmentFilterDto filter
    );

    /// <summary>
    /// Book a new appointment
    /// </summary>
    Task<Result<AppointmentResponseDto>> BookAppointmentAsync(CreateAppointmentDto dto);

    /// <summary>
    /// Cancel an appointment (if not within X minutes of start time)
    /// </summary>
    Task<Result<AppointmentResponseDto>> CancelAppointmentAsync(
        int appointmentId,
        int minutesBeforeStart = 60
    );

    /// <summary>
    /// Reschedule an appointment (if not within X minutes of start time)
    /// </summary>
    Task<Result<AppointmentResponseDto>> RescheduleAppointmentAsync(
        int appointmentId,
        RescheduleAppointmentDto dto,
        int minutesBeforeStart = 60
    );

    /// <summary>
    /// Mark appointment as In Progress
    /// </summary>
    Task<Result<AppointmentResponseDto>> MarkAppointmentInProgressAsync(int appointmentId);

    /// <summary>
    /// Mark appointment as Confirmed
    /// </summary>
    Task<Result<AppointmentResponseDto>> ConfirmAppointmentAsync(int appointmentId);

    /// <summary>
    /// Get appointment by ID
    /// </summary>
    Task<Result<AppointmentResponseDto>> GetAppointmentByIdAsync(int appointmentId);
}

public class AppointmentService : IAppointmentService
{
    private readonly IvyContext _context;
    private const int DEFAULT_DURATION_MINUTES = 30;

    public AppointmentService(IvyContext context)
    {
        _context = context;
    }

    public async Task<Result<PaginatedResult<AppointmentResponseDto>>> GetAllAppointmentsAsync(
        AppointmentFilterDto filter
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
                .AsQueryable();

            // Apply filters
            if (filter.DoctorId.HasValue)
            {
                query = query.Where(a => a.DoctorId == filter.DoctorId.Value);
            }

            if (filter.PatientId.HasValue)
            {
                query = query.Where(a => a.PatientId == filter.PatientId.Value);
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

            if (!string.IsNullOrWhiteSpace(filter.SearchTerm))
            {
                var searchTerm = filter.SearchTerm.ToLower();
                query = query.Where(a =>
                    (a.Doctor.User.FirstName + " " + a.Doctor.User.LastName)
                        .ToLower()
                        .Contains(searchTerm)
                    || (a.Patient.User.FirstName + " " + a.Patient.User.LastName)
                        .ToLower()
                        .Contains(searchTerm)
                );
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
                AppointmentServiceMessageCodes.APPOINTMENTS_RETRIEVED_SUCCESS,
                paginatedResult
            );
        }
        catch (Exception)
        {
            var emptyResult = new PaginatedResult<AppointmentResponseDto>();
            return Result<PaginatedResult<AppointmentResponseDto>>.Error(
                AppointmentServiceMessageCodes.APPOINTMENTS_RETRIEVAL_FAILED,
                emptyResult
            );
        }
    }

    public async Task<Result<AppointmentResponseDto>> BookAppointmentAsync(CreateAppointmentDto dto)
    {
        // Validate that doctor exists and is active
        var doctor = await _context
            .Set<Doctor>()
            .Include(d => d.User)
            .FirstOrDefaultAsync(d => d.UserId == dto.DoctorId);

        if (doctor == null || !doctor.User.IsActive)
        {
            return Result<AppointmentResponseDto>.Error(
                AppointmentServiceMessageCodes.DOCTOR_NOT_FOUND,
                null!
            );
        }

        // Validate that patient exists and is active
        var patient = await _context
            .Set<Patient>()
            .Include(p => p.User)
            .FirstOrDefaultAsync(p => p.UserId == dto.PatientId);

        if (patient == null || !patient.User.IsActive)
        {
            return Result<AppointmentResponseDto>.Error(
                AppointmentServiceMessageCodes.PATIENT_NOT_FOUND,
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
                AppointmentServiceMessageCodes.CLINIC_NOT_FOUND,
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
                AppointmentServiceMessageCodes.DOCTOR_NOT_WORK_AT_CLINIC,
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
                AppointmentServiceMessageCodes.TIME_SLOT_CONFLICT,
                null!
            );
        }

        // Create the appointment
        var appointment = new Appointment
        {
            DoctorId = dto.DoctorId,
            PatientId = dto.PatientId,
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
            AppointmentServiceMessageCodes.APPOINTMENT_BOOKED_SUCCESS,
            response
        );
    }

    public async Task<Result<AppointmentResponseDto>> CancelAppointmentAsync(
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
            .FirstOrDefaultAsync(a => a.Id == appointmentId);

        if (appointment == null)
        {
            return Result<AppointmentResponseDto>.Error(
                AppointmentServiceMessageCodes.APPOINTMENT_NOT_FOUND,
                null!
            );
        }

        if (appointment.Status == AppointmentStatus.Cancelled)
        {
            return Result<AppointmentResponseDto>.Error(
                AppointmentServiceMessageCodes.APPOINTMENT_ALREADY_CANCELLED,
                null!
            );
        }

        if (appointment.Status == AppointmentStatus.Completed)
        {
            return Result<AppointmentResponseDto>.Error(
                AppointmentServiceMessageCodes.APPOINTMENT_ALREADY_COMPLETED,
                null!
            );
        }

        // Check if appointment is within X minutes of start time
        var timeUntilAppointment = appointment.AppointmentDateStart - DateTime.Now;
        if (timeUntilAppointment.TotalMinutes < minutesBeforeStart)
        {
            return Result<AppointmentResponseDto>.Error(
                AppointmentServiceMessageCodes.CANCELLATION_TOO_LATE,
                null!
            );
        }

        appointment.Status = AppointmentStatus.Cancelled;
        await _context.SaveChangesAsync();

        var response = MapToResponseDto(appointment);

        return Result<AppointmentResponseDto>.Ok(
            AppointmentServiceMessageCodes.APPOINTMENT_CANCELLED_SUCCESS,
            response
        );
    }

    public async Task<Result<AppointmentResponseDto>> RescheduleAppointmentAsync(
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
            .FirstOrDefaultAsync(a => a.Id == appointmentId);

        if (appointment == null)
        {
            return Result<AppointmentResponseDto>.Error(
                AppointmentServiceMessageCodes.APPOINTMENT_NOT_FOUND,
                null!
            );
        }

        if (appointment.Status == AppointmentStatus.Cancelled)
        {
            return Result<AppointmentResponseDto>.Error(
                AppointmentServiceMessageCodes.CANNOT_RESCHEDULE_CANCELLED,
                null!
            );
        }

        if (appointment.Status == AppointmentStatus.Completed)
        {
            return Result<AppointmentResponseDto>.Error(
                AppointmentServiceMessageCodes.CANNOT_RESCHEDULE_COMPLETED,
                null!
            );
        }

        // Check if appointment is within X minutes of start time
        var timeUntilAppointment = appointment.AppointmentDateStart - DateTime.Now;
        if (timeUntilAppointment.TotalMinutes < minutesBeforeStart)
        {
            return Result<AppointmentResponseDto>.Error(
                AppointmentServiceMessageCodes.RESCHEDULE_TOO_LATE,
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
                AppointmentServiceMessageCodes.TIME_SLOT_CONFLICT,
                null!
            );
        }

        appointment.AppointmentDateStart = dto.NewAppointmentDateStart;
        appointment.AppointmentDateEnd = newAppointmentEnd;
        appointment.Status = AppointmentStatus.Pending; // Reset to pending

        await _context.SaveChangesAsync();

        var response = MapToResponseDto(appointment);

        return Result<AppointmentResponseDto>.Ok(
            AppointmentServiceMessageCodes.APPOINTMENT_RESCHEDULED_SUCCESS,
            response
        );
    }

    public async Task<Result<AppointmentResponseDto>> MarkAppointmentInProgressAsync(
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
            .FirstOrDefaultAsync(a => a.Id == appointmentId);

        if (appointment == null)
        {
            return Result<AppointmentResponseDto>.Error(
                AppointmentServiceMessageCodes.APPOINTMENT_NOT_FOUND,
                null!
            );
        }

        if (appointment.Status == AppointmentStatus.Cancelled)
        {
            return Result<AppointmentResponseDto>.Error(
                AppointmentServiceMessageCodes.CANNOT_MARK_CANCELLED_AS_INPROGRESS,
                null!
            );
        }

        if (appointment.Status == AppointmentStatus.Completed)
        {
            return Result<AppointmentResponseDto>.Error(
                AppointmentServiceMessageCodes.CANNOT_MARK_COMPLETED_AS_INPROGRESS,
                null!
            );
        }

        appointment.Status = AppointmentStatus.InProgress;
        await _context.SaveChangesAsync();

        var response = MapToResponseDto(appointment);

        return Result<AppointmentResponseDto>.Ok(
            AppointmentServiceMessageCodes.APPOINTMENT_MARKED_INPROGRESS_SUCCESS,
            response
        );
    }

    public async Task<Result<AppointmentResponseDto>> ConfirmAppointmentAsync(int appointmentId)
    {
        var appointment = await _context
            .Set<Appointment>()
            .Include(a => a.Doctor)
            .ThenInclude(d => d.User)
            .Include(a => a.Patient)
            .ThenInclude(p => p.User)
            .Include(a => a.Clinic)
            .FirstOrDefaultAsync(a => a.Id == appointmentId);

        if (appointment == null)
        {
            return Result<AppointmentResponseDto>.Error(
                AppointmentServiceMessageCodes.APPOINTMENT_NOT_FOUND,
                null!
            );
        }

        if (appointment.Status == AppointmentStatus.Cancelled)
        {
            return Result<AppointmentResponseDto>.Error(
                AppointmentServiceMessageCodes.CANNOT_CONFIRM_CANCELLED,
                null!
            );
        }

        if (appointment.Status == AppointmentStatus.Completed)
        {
            return Result<AppointmentResponseDto>.Error(
                AppointmentServiceMessageCodes.CANNOT_CONFIRM_COMPLETED,
                null!
            );
        }

        appointment.Status = AppointmentStatus.Confirmed;
        await _context.SaveChangesAsync();

        var response = MapToResponseDto(appointment);

        return Result<AppointmentResponseDto>.Ok(
            AppointmentServiceMessageCodes.APPOINTMENT_CONFIRMED_SUCCESS,
            response
        );
    }

    public async Task<Result<AppointmentResponseDto>> GetAppointmentByIdAsync(int appointmentId)
    {
        var appointment = await _context
            .Set<Appointment>()
            .Include(a => a.Doctor)
            .ThenInclude(d => d.User)
            .Include(a => a.Patient)
            .ThenInclude(p => p.User)
            .Include(a => a.Clinic)
            .FirstOrDefaultAsync(a => a.Id == appointmentId);

        if (appointment == null)
        {
            return Result<AppointmentResponseDto>.Error(
                AppointmentServiceMessageCodes.APPOINTMENT_NOT_FOUND,
                null!
            );
        }

        var response = MapToResponseDto(appointment);

        return Result<AppointmentResponseDto>.Ok(AppointmentServiceMessageCodes.SUCCESS, response);
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

public static class AppointmentServiceMessageCodes
{
    public const string SUCCESS = "APPOINTMENT_SUCCESS";
    public const string APPOINTMENTS_RETRIEVED_SUCCESS = "APPOINTMENTS_RETRIEVED_SUCCESS";
    public const string APPOINTMENT_BOOKED_SUCCESS = "APPOINTMENT_BOOKED_SUCCESS";
    public const string APPOINTMENT_CANCELLED_SUCCESS = "APPOINTMENT_CANCELLED_SUCCESS";
    public const string APPOINTMENT_RESCHEDULED_SUCCESS = "APPOINTMENT_RESCHEDULED_SUCCESS";
    public const string APPOINTMENT_MARKED_INPROGRESS_SUCCESS =
        "APPOINTMENT_MARKED_INPROGRESS_SUCCESS";
    public const string APPOINTMENT_CONFIRMED_SUCCESS = "APPOINTMENT_CONFIRMED_SUCCESS";

    // Error codes
    public const string APPOINTMENTS_RETRIEVAL_FAILED = "APPOINTMENTS_RETRIEVAL_FAILED";
    public const string APPOINTMENT_NOT_FOUND = "APPOINTMENT_NOT_FOUND";
    public const string DOCTOR_NOT_FOUND = "DOCTOR_NOT_FOUND";
    public const string PATIENT_NOT_FOUND = "PATIENT_NOT_FOUND";
    public const string CLINIC_NOT_FOUND = "CLINIC_NOT_FOUND";
    public const string DOCTOR_NOT_WORK_AT_CLINIC = "DOCTOR_NOT_WORK_AT_CLINIC";
    public const string TIME_SLOT_CONFLICT = "TIME_SLOT_CONFLICT";
    public const string APPOINTMENT_ALREADY_CANCELLED = "APPOINTMENT_ALREADY_CANCELLED";
    public const string APPOINTMENT_ALREADY_COMPLETED = "APPOINTMENT_ALREADY_COMPLETED";
    public const string CANCELLATION_TOO_LATE = "CANCELLATION_TOO_LATE";
    public const string RESCHEDULE_TOO_LATE = "RESCHEDULE_TOO_LATE";
    public const string CANNOT_RESCHEDULE_CANCELLED = "CANNOT_RESCHEDULE_CANCELLED";
    public const string CANNOT_RESCHEDULE_COMPLETED = "CANNOT_RESCHEDULE_COMPLETED";
    public const string CANNOT_MARK_CANCELLED_AS_INPROGRESS = "CANNOT_MARK_CANCELLED_AS_INPROGRESS";
    public const string CANNOT_MARK_COMPLETED_AS_INPROGRESS = "CANNOT_MARK_COMPLETED_AS_INPROGRESS";
    public const string CANNOT_CONFIRM_CANCELLED = "CANNOT_CONFIRM_CANCELLED";
    public const string CANNOT_CONFIRM_COMPLETED = "CANNOT_CONFIRM_COMPLETED";
}
