namespace Ivy.Contracts.Models;

public class UpdateRolePermissionsDto
{
    public List<int> PermissionIds { get; set; } = new();
}