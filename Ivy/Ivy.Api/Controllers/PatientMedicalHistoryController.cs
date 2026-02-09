using Ivy.Contracts.Models;
using Ivy.Contracts.Services;
using Microsoft.AspNetCore.Mvc;

namespace Ivy.Api.Controllers;

[Route("api/patients/medical-histories")]
public class PatientMedicalHistoryController : BaseController
{
    private readonly IMedicalHistoryService _medicalHistoryService;

    public PatientMedicalHistoryController(
        IMedicalHistoryService medicalHistoryService,
        IApiResponseRepresenter responseRepresenter,
        ILogger<PatientMedicalHistoryController> logger
    )
        : base(responseRepresenter, logger)
    {
        _medicalHistoryService = medicalHistoryService;
    }

    [HttpGet]
    public async Task<IActionResult> GetPatientMedicalHistories()
    {
        var result = await _medicalHistoryService.GetByPatientIdAsync(GetUserId());
        return HandleResult(result);
    }

    [HttpPost]
    public async Task<IActionResult> CreatePatientMedicalHistory(
        [FromBody] CreateMedicalHistoryForPatientDto createMedicalHistoryDto
    )
    {
        var result = await _medicalHistoryService.CreateAsync(
            new CreateMedicalHistoryDto
            {
                CreatedByUserId = GetUserId(),
                PatientId = GetUserId(),
                Type = createMedicalHistoryDto.Type,
                Items = createMedicalHistoryDto
                    .Items.Select(item => new CreateMedicalHistoryItemDto
                    {
                        MediaUrl = item.MediaUrl,
                        MediaType = item.MediaType,
                    })
                    .ToList(),
            }
        );
        return HandleResult(result);
    }
}
