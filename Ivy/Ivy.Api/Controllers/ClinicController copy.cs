// using Ivy.Api.DTOs;
// using Ivy.Api.Services;
// using Ivy.Core.Entities;
// using Ivy.Core.Services;
// using IvyBackend;
// using Microsoft.AspNetCore.Mvc;

// namespace Ivy.Api.Controllers;

// [Route("api/clinics")]
// public class ClinicController : BaseController
// {
//     private readonly IClinicService _clinicService;

//     public ClinicController(
//         IClinicService clinicService,
//         IApiResponseRepresenter responseRepresenter,
//         ILogger<ClinicController> logger
//     )
//         : base(responseRepresenter, logger)
//     {
//         _clinicService = clinicService;
//     }

//     /// <summary>
//     /// Get all clinics with pagination and filtering
//     /// </summary>
//     [HttpGet]
//     public async Task<IActionResult> GetAllClinics(
//         [FromQuery] ClinicQueryDto query
//     )
//     {
//         try
//         {
//             var result = await _clinicService.GetAllAsync(
//                 query.Page,
//                 query.PageSize,
//                 query.NameAr,
//                 query.NameEn,
//                 query.DescriptionAr,
//                 query.DescriptionEn,
//                 query.LocationId,
//                 query.IsActive
//             );

//             if (result.Success)
//             {
//                 var clinicDtos = result.Data.Data.Select(c => MapToDto(c, this.GetLanguage()));
//                 var paginatedDto = new PaginatedResult<ClinicDto>
//                 {
//                     Data = clinicDtos,
//                     TotalCount = result.Data.TotalCount,
//                     Page = result.Data.Page,
//                     PageSize = result.Data.PageSize,
//                     TotalPages = result.Data.TotalPages,
//                     HasNextPage = result.Data.HasNextPage,
//                     HasPreviousPage = result.Data.HasPreviousPage,
//                 };

//                 var mappedResult = Result<PaginatedResult<ClinicDto>>.Ok(
//                     result.MessageCode,
//                     paginatedDto
//                 );
//                 return HandleResult(mappedResult);
//             }

//             var failedResult = Result<PaginatedResult<ClinicDto>>.Error(result.MessageCode, default!);
//             return HandleResult(failedResult);
//         }
//         catch (Exception ex)
//         {
//             return HandleInternalError<PaginatedResult<ClinicDto>>(ex, "retrieving clinics");
//         }
//     }

//     /// <summary>
//     /// Get a specific clinic by ID
//     /// </summary>
//     [HttpGet("{id}")]
//     public async Task<IActionResult> GetClinic(int id)
//     {
//         try
//         {
//             var result = await _clinicService.GetByIdAsync(id);

//             if (result.Success)
//             {
//                 var clinicDto = MapToDto(result.Data, this.GetLanguage());
//                 var mappedResult = Result<ClinicDto>.Ok(result.MessageCode, clinicDto);
//                 return HandleResult(mappedResult);
//             }

//             var failedResult = Result<ClinicDto>.Error(result.MessageCode, default!);
//             return HandleResult(failedResult);
//         }
//         catch (Exception ex)
//         {
//             return HandleInternalError<ClinicDto>(ex, $"retrieving clinic with ID {id}");
//         }
//     }

//     /// <summary>
//     /// Create a new clinic
//     /// </summary>
//     [HttpPost]
//     public async Task<IActionResult> CreateClinic(
//         [FromBody] CreateClinicDto createClinicDto
//     )
//     {
//         try
//         {
//             if (!IsModelValid())
//             {
//                 return HandleValidationError<ClinicDto>();
//             }

//             var clinic = new Clinic
//             {
//                 NameAr = createClinicDto.NameAr,
//                 NameEn = createClinicDto.NameEn,
//                 DescriptionAr = createClinicDto.DescriptionAr,
//                 DescriptionEn = createClinicDto.DescriptionEn,
//                 ContactPhoneNumber = createClinicDto.ContactPhoneNumber,
//                 ContactEmail = createClinicDto.ContactEmail,
//                 LocationId = createClinicDto.LocationId,
//                 IsActive = createClinicDto.IsActive,
//             };

//             var result = await _clinicService.CreateAsync(clinic);

//             if (result.Success)
//             {
//                 var clinicDto = MapToDto(result.Data, this.GetLanguage());
//                 var mappedResult = Result<ClinicDto>.Ok(result.MessageCode, clinicDto);
//                 return HandleResult(mappedResult);
//             }

//             var failedResult = Result<ClinicDto>.Error(result.MessageCode, default!);
//             return HandleResult(failedResult);
//         }
//         catch (Exception ex)
//         {
//             return HandleInternalError<ClinicDto>(ex, "creating clinic");
//         }
//     }

//     /// <summary>
//     /// Update an existing clinic
//     /// </summary>
//     [HttpPut("{id}")]
//     public async Task<IActionResult> UpdateClinic(
//         int id,
//         [FromBody] UpdateClinicDto updateClinicDto
//     )
//     {
//         try
//         {
//             if (!IsModelValid())
//             {
//                 return HandleValidationError<ClinicDto>();
//             }

//             var clinic = new Clinic
//             {
//                 NameAr = updateClinicDto.NameAr,
//                 NameEn = updateClinicDto.NameEn,
//                 DescriptionAr = updateClinicDto.DescriptionAr,
//                 DescriptionEn = updateClinicDto.DescriptionEn,
//                 ContactPhoneNumber = updateClinicDto.ContactPhoneNumber,
//                 ContactEmail = updateClinicDto.ContactEmail,
//                 LocationId = updateClinicDto.LocationId,
//                 IsActive = updateClinicDto.IsActive,
//             };

//             var result = await _clinicService.UpdateAsync(id, clinic);

//             if (result.Success)
//             {
//                 var clinicDto = MapToDto(result.Data, this.GetLanguage());
//                 var mappedResult = Result<ClinicDto>.Ok(result.MessageCode, clinicDto);
//                 return HandleResult(mappedResult);
//             }

//             var failedResult = Result<ClinicDto>.Error(result.MessageCode, default!);
//             return HandleResult(failedResult);
//         }
//         catch (Exception ex)
//         {
//             return HandleInternalError<ClinicDto>(ex, $"updating clinic with ID {id}");
//         }
//     }

//     /// <summary>
//     /// Delete a clinic (soft delete)
//     /// </summary>
//     [HttpDelete("{id}")]
//     public async Task<IActionResult> DeleteClinic(int id)
//     {
//         try
//         {
//             var result = await _clinicService.DeleteAsync(id);
//             return HandleResult(result);
//         }
//         catch (Exception ex)
//         {
//             return HandleInternalError(ex, $"deleting clinic with ID {id}");
//         }
//     }

//     /// <summary>
//     /// Get clinics by location ID
//     /// </summary>
//     [HttpGet("location/{locationId}")]
//     public async Task<IActionResult> GetClinicsByLocation(
//         int locationId
//     )
//     {
//         try
//         {
//             var result = await _clinicService.GetByLocationIdAsync(locationId);

//             if (result.Success)
//             {
//                 var clinicDtos = result.Data.Select(c => MapToDto(c, this.GetLanguage()));
//                 var mappedResult = Result<IEnumerable<ClinicDto>>.Ok(result.MessageCode, clinicDtos);
//                 return HandleResult(mappedResult);
//             }

//             var failedResult = Result<IEnumerable<ClinicDto>>.Error(result.MessageCode, default!);
//             return HandleResult(failedResult);
//         }
//         catch (Exception ex)
//         {
//             return HandleInternalError<IEnumerable<ClinicDto>>(
//                 ex,
//                 $"retrieving clinics for location {locationId}"
//             );
//         }
//     }

//     /// <summary>
//     /// Check if a clinic exists by ID
//     /// </summary>
//     [HttpGet("{id}/exists")]
//     public async Task<IActionResult> CheckClinicExists(int id)
//     {
//         try
//         {
//             var result = await _clinicService.ExistsAsync(id);
//             return HandleResult(result);
//         }
//         catch (Exception ex)
//         {
//             return HandleInternalError<bool>(ex, $"checking if clinic exists with ID {id}");
//         }
//     }

//     /// <summary>
//     /// Deactivate a clinic
//     /// </summary>
//     [HttpPatch("{id}/deactivate")]
//     public async Task<IActionResult> DeactivateClinic(int id)
//     {
//         try
//         {
//             var result = await _clinicService.DeactivateAsync(id);
//             return HandleResult(result);
//         }
//         catch (Exception ex)
//         {
//             return HandleInternalError(ex, $"deactivating clinic with ID {id}");
//         }
//     }

//     /// <summary>
//     /// Activate a clinic
//     /// </summary>
//     [HttpPatch("{id}/activate")]
//     public async Task<IActionResult> ActivateClinic(int id)
//     {
//         try
//         {
//             var result = await _clinicService.ActivateAsync(id);
//             return HandleResult(result);
//         }
//         catch (Exception ex)
//         {
//             return HandleInternalError(ex, $"activating clinic with ID {id}");
//         }
//     }

//     /// <summary>
//     /// Add a doctor to a clinic
//     /// </summary>
//     [HttpPost("{clinicId}/doctors")]
//     public async Task<IActionResult> AddDoctorToClinic(
//         int clinicId,
//         [FromBody] AddDoctorToClinicDto addDoctorDto
//     )
//     {
//         try
//         {
//             if (!IsModelValid())
//             {
//                 return HandleValidationError<DoctorClinicDto>();
//             }

//             var result = await _clinicService.AddDoctorToClinicAsync(clinicId, addDoctorDto.DoctorId);

//             if (result.Success)
//             {
//                 var doctorClinicDto = MapToDoctorClinicDto(result.Data, this.GetLanguage());
//                 var mappedResult = Result<DoctorClinicDto>.Ok(result.MessageCode, doctorClinicDto);
//                 return HandleResult(mappedResult);
//             }

//             var failedResult = Result<DoctorClinicDto>.Error(result.MessageCode, default!);
//             return HandleResult(failedResult);
//         }
//         catch (Exception ex)
//         {
//             return HandleInternalError<DoctorClinicDto>(
//                 ex,
//                 $"adding doctor {addDoctorDto.DoctorId} to clinic {clinicId}"
//             );
//         }
//     }

//     /// <summary>
//     /// Remove a doctor from a clinic (soft delete)
//     /// </summary>
//     [HttpDelete("{clinicId}/doctors/{doctorId}")]
//     public async Task<IActionResult> RemoveDoctorFromClinic(int clinicId, int doctorId)
//     {
//         try
//         {
//             var result = await _clinicService.RemoveDoctorFromClinicAsync(clinicId, doctorId);
//             return HandleResult(result);
//         }
//         catch (Exception ex)
//         {
//             return HandleInternalError(ex, $"removing doctor {doctorId} from clinic {clinicId}");
//         }
//     }

//     /// <summary>
//     /// Get all doctors in a clinic
//     /// </summary>
//     [HttpGet("{clinicId}/doctors")]
//     public async Task<IActionResult> GetClinicDoctors(
//         int clinicId
//     )
//     {
//         try
//         {
//             var result = await _clinicService.GetClinicDoctorsAsync(clinicId);

//             if (result.Success)
//             {
//                 var doctorClinicDtos = result.Data.Select(dc => MapToDoctorClinicDto(dc, this.GetLanguage()));
//                 var mappedResult = Result<IEnumerable<DoctorClinicDto>>.Ok(result.MessageCode, doctorClinicDtos);
//                 return HandleResult(mappedResult);
//             }

//             var failedResult = Result<IEnumerable<DoctorClinicDto>>.Error(result.MessageCode, default!);
//             return HandleResult(failedResult);
//         }
//         catch (Exception ex)
//         {
//             return HandleInternalError<IEnumerable<DoctorClinicDto>>(
//                 ex,
//                 $"retrieving doctors for clinic {clinicId}"
//             );
//         }
//     }

//     private static ClinicDto MapToDto(Clinic clinic, string language)
//     {
//         return new ClinicDto
//         {
//             Id = clinic.Id,
//             NameAr = clinic.NameAr,
//             NameEn = clinic.NameEn,
//             DescriptionAr = clinic.DescriptionAr,
//             DescriptionEn = clinic.DescriptionEn,
//             ContactPhoneNumber = clinic.ContactPhoneNumber,
//             ContactEmail = clinic.ContactEmail,
//             LocationId = clinic.LocationId,
//             IsActive = clinic.IsActive,
//             CreatedAt = clinic.CreatedAt,
//             UpdatedAt = clinic.UpdatedAt,
//             Location =
//                 clinic.Location != null
//                     ? new LocationDto
//                     {
//                         Id = clinic.Location.Id,
//                         NameAr = clinic.Location.NameAr,
//                         NameEn = clinic.Location.NameEn,
//                         Latitude = clinic.Location.Latitude,
//                         Longitude = clinic.Location.Longitude,
//                         CityId = clinic.Location.CityId,
//                         IsActive = clinic.Location.IsActive,
//                         CreatedAt = clinic.Location.CreatedAt,
//                         UpdatedAt = clinic.Location.UpdatedAt,
//                     }
//                     : null,
//             ClinicImages = clinic.ClinicMedias.Select(ci => new ClinicImageDto
//             {
//                 Id = ci.Id,
//                 ClinicId = ci.ClinicId,
//                 MediaUrl = ci.MediaUrl,
//                 MediaType = ci.MediaType,
//                 CreatedAt = ci.CreatedAt,
//                 UpdatedAt = ci.UpdatedAt,
//             }).ToList()
//         };
//     }

//     private static DoctorClinicDto MapToDoctorClinicDto(DoctorClinic doctorClinic, string language)
//     {
//         return new DoctorClinicDto
//         {
//             Id = doctorClinic.Id,
//             DoctorId = doctorClinic.DoctorId,
//             ClinicId = doctorClinic.ClinicId,
//             CreatedAt = doctorClinic.CreatedAt,
//             UpdatedAt = doctorClinic.UpdatedAt,
//             Doctor = doctorClinic.Doctor != null
//                 ? new DoctorDetailsDto
//                 {
//                     UserId = doctorClinic.Doctor.UserId,
//                     ProfileImageUrl = doctorClinic.Doctor.ProfileImageUrl,
//                     IsProfileCompleted = doctorClinic.Doctor.IsProfileCompleted,
//                     User = doctorClinic.Doctor.User != null
//                         ? new UserBasicDto
//                         {
//                             Id = doctorClinic.Doctor.User.Id,
//                             FirstName = doctorClinic.Doctor.User.FirstName,
//                             LastName = doctorClinic.Doctor.User.LastName,
//                             PhoneNumber = doctorClinic.Doctor.User.PhoneNumber
//                         }
//                         : null
//                 }
//                 : null,
//             Clinic = doctorClinic.Clinic != null
//                 ? new ClinicBasicDto
//                 {
//                     Id = doctorClinic.Clinic.Id,
//                     NameAr = doctorClinic.Clinic.NameAr,
//                     NameEn = doctorClinic.Clinic.NameEn,
//                     ContactPhoneNumber = doctorClinic.Clinic.ContactPhoneNumber,
//                     ContactEmail = doctorClinic.Clinic.ContactEmail
//                 }
//                 : null
//         };
//     }
// }
