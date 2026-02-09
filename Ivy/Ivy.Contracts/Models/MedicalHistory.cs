using System.ComponentModel.DataAnnotations;
using Ivy.Core.Entities;

namespace Ivy.Contracts.Models;

public class MedicalHistoryDto
{
    public int Id { get; set; }
    public int PatientId { get; set; }
    public int CreatedByUserId { get; set; }
    public string CreatedByUserName { get; set; } = string.Empty;
    public MedicalHistoryType Type { get; set; } = MedicalHistoryType.VisitNotes;
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
    public bool IsActive { get; set; }
    public List<MedicalHistoryItemDto> Items { get; set; } = [];
}

public class MedicalHistoryItemDto
{
    public int Id { get; set; }
    public int MedicalHistoryId { get; set; }
    public string MediaUrl { get; set; } = string.Empty;
    public MediaType MediaType { get; set; }
    public DateTime CreatedAt { get; set; }
}

public class CreateMedicalHistoryDto
{
    public int PatientId { get; set; }

    public int CreatedByUserId { get; set; }

    public MedicalHistoryType Type { get; set; } = MedicalHistoryType.VisitNotes;

    public List<CreateMedicalHistoryItemDto> Items { get; set; } = [];
}

public class CreateMedicalHistoryForPatientDto
{
    public MedicalHistoryType Type { get; set; } = MedicalHistoryType.VisitNotes;

    public List<CreateMedicalHistoryItemDto> Items { get; set; } = [];
}

public class CreateMedicalHistoryItemDto
{
    public string MediaUrl { get; set; } = string.Empty;

    public MediaType MediaType { get; set; } = MediaType.Image;
}

public class UpdateMedicalHistoryDto
{
    public MedicalHistoryType Type { get; set; } = MedicalHistoryType.VisitNotes;

    public bool IsActive { get; set; } = true;

    public List<UpdateMedicalHistoryItemDto> Items { get; set; } = [];
}

public class UpdateMedicalHistoryItemDto
{
    public int? Id { get; set; } // Null if new item

    public string MediaUrl { get; set; } = string.Empty;

    public MediaType MediaType { get; set; } = MediaType.Image;
}

public class MedicalHistoryQueryDto
{
    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 10;
    public int? PatientId { get; set; }
    public int? CreatedByUserId { get; set; }
    public MedicalHistoryType? Type { get; set; }
    public bool? IsActive { get; set; }
    public DateTime? FromDate { get; set; }
    public DateTime? ToDate { get; set; }
}
