namespace Ivy.Core.Entities;

public class Governorate : BaseEntity<int>
{
    public string NameAr { get; set; } = string.Empty;
    public string NameEn { get; set; } = string.Empty;
    public ICollection<City> Cities { get; set; } = new List<City>();
}
