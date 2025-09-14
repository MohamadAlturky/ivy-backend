namespace Ivy.Core.Entities;

public class Clinic : BaseEntity<int>
{
    public string NameAr { get; set; } = string.Empty;
    public string NameEn { get; set; } = string.Empty;
    public string DescriptionAr { get; set; } = string.Empty;
    public string DescriptionEn { get; set; } = string.Empty;
    public string ContactPhoneNumber { get; set; } = string.Empty;
    public string ContactEmail { get; set; } = string.Empty;
    public int LocationId { get; set; }
    public Location Location { get; set; } = null!;
    public ICollection<ClinicImages> ClinicImages { get; set; } = [];
}

public class ClinicImages : BaseEntity<int>
{
    public int ClinicId { get; set; }
    public string ImageUrl { get; set; } = string.Empty;
}
