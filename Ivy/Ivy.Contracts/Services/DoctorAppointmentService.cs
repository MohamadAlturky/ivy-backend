using Ivy.Contracts.Models;
using Ivy.Core.DataContext;
using Ivy.Core.Entities;
using IvyBackend;
using Microsoft.EntityFrameworkCore;

namespace Ivy.Contracts.Services;

public interface IDoctorAppointmentService
{
    /// <summary>
    /// Get paginated list of appointments for a specific doctor
    /// </summary>
    Task<Result<PaginatedResult<AppointmentResponseDto>>> GetDoctorAppointmentsAsync(
        int doctorId,
        DoctorAppointmentFilterDto filter
    );

    /// <summary>
    /// Get a specific appointment by ID (only if it belongs to the doctor)
    /// </summary>
    Task<Result<AppointmentResponseDto>> GetDoctorAppointmentByIdAsync(
        int doctorId,
        int appointmentId
    );

    /// <summary>
    /// Confirm an appointment
    /// </summary>
    Task<Result<AppointmentResponseDto>> ConfirmAppointmentAsync(int doctorId, int appointmentId);
}

public class DoctorAppointmentService : IDoctorAppointmentService
{
    private readonly IvyContext _context;

    public DoctorAppointmentService(IvyContext context)
    {
        _context = context;
    }

    public async Task<Result<PaginatedResult<AppointmentResponseDto>>> GetDoctorAppointmentsAsync(
        int doctorId,
        DoctorAppointmentFilterDto filter
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
                .Where(a => a.DoctorId == doctorId)
                .AsQueryable();

            // Apply filters
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
                DoctorAppointmentServiceMessageCodes.APPOINTMENTS_RETRIEVED_SUCCESS,
                paginatedResult
            );
        }
        catch (Exception)
        {
            var emptyResult = new PaginatedResult<AppointmentResponseDto>();
            return Result<PaginatedResult<AppointmentResponseDto>>.Error(
                DoctorAppointmentServiceMessageCodes.APPOINTMENTS_RETRIEVAL_FAILED,
                emptyResult
            );
        }
    }

    public async Task<Result<AppointmentResponseDto>> GetDoctorAppointmentByIdAsync(
        int doctorId,
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
            .FirstOrDefaultAsync(a => a.Id == appointmentId && a.DoctorId == doctorId);

        if (appointment == null)
        {
            return Result<AppointmentResponseDto>.Error(
                DoctorAppointmentServiceMessageCodes.APPOINTMENT_NOT_FOUND,
                null!
            );
        }

        var response = MapToResponseDto(appointment);

        return Result<AppointmentResponseDto>.Ok(
            DoctorAppointmentServiceMessageCodes.SUCCESS,
            response
        );
    }

    public async Task<Result<AppointmentResponseDto>> ConfirmAppointmentAsync(
        int doctorId,
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
            .FirstOrDefaultAsync(a => a.Id == appointmentId && a.DoctorId == doctorId);

        if (appointment == null)
        {
            return Result<AppointmentResponseDto>.Error(
                DoctorAppointmentServiceMessageCodes.APPOINTMENT_NOT_FOUND,
                null!
            );
        }

        if (appointment.Status == AppointmentStatus.Cancelled)
        {
            return Result<AppointmentResponseDto>.Error(
                DoctorAppointmentServiceMessageCodes.APPOINTMENT_ALREADY_CANCELLED,
                null!
            );
        }

        if (appointment.Status == AppointmentStatus.Completed)
        {
            return Result<AppointmentResponseDto>.Error(
                DoctorAppointmentServiceMessageCodes.APPOINTMENT_ALREADY_COMPLETED,
                null!
            );
        }

        if (appointment.Status == AppointmentStatus.Confirmed)
        {
            return Result<AppointmentResponseDto>.Error(
                DoctorAppointmentServiceMessageCodes.APPOINTMENT_ALREADY_CONFIRMED,
                null!
            );
        }

        appointment.Status = AppointmentStatus.Confirmed;
        await _context.SaveChangesAsync();

        var response = MapToResponseDto(appointment);

        return Result<AppointmentResponseDto>.Ok(
            DoctorAppointmentServiceMessageCodes.APPOINTMENT_CONFIRMED_SUCCESS,
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

public static class DoctorAppointmentServiceMessageCodes
{
    public const string SUCCESS = "DOCTOR_APPOINTMENT_SUCCESS";
    public const string APPOINTMENTS_RETRIEVED_SUCCESS = "DOCTOR_APPOINTMENTS_RETRIEVED_SUCCESS";
    public const string APPOINTMENT_CONFIRMED_SUCCESS = "DOCTOR_APPOINTMENT_CONFIRMED_SUCCESS";

    // Error codes
    public const string APPOINTMENTS_RETRIEVAL_FAILED = "DOCTOR_APPOINTMENTS_RETRIEVAL_FAILED";
    public const string APPOINTMENT_NOT_FOUND = "DOCTOR_APPOINTMENT_NOT_FOUND";
    public const string APPOINTMENT_ALREADY_CANCELLED = "DOCTOR_APPOINTMENT_ALREADY_CANCELLED";
    public const string APPOINTMENT_ALREADY_COMPLETED = "DOCTOR_APPOINTMENT_ALREADY_COMPLETED";
    public const string APPOINTMENT_ALREADY_CONFIRMED = "DOCTOR_APPOINTMENT_ALREADY_CONFIRMED";
}
