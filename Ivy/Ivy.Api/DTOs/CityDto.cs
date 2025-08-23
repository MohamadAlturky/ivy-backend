using System.ComponentModel.DataAnnotations;

namespace Ivy.Api.DTOs;

public class CityDto
{
    public int Id { get; set; }
    public string NameAr { get; set; } = string.Empty;
    public string NameEn { get; set; } = string.Empty;
    public int GovernorateId { get; set; }
    public bool IsActive { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
    public GovernorateDto? Governorate { get; set; }
}

public class CreateCityDto
{
    [Required]
    [StringLength(100, MinimumLength = 1)]
    public string NameAr { get; set; } = string.Empty;

    [Required]
    [StringLength(100, MinimumLength = 1)]
    public string NameEn { get; set; } = string.Empty;

    [Required]
    [Range(1, int.MaxValue, ErrorMessage = "GovernorateId must be greater than 0")]
    public int GovernorateId { get; set; }

    public bool IsActive { get; set; } = true;
}

public class UpdateCityDto
{
    [Required]
    [StringLength(100, MinimumLength = 1)]
    public string NameAr { get; set; } = string.Empty;

    [Required]
    [StringLength(100, MinimumLength = 1)]
    public string NameEn { get; set; } = string.Empty;

    [Required]
    [Range(1, int.MaxValue, ErrorMessage = "GovernorateId must be greater than 0")]
    public int GovernorateId { get; set; }

    public bool IsActive { get; set; } = true;
}

public class CityQueryDto
{
    [Range(1, int.MaxValue, ErrorMessage = "Page must be greater than 0")]
    public int Page { get; set; } = 1;

    [Range(1, 100, ErrorMessage = "PageSize must be between 1 and 100")]
    public int PageSize { get; set; } = 10;

    [StringLength(100)]
    public string? NameAr { get; set; }

    [StringLength(100)]
    public string? NameEn { get; set; }

    public int? GovernorateId { get; set; }

    public bool? IsActive { get; set; }
}
