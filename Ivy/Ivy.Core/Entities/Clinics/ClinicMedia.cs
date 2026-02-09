namespace Ivy.Core.Entities;

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
    Pdf = 4,
}
