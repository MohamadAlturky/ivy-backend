using Ivy.Api.DTOs;
using Ivy.Api.Services;
using Ivy.Core.Entities;
using Ivy.Core.Services;
using IvyBackend;
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

    /// <summary>
    /// Get all medical specialities with pagination and filtering
    /// </summary>
    [HttpGet]
    public async Task<
        IActionResult
    > GetAllMedicalSpecialities([FromQuery] MedicalSpecialityQueryDto query)
    {
        try
        {
            var result = await _medicalSpecialityService.GetAllAsync(
                query.Page,
                query.PageSize,
                query.Name,
                query.Name,
                query.SearchTerm,
                query.IsActive
            );

            if (result.Success)
            {
                var medicalSpecialityDtos = result.Data.Data.Select(ms =>
                    MapToDto(ms, this.GetLanguage())
                );
                var paginatedDto = new PaginatedResult<MedicalSpecialityDto>
                {
                    Data = medicalSpecialityDtos,
                    TotalCount = result.Data.TotalCount,
                    Page = result.Data.Page,
                    PageSize = result.Data.PageSize,
                    TotalPages = result.Data.TotalPages,
                    HasNextPage = result.Data.HasNextPage,
                    HasPreviousPage = result.Data.HasPreviousPage,
                };

                var mappedResult = Result<PaginatedResult<MedicalSpecialityDto>>.Ok(
                    result.MessageCode,
                    paginatedDto
                );
                return HandleResult(mappedResult);
            }

            var failedResult = Result<PaginatedResult<MedicalSpecialityDto>>.Error(
                result.MessageCode,
                default!
            );
            return HandleResult(failedResult);
        }
        catch (Exception ex)
        {
            return HandleInternalError<PaginatedResult<MedicalSpecialityDto>>(
                ex,
                "retrieving medical specialities"
            );
        }
    }

    /// <summary>
    /// Get a specific medical speciality by ID
    /// </summary>
    [HttpGet("{id}")]
    public async Task<IActionResult> GetMedicalSpeciality(int id)
    {
        try
        {
            var result = await _medicalSpecialityService.GetByIdAsync(id);

            if (result.Success)
            {
                var medicalSpecialityDto = MapToDto(result.Data, this.GetLanguage());
                var mappedResult = Result<MedicalSpecialityDto>.Ok(result.MessageCode, medicalSpecialityDto);
                return HandleResult(mappedResult);
            }

            var failedResult = Result<MedicalSpecialityDto>.Error(result.MessageCode, default!);
            return HandleResult(failedResult);
        }
        catch (Exception ex)
        {
            return HandleInternalError<MedicalSpecialityDto>(ex, $"retrieving medical speciality with ID {id}");
        }
    }

    /// <summary>
    /// Create a new medical speciality
    /// </summary>
    [HttpPost]
    public async Task<IActionResult> CreateMedicalSpeciality(
        [FromBody] CreateMedicalSpecialityDto createMedicalSpecialityDto
    )
    {
        try
        {
            if (!IsModelValid())
            {
                return HandleValidationError<MedicalSpecialityDto>();
            }

            var medicalSpeciality = new MedicalSpeciality
            {
                NameAr = createMedicalSpecialityDto.NameAr,
                NameEn = createMedicalSpecialityDto.NameEn,
                DescriptionAr = createMedicalSpecialityDto.DescriptionAr,
                DescriptionEn = createMedicalSpecialityDto.DescriptionEn,
                IsActive = createMedicalSpecialityDto.IsActive,
            };

            var result = await _medicalSpecialityService.CreateAsync(medicalSpeciality);

            if (result.Success)
            {
                var medicalSpecialityDto = MapToDto(result.Data, this.GetLanguage());
                var mappedResult = Result<MedicalSpecialityDto>.Ok(result.MessageCode, medicalSpecialityDto);
                return HandleResult(mappedResult);
            }

            var failedResult = Result<MedicalSpecialityDto>.Error(result.MessageCode, default!);
            return HandleResult(failedResult);
        }
        catch (Exception ex)
        {
            return HandleInternalError<MedicalSpecialityDto>(ex, "creating medical speciality");
        }
    }

    /// <summary>
    /// Update an existing medical speciality
    /// </summary>
    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateMedicalSpeciality(
        int id,
        [FromBody] UpdateMedicalSpecialityDto updateMedicalSpecialityDto
    )
    {
        try
        {
            if (!IsModelValid())
            {
                return HandleValidationError<MedicalSpecialityDto>();
            }

            var medicalSpeciality = new MedicalSpeciality
            {
                NameAr = updateMedicalSpecialityDto.NameAr,
                NameEn = updateMedicalSpecialityDto.NameEn,
                DescriptionAr = updateMedicalSpecialityDto.DescriptionAr,
                DescriptionEn = updateMedicalSpecialityDto.DescriptionEn,
                IsActive = updateMedicalSpecialityDto.IsActive,
            };

            var result = await _medicalSpecialityService.UpdateAsync(id, medicalSpeciality);

            if (result.Success)
            {
                var medicalSpecialityDto = MapToDto(result.Data, this.GetLanguage());
                var mappedResult = Result<MedicalSpecialityDto>.Ok(result.MessageCode, medicalSpecialityDto);
                return HandleResult(mappedResult);
            }

            var failedResult = Result<MedicalSpecialityDto>.Error(result.MessageCode, default!);
            return HandleResult(failedResult);
        }
        catch (Exception ex)
        {
            return HandleInternalError<MedicalSpecialityDto>(ex, $"updating medical speciality with ID {id}");
        }
    }

    /// <summary>
    /// Delete a medical speciality (soft delete)
    /// </summary>
    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteMedicalSpeciality(int id)
    {
        try
        {
            var result = await _medicalSpecialityService.DeleteAsync(id);
            return HandleResult(result);
        }
        catch (Exception ex)
        {
            return HandleInternalError(ex, $"deleting medical speciality with ID {id}");
        }
    }

    /// <summary>
    /// Check if a medical speciality exists by ID
    /// </summary>
    [HttpGet("{id}/exists")]
    public async Task<IActionResult> CheckMedicalSpecialityExists(int id)
    {
        try
        {
            var result = await _medicalSpecialityService.ExistsAsync(id);
            return HandleResult(result);
        }
        catch (Exception ex)
        {
            return HandleInternalError<bool>(ex, $"checking if medical speciality exists with ID {id}");
        }
    }

    private static MedicalSpecialityDto MapToDto(MedicalSpeciality medicalSpeciality, string language)
    {
        return new MedicalSpecialityDto
        {
            Id = medicalSpeciality.Id,
            Name = language == "ar" ? medicalSpeciality.NameAr : medicalSpeciality.NameEn,
            Description = language == "ar" ? medicalSpeciality.DescriptionAr : medicalSpeciality.DescriptionEn,
            IsActive = medicalSpeciality.IsActive,
            CreatedAt = medicalSpeciality.CreatedAt,
            UpdatedAt = medicalSpeciality.UpdatedAt,
        };
    }
}
