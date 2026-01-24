using Ivy.Contracts.Models;
using Ivy.Contracts.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Ivy.Api.Controllers;

/// <summary>
/// Customer/Patient-facing appointment management endpoints
/// </summary>
[Route("api/customer/appointments")]
[Authorize]
public class CustomerAppointmentController : BaseController
{
    private readonly ICustomerAppointmentService _customerAppointmentService;

    public CustomerAppointmentController(
        ICustomerAppointmentService customerAppointmentService,
        IApiResponseRepresenter responseRepresenter,
        ILogger<CustomerAppointmentController> logger
    )
        : base(responseRepresenter, logger)
    {
        _customerAppointmentService = customerAppointmentService;
    }

    /// <summary>
    /// Get all appointments for the current logged-in patient
    /// </summary>
    /// <param name="filter">Filter parameters including pagination, status, and dates</param>
    /// <returns>Paginated list of patient's appointments</returns>
    [HttpGet("my-appointments")]
    public async Task<IActionResult> GetMyAppointments([FromQuery] CustomerAppointmentFilterDto filter)
    {
        var patientId = GetUserId();
        if (patientId == 0)
        {
            return Unauthorized();
        }

        var result = await _customerAppointmentService.GetPatientAppointmentsAsync(patientId, filter);
        return HandleResult(result);
    }

    /// <summary>
    /// Get a specific appointment by ID (only if it belongs to the current patient)
    /// </summary>
    /// <param name="id">Appointment ID</param>
    /// <returns>Appointment details</returns>
    [HttpGet("{id}")]
    public async Task<IActionResult> GetAppointment(int id)
    {
        var patientId = GetUserId();
        if (patientId == 0)
        {
            return Unauthorized();
        }

        var result = await _customerAppointmentService.GetPatientAppointmentByIdAsync(patientId, id);
        return HandleResult(result);
    }

    /// <summary>
    /// Book a new appointment for the current patient
    /// </summary>
    /// <param name="dto">Appointment booking details</param>
    /// <returns>Created appointment details</returns>
    [HttpPost("book")]
    public async Task<IActionResult> BookAppointment([FromBody] CustomerCreateAppointmentDto dto)
    {
        if (!IsModelValid())
        {
            return HandleValidationError();
        }

        var patientId = GetUserId();
        if (patientId == 0)
        {
            return Unauthorized();
        }

        var result = await _customerAppointmentService.BookAppointmentAsync(patientId, dto);
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
        var patientId = GetUserId();
        if (patientId == 0)
        {
            return Unauthorized();
        }

        var result = await _customerAppointmentService.CancelAppointmentAsync(
            patientId,
            id,
            minutesBeforeStart
        );
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

        var patientId = GetUserId();
        if (patientId == 0)
        {
            return Unauthorized();
        }

        var result = await _customerAppointmentService.RescheduleAppointmentAsync(
            patientId,
            id,
            dto,
            minutesBeforeStart
        );
        return HandleResult(result);
    }
}

