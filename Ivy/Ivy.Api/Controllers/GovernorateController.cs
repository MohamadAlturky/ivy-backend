using System.Runtime.CompilerServices;

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

    /// <summary>
    /// Get all governorates with pagination and filtering
    /// </summary>
    [HttpGet]
    public async Task<
        ActionResult<ApiResponse<PaginatedResult<GovernorateDto>>>
    > GetAllGovernorates([FromQuery] GovernorateQueryDto query)
    {
        try
        {
            var result = await _governorateService.GetAllAsync(
                query.Page,
                query.PageSize,
                query.Name,
                query.Name,
                query.IsActive,
                query.IncludeCities
            );

            if (result.Success)
            {
                var governorateDtos = result.Data.Data.Select(g =>
                    MapToDto(g, query.IncludeCities, this.GetLanguage())
                );
                var paginatedDto = new PaginatedResult<GovernorateDto>
                {
                    Data = governorateDtos,
                    TotalCount = result.Data.TotalCount,
                    Page = result.Data.Page,
                    PageSize = result.Data.PageSize,
                    TotalPages = result.Data.TotalPages,
                    HasNextPage = result.Data.HasNextPage,
                    HasPreviousPage = result.Data.HasPreviousPage,
                };

                var mappedResult = Result<PaginatedResult<GovernorateDto>>.Ok(
                    result.MessageCode,
                    paginatedDto
                );
                return HandleResult(mappedResult);
            }

            var failedResult = Result<PaginatedResult<GovernorateDto>>.Error(
                result.MessageCode,
                default!
            );
            return HandleResult(failedResult);
        }
        catch (Exception ex)
        {
            return HandleInternalError<PaginatedResult<GovernorateDto>>(
                ex,
                "retrieving governorates"
            );
        }
    }

    /// <summary>
    /// Get a specific governorate by ID
    /// </summary>
    [HttpGet("{id}")]
    public async Task<ActionResult<ApiResponse<GovernorateDto>>> GetGovernorate(
        int id,
        [FromQuery] bool includeCities = false
    )
    {
        try
        {
            var result = await _governorateService.GetByIdAsync(id, includeCities);

            if (result.Success)
            {
                var governorateDto = MapToDto(result.Data, includeCities, this.GetLanguage());
                var mappedResult = Result<GovernorateDto>.Ok(result.MessageCode, governorateDto);
                return HandleResult(mappedResult);
            }

            var failedResult = Result<GovernorateDto>.Error(result.MessageCode, default!);
            return HandleResult(failedResult);
        }
        catch (Exception ex)
        {
            return HandleInternalError<GovernorateDto>(ex, $"retrieving governorate with ID {id}");
        }
    }

    /// <summary>
    /// Create a new governorate
    /// </summary>
    [HttpPost]
    public async Task<ActionResult<ApiResponse<GovernorateDto>>> CreateGovernorate(
        [FromBody] CreateGovernorateDto createGovernorateDto
    )
    {
        try
        {
            if (!IsModelValid())
            {
                return HandleValidationError<GovernorateDto>();
            }

            var governorate = new Governorate
            {
                NameAr = createGovernorateDto.NameAr,
                NameEn = createGovernorateDto.NameEn,
                IsActive = createGovernorateDto.IsActive,
            };

            var result = await _governorateService.CreateAsync(governorate);

            if (result.Success)
            {
                var governorateDto = MapToDto(result.Data, false, this.GetLanguage());
                var mappedResult = Result<GovernorateDto>.Ok(result.MessageCode, governorateDto);
                return HandleResult(mappedResult);
            }

            var failedResult = Result<GovernorateDto>.Error(result.MessageCode, default!);
            return HandleResult(failedResult);
        }
        catch (Exception ex)
        {
            return HandleInternalError<GovernorateDto>(ex, "creating governorate");
        }
    }

    /// <summary>
    /// Update an existing governorate
    /// </summary>
    [HttpPut("{id}")]
    public async Task<ActionResult<ApiResponse<GovernorateDto>>> UpdateGovernorate(
        int id,
        [FromBody] UpdateGovernorateDto updateGovernorateDto
    )
    {
        try
        {
            if (!IsModelValid())
            {
                return HandleValidationError<GovernorateDto>();
            }

            var governorate = new Governorate
            {
                NameAr = updateGovernorateDto.NameAr,
                NameEn = updateGovernorateDto.NameEn,
                IsActive = updateGovernorateDto.IsActive,
            };

            var result = await _governorateService.UpdateAsync(id, governorate);

            if (result.Success)
            {
                var governorateDto = MapToDto(result.Data, false, this.GetLanguage());
                var mappedResult = Result<GovernorateDto>.Ok(result.MessageCode, governorateDto);
                return HandleResult(mappedResult);
            }

            var failedResult = Result<GovernorateDto>.Error(result.MessageCode, default!);
            return HandleResult(failedResult);
        }
        catch (Exception ex)
        {
            return HandleInternalError<GovernorateDto>(ex, $"updating governorate with ID {id}");
        }
    }

    /// <summary>
    /// Delete a governorate (soft delete)
    /// </summary>
    [HttpDelete("{id}")]
    public async Task<ActionResult<ApiResponse>> DeleteGovernorate(int id)
    {
        try
        {
            var result = await _governorateService.DeleteAsync(id);
            return HandleResult(result);
        }
        catch (Exception ex)
        {
            return HandleInternalError(ex, $"deleting governorate with ID {id}");
        }
    }

    /// <summary>
    /// Check if a governorate exists by ID
    /// </summary>
    [HttpGet("{id}/exists")]
    public async Task<ActionResult<ApiResponse<bool>>> CheckGovernorateExists(int id)
    {
        try
        {
            var result = await _governorateService.ExistsAsync(id);
            return HandleResult(result);
        }
        catch (Exception ex)
        {
            return HandleInternalError<bool>(ex, $"checking if governorate exists with ID {id}");
        }
    }

    /// <summary>
    /// Get the count of cities in a governorate
    /// </summary>
    [HttpGet("{id}/cities/count")]
    public async Task<ActionResult<ApiResponse<int>>> GetCitiesCount(int id)
    {
        try
        {
            var result = await _governorateService.GetCitiesCountAsync(id);
            return HandleResult(result);
        }
        catch (Exception ex)
        {
            return HandleInternalError<int>(ex, $"getting cities count for governorate {id}");
        }
    }

    private static GovernorateDto MapToDto(Governorate governorate, bool includeCities, string language)
    {
        var dto = new GovernorateDto
        {
            Id = governorate.Id,
            Name = language == "ar" ? governorate.NameAr : governorate.NameEn,
            IsActive = governorate.IsActive,
            CreatedAt = governorate.CreatedAt,
            UpdatedAt = governorate.UpdatedAt,
        };

        if (includeCities && governorate.Cities != null)
        {
            dto.Cities = governorate
                .Cities.Select(city => new CityDto
                {
                    Id = city.Id,
                    NameAr = city.NameAr,
                    NameEn = city.NameEn,
                    GovernorateId = city.GovernorateId,
                    IsActive = city.IsActive,
                    CreatedAt = city.CreatedAt,
                    UpdatedAt = city.UpdatedAt,
                })
                .ToList();
        }

        return dto;
    }
}
