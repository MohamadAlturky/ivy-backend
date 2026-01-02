using Ivy.Contracts.Models;
using Ivy.Contracts.Services;
using Microsoft.AspNetCore.Mvc;

namespace Ivy.Api.Controllers;

[Route("api/doctors")]
public class DoctorController : BaseController
{
    private readonly IDoctorService _doctorService;

    public DoctorController(
        IDoctorService doctorService,
        IApiResponseRepresenter responseRepresenter,
        ILogger<DoctorController> logger
    )
        : base(responseRepresenter, logger)
    {
        _doctorService = doctorService;
    }

    [HttpGet]
    public async Task<IActionResult> GetAllDoctors(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 10,
        [FromQuery] string? name = null,
        [FromQuery] int? specialityId = null
    )
    {
        var result = await _doctorService.GetAllAsync(
            page: page,
            pageSize: pageSize,
            name: name,
            specialityId: specialityId
        );
        return HandleResult(result);
    }

    [HttpGet("localized")]
    public async Task<IActionResult> GetAllLocalizedDoctors(
        [FromQuery] string language = "en",
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 10,
        [FromQuery] string? name = null,
        [FromQuery] int? specialityId = null
    )
    {
        var result = await _doctorService.GetAllLocalizedAsync(
            language: language,
            page: page,
            pageSize: pageSize,
            name: name,
            specialityId: specialityId
        );
        return HandleResult(result);
    }

    [HttpPost]
    public async Task<IActionResult> CreateDoctor(
        [FromBody] CreateDoctorDto createDoctorDto
    )
    {
        var result = await _doctorService.CreateAsync(createDoctorDto);
        return HandleResult(result);
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateDoctor(
        int id,
        [FromBody] UpdateDoctorDto updateDoctorDto
    )
    {
        var result = await _doctorService.UpdateAsync(id, updateDoctorDto);
        return HandleResult(result);
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteDoctor(int id)
    {
        var result = await _doctorService.DeleteAsync(id);
        return HandleResult(result);
    }
}