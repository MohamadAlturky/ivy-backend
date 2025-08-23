using System.ComponentModel.DataAnnotations;

namespace Ivy.Api.DTOs;

public class GovernorateDto
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public bool IsActive { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
    public ICollection<CityDto>? Cities { get; set; }
}

public class CreateGovernorateDto
{
    [Required]
    [StringLength(100, MinimumLength = 1)]
    public string NameAr { get; set; } = string.Empty;

    [Required]
    [StringLength(100, MinimumLength = 1)]
    public string NameEn { get; set; } = string.Empty;

    public bool IsActive { get; set; } = true;
}

public class UpdateGovernorateDto
{
    [Required]
    [StringLength(100, MinimumLength = 1)]
    public string NameAr { get; set; } = string.Empty;

    [Required]
    [StringLength(100, MinimumLength = 1)]
    public string NameEn { get; set; } = string.Empty;

    public bool IsActive { get; set; } = true;
}

public class GovernorateQueryDto
{
    [Range(1, int.MaxValue, ErrorMessage = "Page must be greater than 0")]
    public int Page { get; set; } = 1;

    [Range(1, 100, ErrorMessage = "PageSize must be between 1 and 100")]
    public int PageSize { get; set; } = 10;

    [StringLength(100)]
    public string? Name { get; set; }

    public bool? IsActive { get; set; }

    public bool IncludeCities { get; set; } = false;
}
