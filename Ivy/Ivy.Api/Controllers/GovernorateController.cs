using Ivy.Contracts.Models;
using Ivy.Contracts.Services;
using Microsoft.AspNetCore.Mvc;

namespace Ivy.Api.Controllers;

[Route("api/governorates")]
public class GovernorateController : BaseController
{
    private readonly IGovernorateService _governorateService;

    public GovernorateController(
        IGovernorateService governorateService,
        IApiResponseRepresenter responseRepresenter,
        ILogger<GovernorateController> logger
    )
        : base(responseRepresenter, logger)
    {
        _governorateService = governorateService;
    }

    [HttpGet]
    public async Task<IActionResult> GetAllGovernorates(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 10,
        [FromQuery] string? name = null,
        [FromQuery] bool? isActive = null
    )
    {
        var result = await _governorateService.GetAllAsync(
            page: page,
            pageSize: pageSize,
            name: name,
            isActive: isActive
        );
        return HandleResult(result);
    }

    [HttpGet("localized")]
    public async Task<IActionResult> GetAllLocalizedGovernorates(
        [FromQuery] string language = "en",
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 10,
        [FromQuery] string? name = null,
        [FromQuery] bool? isActive = null
    )
    {
        var result = await _governorateService.GetAllLocalizedAsync(
            language: language,
            page: page,
            pageSize: pageSize,
            name: name,
            isActive: isActive
        );
        return HandleResult(result);
    }

    [HttpPost]
    public async Task<IActionResult> CreateGovernorate(
        [FromBody] CreateGovernorateDto createGovernorateDto
    )
    {
        var result = await _governorateService.CreateAsync(createGovernorateDto);
        return HandleResult(result);
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateGovernorate(
        int id,
        [FromBody] UpdateGovernorateDto updateGovernorateDto
    )
    {
        var result = await _governorateService.UpdateAsync(id, updateGovernorateDto);
        return HandleResult(result);
    }

    [HttpPut("{id}/toggle-status")]
    public async Task<IActionResult> ToggleStatus(int id)
    {
        var result = await _governorateService.ToggleStatusAsync(id);
        return HandleResult(result);
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteGovernorate(int id)
    {
        var result = await _governorateService.DeleteAsync(id);
        return HandleResult(result);
    }
    [HttpGet("dropdown")]
    public async Task<IActionResult> GetDropdownGovernorates(
        [FromQuery] string? name = null
    )
    {
        var result = await _governorateService.DropDownAsync(GetLanguage(), name: name);
        return HandleResult(result);
    }
}