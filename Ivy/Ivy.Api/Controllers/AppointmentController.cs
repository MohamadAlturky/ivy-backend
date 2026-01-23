using Ivy.Contracts.Models;
using Ivy.Contracts.Services;
using Microsoft.AspNetCore.Mvc;

namespace Ivy.Api.Controllers;

[Route("api/appointments")]
public class AppointmentController : BaseController
{
    private readonly IAppointmentService _appointmentService;

    public AppointmentController(
        IAppointmentService appointmentService,
        IApiResponseRepresenter responseRepresenter,
        ILogger<AppointmentController> logger
    )
        : base(responseRepresenter, logger)
    {
        _appointmentService = appointmentService;
    }

    /// <summary>
    /// Get paginated list of appointments with filters
    /// </summary>
    /// <param name="filter">Filter parameters including pagination, status, dates, doctor, patient, clinic, and search term</param>
    /// <returns>Paginated list of appointments</returns>
    [HttpGet]
    public async Task<IActionResult> GetAllAppointments([FromQuery] AppointmentFilterDto filter)
    {
        var result = await _appointmentService.GetAllAppointmentsAsync(filter);
        return HandleResult(result);
    }

    /// <summary>
    /// Book a new appointment
    /// </summary>
    /// <param name="dto">Appointment booking details</param>
    /// <returns>Created appointment details</returns>
    [HttpPost("book")]
    public async Task<IActionResult> BookAppointment([FromBody] CreateAppointmentDto dto)
    {
        if (!IsModelValid())
        {
            return HandleValidationError();
        }

        var result = await _appointmentService.BookAppointmentAsync(dto);
        return HandleResult(result);
    }

    /// <summary>
    /// Cancel an appointment (only if not within specified minutes before start)
    /// </summary>
    /// <param name="id">Appointment ID</param>
    /// <param name="minutesBeforeStart">Minimum minutes before appointment start time (default: 60)</param>
    /// <returns>Updated appointment details</returns>
    [HttpPut("{id}/cancel")]
    public async Task<IActionResult> CancelAppointment(
        int id,
        [FromQuery] int minutesBeforeStart = 60
    )
    {
        var result = await _appointmentService.CancelAppointmentAsync(id, minutesBeforeStart);
        return HandleResult(result);
    }

    /// <summary>
    /// Reschedule an appointment (only if not within specified minutes before start)
    /// </summary>
    /// <param name="id">Appointment ID</param>
    /// <param name="dto">Reschedule details with new start time</param>
    /// <param name="minutesBeforeStart">Minimum minutes before appointment start time (default: 60)</param>
    /// <returns>Updated appointment details</returns>
    [HttpPut("{id}/reschedule")]
    public async Task<IActionResult> RescheduleAppointment(
        int id,
        [FromBody] RescheduleAppointmentDto dto,
        [FromQuery] int minutesBeforeStart = 60
    )
    {
        if (!IsModelValid())
        {
            return HandleValidationError();
        }

        var result = await _appointmentService.RescheduleAppointmentAsync(
            id,
            dto,
            minutesBeforeStart
        );
        return HandleResult(result);
    }

    /// <summary>
    /// Mark an appointment as In Progress
    /// </summary>
    /// <param name="id">Appointment ID</param>
    /// <returns>Updated appointment details</returns>
    [HttpPut("{id}/mark-inprogress")]
    public async Task<IActionResult> MarkAppointmentInProgress(int id)
    {
        var result = await _appointmentService.MarkAppointmentInProgressAsync(id);
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
        var result = await _appointmentService.ConfirmAppointmentAsync(id);
        return HandleResult(result);
    }

    /// <summary>
    /// Get appointment by ID
    /// </summary>
    /// <param name="id">Appointment ID</param>
    /// <returns>Appointment details</returns>
    [HttpGet("{id}")]
    public async Task<IActionResult> GetAppointment(int id)
    {
        var result = await _appointmentService.GetAppointmentByIdAsync(id);
        return HandleResult(result);
    }
}

