using Ivy.Contracts.Models;
using Ivy.Contracts.Services;
using Microsoft.AspNetCore.Mvc;

namespace Ivy.Api.Controllers;

[Route("api/doctor-clinics")]
public class DoctorClinicController : BaseController
{
    private readonly IDoctorClinicService _doctorClinicService;

    public DoctorClinicController(
        IDoctorClinicService doctorClinicService,
        IApiResponseRepresenter responseRepresenter,
        ILogger<DoctorClinicController> logger
    )
        : base(responseRepresenter, logger)
    {
        _doctorClinicService = doctorClinicService;
    }

    [HttpGet("clinics/{clinicId}/doctors")]
    public async Task<IActionResult> GetDoctorsInClinic(
        int clinicId,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 10,
        [FromQuery] string? doctorName = null
    )
    {
        var result = await _doctorClinicService.GetDoctorsInClinicAsync(
            clinicId,
            page,
            pageSize,
            doctorName
        );
        return HandleResult(result);
    }

    [HttpGet("clinics/{clinicId}/doctors/localized")]
    public async Task<IActionResult> GetDoctorsInClinicLocalized(
        int clinicId,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 10,
        [FromQuery] string? doctorName = null
    )
    {
        var result = await _doctorClinicService.GetDoctorsInClinicLocalizedAsync(
            clinicId,
            GetLanguage(),
            page,
            pageSize,
            doctorName
        );
        return HandleResult(result);
    }

    [HttpPost]
    public async Task<IActionResult> AddDoctorToClinic(
        [FromBody] AddDoctorToClinicDto dto
    )
    {
        var result = await _doctorClinicService.AddDoctorToClinicAsync(dto);
        return HandleResult(result);
    }

    [HttpDelete("clinics/{clinicId}/doctors/{doctorId}")]
    public async Task<IActionResult> RemoveDoctorFromClinic(
        int clinicId,
        int doctorId
    )
    {
        var result = await _doctorClinicService.RemoveDoctorFromClinicAsync(doctorId, clinicId);
        return HandleResult(result);
    }
}