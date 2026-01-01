namespace Ivy.Contracts.Models;

public class CityDto
{
    public int Id { get; set; }
    public string NameAr { get; set; } = string.Empty;
    public string NameEn { get; set; } = string.Empty;
    public GovernorateDto Governorate { get; set; } = null!;
    public bool IsActive { get; set; }
}

public class CreateCityDto
{
    public string NameAr { get; set; } = string.Empty;
    public string NameEn { get; set; } = string.Empty;
    public int GovernorateId { get; set; }
}

public class UpdateCityDto
{
    public string NameAr { get; set; } = string.Empty;
    public string NameEn { get; set; } = string.Empty;
    public int GovernorateId { get; set; }
}

public class CityLocalizedDto
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public GovernorateLocalizedDto Governorate { get; set; } = null!;
    public bool IsActive { get; set; }
}

public class CityDropDownDto
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public GovernorateDropDownDto Governorate { get; set; } = null!;
}
