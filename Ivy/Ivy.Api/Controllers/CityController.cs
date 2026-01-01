using Ivy.Contracts.Models;
using Ivy.Contracts.Services;

namespace Ivy.Api.Controllers;

[Route("api/cities")]
public class CityController : BaseController
{
    private readonly ICityService _cityService;

    public CityController(
        ICityService cityService,
        IApiResponseRepresenter responseRepresenter,
        ILogger<CityController> logger
    )
        : base(responseRepresenter, logger)
    {
        _cityService = cityService;
    }

    [HttpGet]
    public async Task<IActionResult> GetAllCities(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 10,
        [FromQuery] string? name = null,
        [FromQuery] int? governorateId = null,
        [FromQuery] bool? isActive = null
    )
    {
        var result = await _cityService.GetAllAsync(
                page:page,
                pageSize:pageSize,
                name:name,
                governorateId:governorateId,
                isActive:isActive
            );
            return HandleResult(result);
    }
    [HttpGet("localized")]
    public async Task<IActionResult> GetAllLocalizedCities(
        [FromQuery] string language = "en",
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 10,
        [FromQuery] string? name = null,
        [FromQuery] int? governorateId = null,
        [FromQuery] bool? isActive = null
    )
    {
        var result = await _cityService.GetAllLocalizedAsync(language:language, page:page, pageSize:pageSize, name:name, governorateId:governorateId, isActive:isActive);
        return HandleResult(result);
    }

    [HttpPost]
    public async Task<IActionResult> CreateCity(
        [FromBody] CreateCityDto createCityDto
    )
    {
        var result = await _cityService.CreateAsync(createCityDto);
        return HandleResult(result);
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateCity(
        int id,
        [FromBody] UpdateCityDto updateCityDto
    )
    {
        var result = await _cityService.UpdateAsync(id, updateCityDto);
        return HandleResult(result);
    }

    [HttpPut("{id}/toggle-status")]
    public async Task<IActionResult> ToggleStatus(int id)
    {
        var result = await _cityService.ToggleStatusAsync(id);
        return HandleResult(result);
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteCity(int id)
    {
        var result = await _cityService.DeleteAsync(id);
        return HandleResult(result);
    }
}
