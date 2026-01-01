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
    public ICollection<ClinicMedia> ClinicMedias { get; set; } = [];
}

public class ClinicMedia : BaseEntity<int>
{
    public int ClinicId { get; set; }
    public string MediaUrl { get; set; } = string.Empty;
    public MediaType MediaType { get; set; } = MediaType.Image;
}

public enum MediaType
{
    Image = 1,
    Video = 2,
    Document = 3,
}
