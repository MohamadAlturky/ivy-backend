using Ivy.Contracts.Models;
using Ivy.Contracts.Services;
using Ivy.Core.DataContext;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Ivy.Api.Controllers;

/// <summary>
/// Clinic staff appointment management endpoints
/// </summary>
[Route("api/clinic/appointments")]
public class ClinicAppointmentController : BaseController
{
    private readonly IClinicAppointmentService _clinicAppointmentService;
    private readonly IvyContext _context;

    public ClinicAppointmentController(
        IClinicAppointmentService clinicAppointmentService,
        IvyContext context,
        IApiResponseRepresenter responseRepresenter,
        ILogger<ClinicAppointmentController> logger
    )
        : base(responseRepresenter, logger)
    {
        _clinicAppointmentService = clinicAppointmentService;
        _context = context;
    }

    /// <summary>
    /// Get all appointments for the authenticated admin's clinic
    /// </summary>
    /// <param name="filter">Filter parameters including pagination, status, dates, and search</param>
    /// <returns>Paginated list of clinic appointments</returns>
    [HttpGet]
    public async Task<IActionResult> GetClinicAppointments(
        [FromQuery] ClinicAppointmentFilterDto filter
    )
    {
        var clinicId = await GetClinicId(_context);
        if (clinicId == null)
            return Unauthorized();

        var result = await _clinicAppointmentService.GetClinicAppointmentsAsync(clinicId.Value, filter);
        return HandleResult(result);
    }

    /// <summary>
    /// Get a specific appointment by ID for the authenticated admin's clinic
    /// </summary>
    /// <param name="appointmentId">Appointment ID</param>
    /// <returns>Appointment details</returns>
    [HttpGet("appointment/{appointmentId}")]
    public async Task<IActionResult> GetAppointment(int appointmentId)
    {
        var clinicId = await GetClinicId(_context);
        if (clinicId == null)
            return Unauthorized();

        var result = await _clinicAppointmentService.GetClinicAppointmentByIdAsync(clinicId.Value, appointmentId);
        return HandleResult(result);
    }

    /// <summary>
    /// Confirm an appointment
    /// </summary>
    /// <param name="appointmentId">Appointment ID</param>
    /// <returns>Updated appointment details</returns>
    [HttpPut("appointment/{appointmentId}/confirm")]
    public async Task<IActionResult> ConfirmAppointment(int appointmentId)
    {
        var clinicId = await GetClinicId(_context);
        if (clinicId == null)
            return Unauthorized();

        var result = await _clinicAppointmentService.ConfirmAppointmentAsync(clinicId.Value, appointmentId);
        return HandleResult(result);
    }

    /// <summary>
    /// Mark an appointment as In Progress
    /// </summary>
    /// <param name="appointmentId">Appointment ID</param>
    /// <returns>Updated appointment details</returns>
    [HttpPut("appointment/{appointmentId}/mark-inprogress")]
    public async Task<IActionResult> MarkAppointmentInProgress(int appointmentId)
    {
        var clinicId = await GetClinicId(_context);
        if (clinicId == null)
            return Unauthorized();

        var result = await _clinicAppointmentService.MarkAppointmentInProgressAsync(clinicId.Value, appointmentId);
        return HandleResult(result);
    }

    /// <summary>
    /// Mark an appointment as Completed
    /// </summary>
    /// <param name="appointmentId">Appointment ID</param>
    /// <returns>Updated appointment details</returns>
    [HttpPut("appointment/{appointmentId}/complete")]
    public async Task<IActionResult> CompleteAppointment(int appointmentId)
    {
        var clinicId = await GetClinicId(_context);
        if (clinicId == null)
            return Unauthorized();

        var result = await _clinicAppointmentService.CompleteAppointmentAsync(clinicId.Value, appointmentId);
        return HandleResult(result);
    }

    /// <summary>
    /// Cancel an appointment from clinic side
    /// </summary>
    /// <param name="appointmentId">Appointment ID</param>
    /// <param name="reason">Cancellation reason</param>
    /// <returns>Updated appointment details</returns>
    [HttpPut("appointment/{appointmentId}/cancel")]
    public async Task<IActionResult> CancelAppointment(
        int appointmentId,
        [FromQuery] string? reason = null
    )
    {
        var clinicId = await GetClinicId(_context);
        if (clinicId == null)
            return Unauthorized();

        var result = await _clinicAppointmentService.CancelAppointmentAsync(clinicId.Value, appointmentId, reason);
        return HandleResult(result);
    }

    /// <summary>
    /// Reschedule an appointment from clinic side
    /// </summary>
    /// <param name="appointmentId">Appointment ID</param>
    /// <param name="dto">Reschedule details with new start time</param>
    /// <returns>Updated appointment details</returns>
    [HttpPut("appointment/{appointmentId}/reschedule")]
    public async Task<IActionResult> RescheduleAppointment(
        int appointmentId,
        [FromBody] RescheduleAppointmentDto dto
    )
    {
        if (!IsModelValid())
        {
            return HandleValidationError();
        }

        var clinicId = await GetClinicId(_context);
        if (clinicId == null)
            return Unauthorized();

        var result = await _clinicAppointmentService.RescheduleAppointmentAsync(clinicId.Value, appointmentId, dto);
        return HandleResult(result);
    }
}

