// using Ivy.Api.DTOs;
// using Ivy.Api.Services;
// using Ivy.Core.Entities;
// using Ivy.Core.Services;
// using IvyBackend;
// using Microsoft.AspNetCore.Mvc;

// namespace Ivy.Api.Controllers;

// [Route("api/cities")]
// public class CityController : BaseController
// {
//     private readonly ICityService _cityService;

//     public CityController(
//         ICityService cityService,
//         IApiResponseRepresenter responseRepresenter,
//         ILogger<CityController> logger
//     )
//         : base(responseRepresenter, logger)
//     {
//         _cityService = cityService;
//     }

//     /// <summary>
//     /// Get all cities with pagination and filtering
//     /// </summary>
//     [HttpGet]
//     public async Task<ActionResult<ApiResponse<PaginatedResult<CityDto>>>> GetAllCities(
//         [FromQuery] CityQueryDto query
//     )
//     {
//         try
//         {
//             var result = await _cityService.GetAllAsync(
//                 query.Page,
//                 query.PageSize,
//                 query.NameAr,
//                 query.NameEn,
//                 query.GovernorateId,
//                 query.IsActive
//             );

//             if (result.Success)
//             {
//                 var cityDtos = result.Data.Data.Select(c => MapToDto(c, this.GetLanguage()));
//                 var paginatedDto = new PaginatedResult<CityDto>
//                 {
//                     Data = cityDtos,
//                     TotalCount = result.Data.TotalCount,
//                     Page = result.Data.Page,
//                     PageSize = result.Data.PageSize,
//                     TotalPages = result.Data.TotalPages,
//                     HasNextPage = result.Data.HasNextPage,
//                     HasPreviousPage = result.Data.HasPreviousPage,
//                 };

//                 var mappedResult = Result<PaginatedResult<CityDto>>.Ok(
//                     result.MessageCode,
//                     paginatedDto
//                 );
//                 return HandleResult(mappedResult);
//             }

//             var failedResult = Result<PaginatedResult<CityDto>>.Error(result.MessageCode, default!);
//             return HandleResult(failedResult);
//         }
//         catch (Exception ex)
//         {
//             return HandleInternalError<PaginatedResult<CityDto>>(ex, "retrieving cities");
//         }
//     }

//     /// <summary>
//     /// Get a specific city by ID
//     /// </summary>
//     [HttpGet("{id}")]
//     public async Task<ActionResult<ApiResponse<CityDto>>> GetCity(int id)
//     {
//         try
//         {
//             var result = await _cityService.GetByIdAsync(id);

//             if (result.Success)
//             {
//                 var cityDto = MapToDto(result.Data, this.GetLanguage());
//                 var mappedResult = Result<CityDto>.Ok(result.MessageCode, cityDto);
//                 return HandleResult(mappedResult);
//             }

//             var failedResult = Result<CityDto>.Error(result.MessageCode, default!);
//             return HandleResult(failedResult);
//         }
//         catch (Exception ex)
//         {
//             return HandleInternalError<CityDto>(ex, $"retrieving city with ID {id}");
//         }
//     }

//     /// <summary>
//     /// Create a new city
//     /// </summary>
//     [HttpPost]
//     public async Task<ActionResult<ApiResponse<CityDto>>> CreateCity(
//         [FromBody] CreateCityDto createCityDto
//     )
//     {
//         try
//         {
//             if (!IsModelValid())
//             {
//                 return HandleValidationError<CityDto>();
//             }

//             var city = new City
//             {
//                 NameAr = createCityDto.NameAr,
//                 NameEn = createCityDto.NameEn,
//                 GovernorateId = createCityDto.GovernorateId,
//                 IsActive = createCityDto.IsActive,
//             };

//             var result = await _cityService.CreateAsync(city);

//             if (result.Success)
//             {
//                 var cityDto = MapToDto(result.Data, this.GetLanguage());
//                 var mappedResult = Result<CityDto>.Ok(result.MessageCode, cityDto);
//                 return HandleResult(mappedResult);
//             }

//             var failedResult = Result<CityDto>.Error(result.MessageCode, default!);
//             return HandleResult(failedResult);
//         }
//         catch (Exception ex)
//         {
//             return HandleInternalError<CityDto>(ex, "creating city");
//         }
//     }

//     /// <summary>
//     /// Update an existing city
//     /// </summary>
//     [HttpPut("{id}")]
//     public async Task<ActionResult<ApiResponse<CityDto>>> UpdateCity(
//         int id,
//         [FromBody] UpdateCityDto updateCityDto
//     )
//     {
//         try
//         {
//             if (!IsModelValid())
//             {
//                 return HandleValidationError<CityDto>();
//             }

//             var city = new City
//             {
//                 NameAr = updateCityDto.NameAr,
//                 NameEn = updateCityDto.NameEn,
//                 GovernorateId = updateCityDto.GovernorateId,
//                 IsActive = updateCityDto.IsActive,
//             };

//             var result = await _cityService.UpdateAsync(id, city);

//             if (result.Success)
//             {
//                 var cityDto = MapToDto(result.Data, this.GetLanguage());
//                 var mappedResult = Result<CityDto>.Ok(result.MessageCode, cityDto);
//                 return HandleResult(mappedResult);
//             }

//             var failedResult = Result<CityDto>.Error(result.MessageCode, default!);
//             return HandleResult(failedResult);
//         }
//         catch (Exception ex)
//         {
//             return HandleInternalError<CityDto>(ex, $"updating city with ID {id}");
//         }
//     }

//     /// <summary>
//     /// Delete a city (soft delete)
//     /// </summary>
//     [HttpDelete("{id}")]
//     public async Task<ActionResult<ApiResponse>> DeleteCity(int id)
//     {
//         try
//         {
//             var result = await _cityService.DeleteAsync(id);
//             return HandleResult(result);
//         }
//         catch (Exception ex)
//         {
//             return HandleInternalError(ex, $"deleting city with ID {id}");
//         }
//     }

//     /// <summary>
//     /// Get cities by governorate ID
//     /// </summary>
//     [HttpGet("governorate/{governorateId}")]
//     public async Task<ActionResult<ApiResponse<IEnumerable<CityDto>>>> GetCitiesByGovernorate(
//         int governorateId
//     )
//     {
//         try
//         {
//             var result = await _cityService.GetByGovernorateIdAsync(governorateId);

//             if (result.Success)
//             {
//                 var cityDtos = result.Data.Select(c => MapToDto(c, this.GetLanguage()));
//                 var mappedResult = Result<IEnumerable<CityDto>>.Ok(result.MessageCode, cityDtos);
//                 return HandleResult(mappedResult);
//             }

//             var failedResult = Result<IEnumerable<CityDto>>.Error(result.MessageCode, default!);
//             return HandleResult(failedResult);
//         }
//         catch (Exception ex)
//         {
//             return HandleInternalError<IEnumerable<CityDto>>(
//                 ex,
//                 $"retrieving cities for governorate {governorateId}"
//             );
//         }
//     }

//     /// <summary>
//     /// Check if a city exists by ID
//     /// </summary>
//     [HttpGet("{id}/exists")]
//     public async Task<ActionResult<ApiResponse<bool>>> CheckCityExists(int id)
//     {
//         try
//         {
//             var result = await _cityService.ExistsAsync(id);
//             return HandleResult(result);
//         }
//         catch (Exception ex)
//         {
//             return HandleInternalError<bool>(ex, $"checking if city exists with ID {id}");
//         }
//     }

//     private static CityDto MapToDto(City city, string language)
//     {
//         return new CityDto
//         {
//             Id = city.Id,
//             NameAr = city.NameAr,
//             NameEn = city.NameEn,
//             GovernorateId = city.GovernorateId,
//             IsActive = city.IsActive,
//             CreatedAt = city.CreatedAt,
//             UpdatedAt = city.UpdatedAt,
//             Governorate =
//                 city.Governorate != null
//                     ? new GovernorateDto
//                     {
//                         Id = city.Governorate.Id,
//                         Name = language == "ar" ? city.Governorate.NameAr : city.Governorate.NameEn,
//                         IsActive = city.Governorate.IsActive,
//                         CreatedAt = city.Governorate.CreatedAt,
//                         UpdatedAt = city.Governorate.UpdatedAt,
//                     }
//                     : null,
//         };
//     }
// }
