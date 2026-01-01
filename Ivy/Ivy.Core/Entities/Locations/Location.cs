namespace Ivy.Core.Entities;

public class Location : BaseEntity<int>
{
    public string NameAr { get; set; } = string.Empty;
    public string NameEn { get; set; } = string.Empty;
    public decimal Latitude { get; set; }
    public decimal Longitude { get; set; }
    public int CityId { get; set; }
    public City City { get; set; } = null!;
}