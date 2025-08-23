using System.ComponentModel.DataAnnotations;

namespace Ivy.Api.DTOs;

public class MedicalSpecialityDto
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public bool IsActive { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}

public class CreateMedicalSpecialityDto
{
    [Required]
    [StringLength(100, MinimumLength = 1)]
    public string NameAr { get; set; } = string.Empty;

    [Required]
    [StringLength(100, MinimumLength = 1)]
    public string NameEn { get; set; } = string.Empty;

    [StringLength(500)]
    public string DescriptionAr { get; set; } = string.Empty;

    [StringLength(500)]
    public string DescriptionEn { get; set; } = string.Empty;

    public bool IsActive { get; set; } = true;
}

public class UpdateMedicalSpecialityDto
{
    [Required]
    [StringLength(100, MinimumLength = 1)]
    public string NameAr { get; set; } = string.Empty;

    [Required]
    [StringLength(100, MinimumLength = 1)]
    public string NameEn { get; set; } = string.Empty;

    [StringLength(500)]
    public string DescriptionAr { get; set; } = string.Empty;

    [StringLength(500)]
    public string DescriptionEn { get; set; } = string.Empty;

    public bool IsActive { get; set; } = true;
}

public class MedicalSpecialityQueryDto
{
    [Range(1, int.MaxValue, ErrorMessage = "Page must be greater than 0")]
    public int Page { get; set; } = 1;

    [Range(1, 100, ErrorMessage = "PageSize must be between 1 and 100")]
    public int PageSize { get; set; } = 10;

    [StringLength(100)]
    public string? Name { get; set; }

    [StringLength(100)]
    public string? SearchTerm { get; set; }

    public bool? IsActive { get; set; }
}
