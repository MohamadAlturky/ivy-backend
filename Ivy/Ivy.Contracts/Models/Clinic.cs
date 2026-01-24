namespace Ivy.Contracts.Models;

public class ClinicDto
{
    public int Id { get; set; }
    public string NameAr { get; set; } = string.Empty;
    public string NameEn { get; set; } = string.Empty;
    public string DescriptionAr { get; set; } = string.Empty;
    public string DescriptionEn { get; set; } = string.Empty;
    public string ContactPhoneNumber { get; set; } = string.Empty;
    public string ContactEmail { get; set; } = string.Empty;
    public LocationDto Location { get; set; } = null!;
    public bool IsActive { get; set; }
}

public class CreateClinicDto
{
    public string NameAr { get; set; } = string.Empty;
    public string NameEn { get; set; } = string.Empty;
    public string DescriptionAr { get; set; } = string.Empty;
    public string DescriptionEn { get; set; } = string.Empty;
    public LocationPayLoadForClinicCreateOrUpdateDto Location { get; set; } = null!;
    public bool IsActive { get; set; } = true;
}

public class UpdateClinicDto
{
    public string NameAr { get; set; } = string.Empty;
    public string NameEn { get; set; } = string.Empty;
    public string DescriptionAr { get; set; } = string.Empty;
    public string DescriptionEn { get; set; } = string.Empty;
    public string ContactPhoneNumber { get; set; } = string.Empty;
    public string ContactEmail { get; set; } = string.Empty;
    public LocationPayLoadForClinicCreateOrUpdateDto? Location { get; set; }
    public bool IsActive { get; set; }
}

public class ClinicLocalizedDto
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string ContactPhoneNumber { get; set; } = string.Empty;
    public string ContactEmail { get; set; } = string.Empty;
    public LocationLocalizedDto Location { get; set; } = null!;
    public bool IsActive { get; set; }
}

public class ClinicDropDownDto
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
}

// -----------------------------
// -----------------------------
// LOCATION
// -----------------------------
// -----------------------------
public class LocationDto
{
    public int Id { get; set; }
    public string NameAr { get; set; } = string.Empty;
    public string NameEn { get; set; } = string.Empty;
    public decimal Latitude { get; set; }
    public decimal Longitude { get; set; }
    public CityDto City { get; set; } = null!;
}

public class LocationLocalizedDto
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public decimal Latitude { get; set; }
    public decimal Longitude { get; set; }
    public CityLocalizedDto City { get; set; } = null!;
}

public class LocationPayLoadForClinicCreateOrUpdateDto
{
    public string NameAr { get; set; } = string.Empty;
    public string NameEn { get; set; } = string.Empty;
    public decimal Latitude { get; set; }
    public decimal Longitude { get; set; }
    public int CityId { get; set; }
}
