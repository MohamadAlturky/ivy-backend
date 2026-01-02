using Ivy.Contracts.Models;
using Ivy.Contracts.Services;
using Microsoft.AspNetCore.Mvc;

namespace Ivy.Api.Controllers;

[Route("api/clinics")]
public class ClinicController : BaseController
{
    private readonly IClinicService _clinicService;

    public ClinicController(
        IClinicService clinicService,
        IApiResponseRepresenter responseRepresenter,
        ILogger<ClinicController> logger
    )
        : base(responseRepresenter, logger)
    {
        _clinicService = clinicService;
    }

    [HttpGet]
    public async Task<IActionResult> GetAllClinics(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 10,
        [FromQuery] string? name = null,
        [FromQuery] bool? isActive = null
    )
    {
        var result = await _clinicService.GetAllAsync(
            page: page,
            pageSize: pageSize,
            name: name,
            isActive: isActive
        );
        return HandleResult(result);
    }

    [HttpGet("localized")]
    public async Task<IActionResult> GetAllLocalizedClinics(
        [FromQuery] string language = "en",
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 10,
        [FromQuery] string? name = null,
        [FromQuery] bool? isActive = null
    )
    {
        var result = await _clinicService.GetAllLocalizedAsync(
            language: language,
            page: page,
            pageSize: pageSize,
            name: name,
            isActive: isActive
        );
        return HandleResult(result);
    }

    [HttpPost]
    public async Task<IActionResult> CreateClinic(
        [FromBody] CreateClinicDto createClinicDto
    )
    {
        var result = await _clinicService.CreateAsync(createClinicDto);
        return HandleResult(result);
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateClinic(
        int id,
        [FromBody] UpdateClinicDto updateClinicDto
    )
    {
        var result = await _clinicService.UpdateAsync(id, updateClinicDto);
        return HandleResult(result);
    }

    [HttpPut("{id}/toggle-status")]
    public async Task<IActionResult> ToggleStatus(int id)
    {
        var result = await _clinicService.ToggleStatusAsync(id);
        return HandleResult(result);
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteClinic(int id)
    {
        var result = await _clinicService.DeleteAsync(id);
        return HandleResult(result);
    }

    // [HttpGet("dropdown")]
    // public async Task<IActionResult> GetDropdownClinics(
    //     [FromQuery] string? name = null
    // )
    // {
    //     var result = await _clinicService.DropDownAsync(GetLanguage(), name: name);
    //     return HandleResult(result);
    // }
}