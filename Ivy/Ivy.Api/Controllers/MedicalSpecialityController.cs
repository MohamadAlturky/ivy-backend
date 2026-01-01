using Ivy.Contracts.Models;
using Ivy.Contracts.Services;
using Microsoft.AspNetCore.Mvc;

namespace Ivy.Api.Controllers;

[Route("api/medical-specialities")]
public class MedicalSpecialityController : BaseController
{
    private readonly IMedicalSpecialityService _medicalSpecialityService;

    public MedicalSpecialityController(
        IMedicalSpecialityService medicalSpecialityService,
        IApiResponseRepresenter responseRepresenter,
        ILogger<MedicalSpecialityController> logger
    )
        : base(responseRepresenter, logger)
    {
        _medicalSpecialityService = medicalSpecialityService;
    }

    [HttpGet]
    public async Task<IActionResult> GetAllMedicalSpecialities(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 10,
        [FromQuery] string? name = null,
        [FromQuery] bool? isActive = null
    )
    {
        var result = await _medicalSpecialityService.GetAllAsync(
            page: page,
            pageSize: pageSize,
            name: name,
            isActive: isActive
        );
        return HandleResult(result);
    }

    [HttpGet("localized")]
    public async Task<IActionResult> GetAllLocalizedMedicalSpecialities(
        [FromQuery] string language = "en",
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 10,
        [FromQuery] string? name = null,
        [FromQuery] bool? isActive = null
    )
    {
        var result = await _medicalSpecialityService.GetAllLocalizedAsync(
            language: language,
            page: page,
            pageSize: pageSize,
            name: name,
            isActive: isActive
        );
        return HandleResult(result);
    }

    [HttpPost]
    public async Task<IActionResult> CreateMedicalSpeciality(
        [FromBody] CreateMedicalSpecialityDto createMedicalSpecialityDto
    )
    {
        var result = await _medicalSpecialityService.CreateAsync(createMedicalSpecialityDto);
        return HandleResult(result);
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateMedicalSpeciality(
        int id,
        [FromBody] UpdateMedicalSpecialityDto updateMedicalSpecialityDto
    )
    {
        var result = await _medicalSpecialityService.UpdateAsync(id, updateMedicalSpecialityDto);
        return HandleResult(result);
    }

    [HttpPut("{id}/toggle-status")]
    public async Task<IActionResult> ToggleStatus(int id)
    {
        var result = await _medicalSpecialityService.ToggleStatusAsync(id);
        return HandleResult(result);
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteMedicalSpeciality(int id)
    {
        var result = await _medicalSpecialityService.DeleteAsync(id);
        return HandleResult(result);
    }
    [HttpGet("dropdown")]
    public async Task<IActionResult> GetDropdownMedicalSpecialities(
        [FromQuery] string? name = null
    )
    {
        var result = await _medicalSpecialityService.DropDownAsync(GetLanguage(), name: name);
        return HandleResult(result);
    }
}