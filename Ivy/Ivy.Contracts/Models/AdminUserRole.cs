namespace Ivy.Contracts.Models;

public class UpdateAdminUserRolesDto
{
    public List<int> RoleIds { get; set; } = new();
}