using Ivy.Contracts.Models;
using Ivy.Contracts.Services;
using Microsoft.AspNetCore.Mvc;

namespace Ivy.Api.Controllers;

[Route("api/client-data-collection-for-booking")]
public class ClientDataCollectionForBookingController : BaseController
{
    private readonly IClientDataCollectionForBookingService _clientDataCollectionForBookingService;

    public ClientDataCollectionForBookingController(
        IClientDataCollectionForBookingService clientDataCollectionForBookingService,
        IApiResponseRepresenter responseRepresenter,
        ILogger<ClientDataCollectionForBookingController> logger
    )
        : base(responseRepresenter, logger)
    {
        _clientDataCollectionForBookingService = clientDataCollectionForBookingService;
    }

    // Clinic Endpoints

    [HttpGet("clinics")]
    public async Task<IActionResult> GetClinicsLocalized(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 10,
        [FromQuery] string? name = null,
        [FromQuery] int? cityId = null,
        [FromQuery] int? governorateId = null,
        [FromQuery] int? doctorId = null,
        [FromQuery] bool? isActive = null
    )
    {
        var result = await _clientDataCollectionForBookingService.GetClinicsLocalizedAsync(
            language: GetLanguage(),
            page: page,
            pageSize: pageSize,
            name: name,
            cityId: cityId,
            isActive: isActive,
            governorateId: governorateId,
            doctorId: doctorId
        );
        return HandleResult(result);
    }

    // Doctor Endpoints

    [HttpGet("doctors")]
    public async Task<IActionResult> GetDoctorsLocalized(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 10,
        [FromQuery] string? name = null,
        [FromQuery] int? clinicId = null,
        [FromQuery] int? specialtyId = null,
        [FromQuery] bool? isActive = null
    )
    {
        var result = await _clientDataCollectionForBookingService.GetDoctorsLocalizedAsync(
            language: GetLanguage(),
            page: page,
            pageSize: pageSize,
            name: name,
            clinicId: clinicId,
            specialty: specialtyId,
            isActive: isActive
        );
        return HandleResult(result);
    }

    // Working Days Endpoints

    [HttpGet("working-days")]
    public async Task<IActionResult> GetWorkingDays(
        [FromQuery] int doctorId,
        [FromQuery] int clinicId
    )
    {
        var result = await _clientDataCollectionForBookingService.GetWorkingDaysAsync(
            doctorId: doctorId,
            clinicId: clinicId
        );
        return HandleResult(result);
    }

    [HttpGet("working-times-by-date")]
    public async Task<IActionResult> GetWorkingDaysByDate(
        [FromQuery] int doctorId,
        [FromQuery] int clinicId,
        [FromQuery] DateTime date,
        [FromQuery] int segmentDurationMinutes = 30,
        [FromQuery] int stepInMinutes = 15
    )
    {
        var result = await _clientDataCollectionForBookingService.GetWorkingDaysByDateAsync(
            doctorId: doctorId,
            clinicId: clinicId,
            date: date,
            segmentDurationMinutes: segmentDurationMinutes,
            stepInMinutes: stepInMinutes
        );
        return HandleResult(result);
    }
}

