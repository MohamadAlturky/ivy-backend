namespace Ivy.Core.Entities;

public class Admin : BaseEntity<int>
{
    public string Email { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
    public List<AdminRolesLink> AdminRolesLinks { get; set; } = [];
}
