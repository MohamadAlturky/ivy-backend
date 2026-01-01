namespace Ivy.Core.Entities;

public class RolePermissionsLink : BaseEntity<int>
{
    public int RoleId { get; set; }
    public AdminRole Role { get; set; } = null!;
    public int PermissionId { get; set; }
    public AdminPermission Permission { get; set; } = null!;
}