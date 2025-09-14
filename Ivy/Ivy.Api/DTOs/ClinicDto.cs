using System.ComponentModel.DataAnnotations;

namespace Ivy.Api.DTOs;

public class ClinicDto
{
    public int Id { get; set; }
    public string NameAr { get; set; } = string.Empty;
    public string NameEn { get; set; } = string.Empty;
    public string DescriptionAr { get; set; } = string.Empty;
    public string DescriptionEn { get; set; } = string.Empty;
    public string ContactPhoneNumber { get; set; } = string.Empty;
    public string ContactEmail { get; set; } = string.Empty;
    public int LocationId { get; set; }
    public bool IsActive { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
    public LocationDto? Location { get; set; }
    public IEnumerable<ClinicImageDto> ClinicImages { get; set; } = [];
}

public class CreateClinicDto
{
    [Required]
    [StringLength(200, MinimumLength = 1)]
    public string NameAr { get; set; } = string.Empty;

    [Required]
    [StringLength(200, MinimumLength = 1)]
    public string NameEn { get; set; } = string.Empty;

    [Required]
    [StringLength(1000, MinimumLength = 1)]
    public string DescriptionAr { get; set; } = string.Empty;

    [Required]
    [StringLength(1000, MinimumLength = 1)]
    public string DescriptionEn { get; set; } = string.Empty;

    [Required]
    [Phone]
    [StringLength(20)]
    public string ContactPhoneNumber { get; set; } = string.Empty;

    [Required]
    [EmailAddress]
    [StringLength(255)]
    public string ContactEmail { get; set; } = string.Empty;

    [Required]
    [Range(1, int.MaxValue, ErrorMessage = "LocationId must be greater than 0")]
    public int LocationId { get; set; }

    public bool IsActive { get; set; } = true;
}

public class UpdateClinicDto
{
    [Required]
    [StringLength(200, MinimumLength = 1)]
    public string NameAr { get; set; } = string.Empty;

    [Required]
    [StringLength(200, MinimumLength = 1)]
    public string NameEn { get; set; } = string.Empty;

    [Required]
    [StringLength(1000, MinimumLength = 1)]
    public string DescriptionAr { get; set; } = string.Empty;

    [Required]
    [StringLength(1000, MinimumLength = 1)]
    public string DescriptionEn { get; set; } = string.Empty;

    [Required]
    [Phone]
    [StringLength(20)]
    public string ContactPhoneNumber { get; set; } = string.Empty;

    [Required]
    [EmailAddress]
    [StringLength(255)]
    public string ContactEmail { get; set; } = string.Empty;

    [Required]
    [Range(1, int.MaxValue, ErrorMessage = "LocationId must be greater than 0")]
    public int LocationId { get; set; }

    public bool IsActive { get; set; } = true;
}

public class ClinicQueryDto
{
    [Range(1, int.MaxValue, ErrorMessage = "Page must be greater than 0")]
    public int Page { get; set; } = 1;

    [Range(1, 100, ErrorMessage = "PageSize must be between 1 and 100")]
    public int PageSize { get; set; } = 10;

    [StringLength(200)]
    public string? NameAr { get; set; }

    [StringLength(200)]
    public string? NameEn { get; set; }

    [StringLength(1000)]
    public string? DescriptionAr { get; set; }

    [StringLength(1000)]
    public string? DescriptionEn { get; set; }

    public int? LocationId { get; set; }

    public bool? IsActive { get; set; }
}

public class ClinicImageDto
{
    public int Id { get; set; }
    public int ClinicId { get; set; }
    public string ImageUrl { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}

public class LocationDto
{
    public int Id { get; set; }
    public required string NameAr { get; set; } = string.Empty;
    public required string NameEn { get; set; } = string.Empty;
    public decimal Latitude { get; set; }
    public decimal Longitude { get; set; }
    public int CityId { get; set; }
    public bool IsActive { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}

public class AddDoctorToClinicDto
{
    [Required]
    [Range(1, int.MaxValue, ErrorMessage = "DoctorId must be greater than 0")]
    public int DoctorId { get; set; }
}

public class DoctorClinicDto
{
    public int Id { get; set; }
    public int DoctorId { get; set; }
    public int ClinicId { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
    public DoctorDetailsDto? Doctor { get; set; }
    public ClinicBasicDto? Clinic { get; set; }
}

public class DoctorDetailsDto
{
    public int UserId { get; set; }
    public string ProfileImageUrl { get; set; } = string.Empty;
    public bool IsProfileCompleted { get; set; }
    public UserBasicDto? User { get; set; }
}

public class UserBasicDto
{
    public int Id { get; set; }
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string PhoneNumber { get; set; } = string.Empty;
}

public class ClinicBasicDto
{
    public int Id { get; set; }
    public string NameAr { get; set; } = string.Empty;
    public string NameEn { get; set; } = string.Empty;
    public string ContactPhoneNumber { get; set; } = string.Empty;
    public string ContactEmail { get; set; } = string.Empty;
}