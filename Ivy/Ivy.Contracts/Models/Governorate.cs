namespace Ivy.Contracts.Models;

public class GovernorateDto
{
    public int Id { get; set; }
    public string NameAr { get; set; } = string.Empty;
    public string NameEn { get; set; } = string.Empty;
    public bool IsActive { get; set; }
}

public class CreateGovernorateDto
{
    public string NameAr { get; set; } = string.Empty;
    public string NameEn { get; set; } = string.Empty;
    public bool IsActive { get; set; } = true;
}

public class UpdateGovernorateDto
{
    public string NameAr { get; set; } = string.Empty;
    public string NameEn { get; set; } = string.Empty;
    public bool IsActive { get; set; }
}

public class GovernorateLocalizedDto
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public bool IsActive { get; set; }
}

public class GovernorateDropDownDto
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
}
