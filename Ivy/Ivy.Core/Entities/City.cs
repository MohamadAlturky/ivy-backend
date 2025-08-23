namespace Ivy.Core.Entities;

public class City : BaseEntity<int>
{
    public string NameAr { get; set; } = string.Empty;
    public string NameEn { get; set; } = string.Empty;
    public int GovernorateId { get; set; }
    public Governorate Governorate { get; set; } = null!;
}
