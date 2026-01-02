using Ivy.Contracts.Models;
using Ivy.Contracts.Services;
using Microsoft.AspNetCore.Mvc;

namespace Ivy.Api.Controllers;

[Route("api/doctor-schedules")]
public class DoctorScheduleController : BaseController
{
    private readonly IDoctorScheduleService _doctorScheduleService;

    public DoctorScheduleController(
        IDoctorScheduleService doctorScheduleService,
        IApiResponseRepresenter responseRepresenter,
        ILogger<DoctorScheduleController> logger
    )
        : base(responseRepresenter, logger)
    {
        _doctorScheduleService = doctorScheduleService;
    }

    [HttpGet("working-hours")]
    public async Task<IActionResult> GetWorkingHours(
        [FromQuery] int? doctorId = null,
        [FromQuery] int? clinicId = null,
        [FromQuery] string? searchKey = null,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 10
    )
    {
        var result = await _doctorScheduleService.GetWorkingHoursAsync(
            doctorId: doctorId,
            clinicId: clinicId,
            language: GetLanguage(),
            searchKey: searchKey,
            page: page,
            pageSize: pageSize
        );

        return HandleResult(result);
    }

    [HttpPost("working-hours")]
    public async Task<IActionResult> AddWorkingHours(
        [FromBody] AddWorkingHoursRequest request
    )
    {
        var result = await _doctorScheduleService.AddWorkingHoursAsync(
            request.DoctorId,
            request.ClinicId,
            request.TimeRange
        );

        return HandleResult(result);
    }

    [HttpPut("working-hours/{id}")]
    public async Task<IActionResult> UpdateWorkingHours(
        int id,
        [FromBody] TimeRangeDto timeRange
    )
    {
        var result = await _doctorScheduleService.UpdateWorkingHoursAsync(id, timeRange);
        return HandleResult(result);
    }

    [HttpDelete("working-hours/{id}")]
    public async Task<IActionResult> RemoveWorkingHours(int id)
    {
        var result = await _doctorScheduleService.RemoveWorkingHoursAsync(id);
        return HandleResult(result);
    }
}

// Request DTO for the POST action
public class AddWorkingHoursRequest
{
    public int DoctorId { get; set; }
    public int ClinicId { get; set; }
    public TimeRangeDto TimeRange { get; set; } = null!;
}