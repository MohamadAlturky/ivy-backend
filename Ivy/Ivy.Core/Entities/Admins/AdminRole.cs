namespace Ivy.Core.Entities;

public class AdminRole : BaseEntity<int>
{
    public string NameAr { get; set; } = string.Empty;
    public string NameEn { get; set; } = string.Empty;
    public string DescriptionAr { get; set; } = string.Empty;
    public string DescriptionEn { get; set; } = string.Empty;
    public List<RolePermissionsLink> RolePermissionsLinks { get; set; } = [];
}