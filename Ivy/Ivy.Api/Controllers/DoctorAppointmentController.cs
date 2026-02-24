using Ivy.Contracts.Models;
using Ivy.Contracts.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Ivy.Api.Controllers;

/// <summary>
/// Doctor-facing appointment management endpoints
/// </summary>
[Route("api/doctor/appointments")]
[Authorize]
public class DoctorAppointmentController : BaseController
{
    private readonly IDoctorAppointmentService _doctorAppointmentService;

    public DoctorAppointmentController(
        IDoctorAppointmentService doctorAppointmentService,
        IApiResponseRepresenter responseRepresenter,
        ILogger<DoctorAppointmentController> logger
    )
        : base(responseRepresenter, logger)
    {
        _doctorAppointmentService = doctorAppointmentService;
    }

    /// <summary>
    /// Get all appointments for the current logged-in doctor
    /// </summary>
    /// <param name="filter">Filter parameters including pagination, status, and dates</param>
    /// <returns>Paginated list of doctor's appointments</returns>
    [HttpGet("my-appointments")]
    public async Task<IActionResult> GetMyAppointments(
        [FromQuery] DoctorAppointmentFilterDto filter
    )
    {
        var doctorId = GetUserId();
        if (doctorId == 0)
        {
            return Unauthorized();
        }

        var result = await _doctorAppointmentService.GetDoctorAppointmentsAsync(doctorId, filter);
        return HandleResult(result);
    }

    /// <summary>
    /// Get a specific appointment by ID (only if it belongs to the current doctor)
    /// </summary>
    /// <param name="id">Appointment ID</param>
    /// <returns>Appointment details</returns>
    [HttpGet("{id}")]
    public async Task<IActionResult> GetAppointment(int id)
    {
        var doctorId = GetUserId();
        if (doctorId == 0)
        {
            return Unauthorized();
        }

        var result = await _doctorAppointmentService.GetDoctorAppointmentByIdAsync(doctorId, id);
        return HandleResult(result);
    }

    /// <summary>
    /// Confirm an appointment
    /// </summary>
    /// <param name="id">Appointment ID</param>
    /// <returns>Updated appointment details</returns>
    [HttpPut("{id}/confirm")]
    public async Task<IActionResult> ConfirmAppointment(int id)
    {
        var doctorId = GetUserId();
        if (doctorId == 0)
        {
            return Unauthorized();
        }

        var result = await _doctorAppointmentService.ConfirmAppointmentAsync(doctorId, id);
        return HandleResult(result);
    }

    /// <summary>
    /// Cancel an appointment
    /// </summary>
    /// <param name="id">Appointment ID</param>
    /// <returns>Updated appointment details</returns>
    [HttpPut("{id}/cancel")]
    public async Task<IActionResult> CancelAppointment(int id)
    {
        var doctorId = GetUserId();
        if (doctorId == 0)
        {
            return Unauthorized();
        }

        var result = await _doctorAppointmentService.CancelAppointmentAsync(doctorId, id);
        return HandleResult(result);
    }

    /// <summary>
    /// Set an appointment as in progress
    /// </summary>
    /// <param name="id">Appointment ID</param>
    /// <returns>Updated appointment details</returns>
    [HttpPut("{id}/in-progress")]
    public async Task<IActionResult> SetAppointmentInProgress(int id)
    {
        var doctorId = GetUserId();
        if (doctorId == 0)
        {
            return Unauthorized();
        }

        var result = await _doctorAppointmentService.SetAppointmentInProgressAsync(doctorId, id);
        return HandleResult(result);
    }

    /// <summary>
    /// Mark an appointment as completed
    /// </summary>
    /// <param name="id">Appointment ID</param>
    /// <returns>Updated appointment details</returns>
    [HttpPut("{id}/complete")]
    public async Task<IActionResult> CompleteAppointment(int id)
    {
        var doctorId = GetUserId();
        if (doctorId == 0)
        {
            return Unauthorized();
        }

        var result = await _doctorAppointmentService.CompleteAppointmentAsync(doctorId, id);
        return HandleResult(result);
    }

    /// <summary>
    /// Set the doctor's feedback on the appointment schedule
    /// </summary>
    /// <param name="id">Appointment ID</param>
    /// <param name="dto">Feedback content (DoctorFeedbackOnSchedule can be null or empty to clear)</param>
    /// <returns>Updated appointment details</returns>
    [HttpPut("{id}/feedback")]
    public async Task<IActionResult> SetDoctorFeedbackOnSchedule(
        int id,
        [FromBody] DoctorFeedbackOnScheduleDto dto
    )
    {
        var doctorId = GetUserId();
        if (doctorId == 0)
        {
            return Unauthorized();
        }

        var result = await _doctorAppointmentService.SetDoctorFeedbackOnScheduleAsync(
            doctorId,
            id,
            dto?.DoctorFeedbackOnSchedule
        );
        return HandleResult(result);
    }
}
