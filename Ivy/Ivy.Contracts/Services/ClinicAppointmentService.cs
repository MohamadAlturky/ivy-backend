using Ivy.Contracts.Models;
using Ivy.Core.DataContext;
using Ivy.Core.Entities;
using IvyBackend;
using Microsoft.EntityFrameworkCore;

namespace Ivy.Contracts.Services;

public interface IClinicAppointmentService
{
    /// <summary>
    /// Get paginated list of appointments for a specific clinic
    /// </summary>
    Task<Result<PaginatedResult<AppointmentResponseDto>>> GetClinicAppointmentsAsync(
        int clinicId,
        ClinicAppointmentFilterDto filter
    );

    /// <summary>
    /// Get a specific appointment by ID for a clinic
    /// </summary>
    Task<Result<AppointmentResponseDto>> GetClinicAppointmentByIdAsync(
        int clinicId,
        int appointmentId
    );

    /// <summary>
    /// Confirm an appointment
    /// </summary>
    Task<Result<AppointmentResponseDto>> ConfirmAppointmentAsync(int clinicId, int appointmentId);

    /// <summary>
    /// Mark appointment as In Progress
    /// </summary>
    Task<Result<AppointmentResponseDto>> MarkAppointmentInProgressAsync(
        int clinicId,
        int appointmentId
    );

    /// <summary>
    /// Mark appointment as Completed
    /// </summary>
    Task<Result<AppointmentResponseDto>> CompleteAppointmentAsync(int clinicId, int appointmentId);

    /// <summary>
    /// Cancel an appointment from clinic side
    /// </summary>
    Task<Result<AppointmentResponseDto>> CancelAppointmentAsync(
        int clinicId,
        int appointmentId,
        string? reason = null
    );

    /// <summary>
    /// Reschedule an appointment from clinic side
    /// </summary>
    Task<Result<AppointmentResponseDto>> RescheduleAppointmentAsync(
        int clinicId,
        int appointmentId,
        RescheduleAppointmentDto dto
    );
}

public class ClinicAppointmentService : IClinicAppointmentService
{
    private readonly IvyContext _context;
    private const int DEFAULT_DURATION_MINUTES = 30;

    public ClinicAppointmentService(IvyContext context)
    {
        _context = context;
    }

    public async Task<Result<PaginatedResult<AppointmentResponseDto>>> GetClinicAppointmentsAsync(
        int clinicId,
        ClinicAppointmentFilterDto filter
    )
    {
        try
        {
            // Validate clinic exists
            var clinicExists = await _context.Set<Clinic>().AnyAsync(c => c.Id == clinicId);
            if (!clinicExists)
            {
                return Result<PaginatedResult<AppointmentResponseDto>>.Error(
                    ClinicAppointmentServiceMessageCodes.CLINIC_NOT_FOUND,
                    new PaginatedResult<AppointmentResponseDto>()
                );
            }

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
                .Where(a => a.ClinicId == clinicId)
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
                    || (
                        a.Patient.User.PhoneNumber != null
                        && a.Patient.User.PhoneNumber.Contains(searchTerm)
                    )
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
                ClinicAppointmentServiceMessageCodes.APPOINTMENTS_RETRIEVED_SUCCESS,
                paginatedResult
            );
        }
        catch (Exception)
        {
            var emptyResult = new PaginatedResult<AppointmentResponseDto>();
            return Result<PaginatedResult<AppointmentResponseDto>>.Error(
                ClinicAppointmentServiceMessageCodes.APPOINTMENTS_RETRIEVAL_FAILED,
                emptyResult
            );
        }
    }

    public async Task<Result<AppointmentResponseDto>> GetClinicAppointmentByIdAsync(
        int clinicId,
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
            .FirstOrDefaultAsync(a => a.Id == appointmentId && a.ClinicId == clinicId);

        if (appointment == null)
        {
            return Result<AppointmentResponseDto>.Error(
                ClinicAppointmentServiceMessageCodes.APPOINTMENT_NOT_FOUND,
                null!
            );
        }

        var response = MapToResponseDto(appointment);

        return Result<AppointmentResponseDto>.Ok(
            ClinicAppointmentServiceMessageCodes.SUCCESS,
            response
        );
    }

    public async Task<Result<AppointmentResponseDto>> ConfirmAppointmentAsync(
        int clinicId,
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
            .FirstOrDefaultAsync(a => a.Id == appointmentId && a.ClinicId == clinicId);

        if (appointment == null)
        {
            return Result<AppointmentResponseDto>.Error(
                ClinicAppointmentServiceMessageCodes.APPOINTMENT_NOT_FOUND,
                null!
            );
        }

        if (appointment.Status == AppointmentStatus.Cancelled)
        {
            return Result<AppointmentResponseDto>.Error(
                ClinicAppointmentServiceMessageCodes.CANNOT_CONFIRM_CANCELLED,
                null!
            );
        }

        if (appointment.Status == AppointmentStatus.Completed)
        {
            return Result<AppointmentResponseDto>.Error(
                ClinicAppointmentServiceMessageCodes.CANNOT_CONFIRM_COMPLETED,
                null!
            );
        }

        appointment.Status = AppointmentStatus.Confirmed;
        await _context.SaveChangesAsync();

        var response = MapToResponseDto(appointment);

        return Result<AppointmentResponseDto>.Ok(
            ClinicAppointmentServiceMessageCodes.APPOINTMENT_CONFIRMED_SUCCESS,
            response
        );
    }

    public async Task<Result<AppointmentResponseDto>> MarkAppointmentInProgressAsync(
        int clinicId,
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
            .FirstOrDefaultAsync(a => a.Id == appointmentId && a.ClinicId == clinicId);

        if (appointment == null)
        {
            return Result<AppointmentResponseDto>.Error(
                ClinicAppointmentServiceMessageCodes.APPOINTMENT_NOT_FOUND,
                null!
            );
        }

        if (appointment.Status == AppointmentStatus.Cancelled)
        {
            return Result<AppointmentResponseDto>.Error(
                ClinicAppointmentServiceMessageCodes.CANNOT_MARK_CANCELLED_AS_INPROGRESS,
                null!
            );
        }

        if (appointment.Status == AppointmentStatus.Completed)
        {
            return Result<AppointmentResponseDto>.Error(
                ClinicAppointmentServiceMessageCodes.CANNOT_MARK_COMPLETED_AS_INPROGRESS,
                null!
            );
        }

        appointment.Status = AppointmentStatus.InProgress;
        await _context.SaveChangesAsync();

        var response = MapToResponseDto(appointment);

        return Result<AppointmentResponseDto>.Ok(
            ClinicAppointmentServiceMessageCodes.APPOINTMENT_MARKED_INPROGRESS_SUCCESS,
            response
        );
    }

    public async Task<Result<AppointmentResponseDto>> CompleteAppointmentAsync(
        int clinicId,
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
            .FirstOrDefaultAsync(a => a.Id == appointmentId && a.ClinicId == clinicId);

        if (appointment == null)
        {
            return Result<AppointmentResponseDto>.Error(
                ClinicAppointmentServiceMessageCodes.APPOINTMENT_NOT_FOUND,
                null!
            );
        }

        if (appointment.Status == AppointmentStatus.Cancelled)
        {
            return Result<AppointmentResponseDto>.Error(
                ClinicAppointmentServiceMessageCodes.CANNOT_COMPLETE_CANCELLED,
                null!
            );
        }

        if (appointment.Status == AppointmentStatus.Completed)
        {
            return Result<AppointmentResponseDto>.Error(
                ClinicAppointmentServiceMessageCodes.APPOINTMENT_ALREADY_COMPLETED,
                null!
            );
        }

        appointment.Status = AppointmentStatus.Completed;
        await _context.SaveChangesAsync();

        var response = MapToResponseDto(appointment);

        return Result<AppointmentResponseDto>.Ok(
            ClinicAppointmentServiceMessageCodes.APPOINTMENT_COMPLETED_SUCCESS,
            response
        );
    }

    public async Task<Result<AppointmentResponseDto>> CancelAppointmentAsync(
        int clinicId,
        int appointmentId,
        string? reason = null
    )
    {
        var appointment = await _context
            .Set<Appointment>()
            .Include(a => a.Doctor)
            .ThenInclude(d => d.User)
            .Include(a => a.Patient)
            .ThenInclude(p => p.User)
            .Include(a => a.Clinic)
            .FirstOrDefaultAsync(a => a.Id == appointmentId && a.ClinicId == clinicId);

        if (appointment == null)
        {
            return Result<AppointmentResponseDto>.Error(
                ClinicAppointmentServiceMessageCodes.APPOINTMENT_NOT_FOUND,
                null!
            );
        }

        if (appointment.Status == AppointmentStatus.Cancelled)
        {
            return Result<AppointmentResponseDto>.Error(
                ClinicAppointmentServiceMessageCodes.APPOINTMENT_ALREADY_CANCELLED,
                null!
            );
        }

        if (appointment.Status == AppointmentStatus.Completed)
        {
            return Result<AppointmentResponseDto>.Error(
                ClinicAppointmentServiceMessageCodes.APPOINTMENT_ALREADY_COMPLETED,
                null!
            );
        }

        appointment.Status = AppointmentStatus.Cancelled;
        if (!string.IsNullOrWhiteSpace(reason))
        {
            appointment.Notes = string.IsNullOrWhiteSpace(appointment.Notes)
                ? $"Cancelled by clinic: {reason}"
                : $"{appointment.Notes}\nCancelled by clinic: {reason}";
        }
        await _context.SaveChangesAsync();

        var response = MapToResponseDto(appointment);

        return Result<AppointmentResponseDto>.Ok(
            ClinicAppointmentServiceMessageCodes.APPOINTMENT_CANCELLED_SUCCESS,
            response
        );
    }

    public async Task<Result<AppointmentResponseDto>> RescheduleAppointmentAsync(
        int clinicId,
        int appointmentId,
        RescheduleAppointmentDto dto
    )
    {
        var appointment = await _context
            .Set<Appointment>()
            .Include(a => a.Doctor)
            .ThenInclude(d => d.User)
            .Include(a => a.Patient)
            .ThenInclude(p => p.User)
            .Include(a => a.Clinic)
            .FirstOrDefaultAsync(a => a.Id == appointmentId && a.ClinicId == clinicId);

        if (appointment == null)
        {
            return Result<AppointmentResponseDto>.Error(
                ClinicAppointmentServiceMessageCodes.APPOINTMENT_NOT_FOUND,
                null!
            );
        }

        if (appointment.Status == AppointmentStatus.Cancelled)
        {
            return Result<AppointmentResponseDto>.Error(
                ClinicAppointmentServiceMessageCodes.CANNOT_RESCHEDULE_CANCELLED,
                null!
            );
        }

        if (appointment.Status == AppointmentStatus.Completed)
        {
            return Result<AppointmentResponseDto>.Error(
                ClinicAppointmentServiceMessageCodes.CANNOT_RESCHEDULE_COMPLETED,
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
                ClinicAppointmentServiceMessageCodes.TIME_SLOT_CONFLICT,
                null!
            );
        }

        appointment.AppointmentDateStart = dto.NewAppointmentDateStart;
        appointment.AppointmentDateEnd = newAppointmentEnd;
        appointment.Status = AppointmentStatus.Pending; // Reset to pending

        await _context.SaveChangesAsync();

        var response = MapToResponseDto(appointment);

        return Result<AppointmentResponseDto>.Ok(
            ClinicAppointmentServiceMessageCodes.APPOINTMENT_RESCHEDULED_SUCCESS,
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

public static class ClinicAppointmentServiceMessageCodes
{
    public const string SUCCESS = "CLINIC_APPOINTMENT_SUCCESS";
    public const string APPOINTMENTS_RETRIEVED_SUCCESS = "CLINIC_APPOINTMENTS_RETRIEVED_SUCCESS";
    public const string APPOINTMENT_CONFIRMED_SUCCESS = "CLINIC_APPOINTMENT_CONFIRMED_SUCCESS";
    public const string APPOINTMENT_MARKED_INPROGRESS_SUCCESS =
        "CLINIC_APPOINTMENT_MARKED_INPROGRESS_SUCCESS";
    public const string APPOINTMENT_COMPLETED_SUCCESS = "CLINIC_APPOINTMENT_COMPLETED_SUCCESS";
    public const string APPOINTMENT_CANCELLED_SUCCESS = "CLINIC_APPOINTMENT_CANCELLED_SUCCESS";
    public const string APPOINTMENT_RESCHEDULED_SUCCESS = "CLINIC_APPOINTMENT_RESCHEDULED_SUCCESS";
    public const string APPOINTMENT_MARKED_NOSHOW_SUCCESS =
        "CLINIC_APPOINTMENT_MARKED_NOSHOW_SUCCESS";
    public const string STATISTICS_RETRIEVED_SUCCESS = "CLINIC_STATISTICS_RETRIEVED_SUCCESS";

    // Error codes
    public const string APPOINTMENTS_RETRIEVAL_FAILED = "CLINIC_APPOINTMENTS_RETRIEVAL_FAILED";
    public const string APPOINTMENT_NOT_FOUND = "CLINIC_APPOINTMENT_NOT_FOUND";
    public const string CLINIC_NOT_FOUND = "CLINIC_NOT_FOUND";
    public const string TIME_SLOT_CONFLICT = "CLINIC_TIME_SLOT_CONFLICT";
    public const string APPOINTMENT_ALREADY_CANCELLED = "CLINIC_APPOINTMENT_ALREADY_CANCELLED";
    public const string APPOINTMENT_ALREADY_COMPLETED = "CLINIC_APPOINTMENT_ALREADY_COMPLETED";
    public const string CANNOT_CONFIRM_CANCELLED = "CLINIC_CANNOT_CONFIRM_CANCELLED";
    public const string CANNOT_CONFIRM_COMPLETED = "CLINIC_CANNOT_CONFIRM_COMPLETED";
    public const string CANNOT_MARK_CANCELLED_AS_INPROGRESS =
        "CLINIC_CANNOT_MARK_CANCELLED_AS_INPROGRESS";
    public const string CANNOT_MARK_COMPLETED_AS_INPROGRESS =
        "CLINIC_CANNOT_MARK_COMPLETED_AS_INPROGRESS";
    public const string CANNOT_COMPLETE_CANCELLED = "CLINIC_CANNOT_COMPLETE_CANCELLED";
    public const string CANNOT_RESCHEDULE_CANCELLED = "CLINIC_CANNOT_RESCHEDULE_CANCELLED";
    public const string CANNOT_RESCHEDULE_COMPLETED = "CLINIC_CANNOT_RESCHEDULE_COMPLETED";
    public const string CANNOT_MARK_CANCELLED_AS_NOSHOW = "CLINIC_CANNOT_MARK_CANCELLED_AS_NOSHOW";
    public const string CANNOT_MARK_COMPLETED_AS_NOSHOW = "CLINIC_CANNOT_MARK_COMPLETED_AS_NOSHOW";
    public const string STATISTICS_RETRIEVAL_FAILED = "CLINIC_STATISTICS_RETRIEVAL_FAILED";
}
