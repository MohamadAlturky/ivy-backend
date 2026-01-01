namespace Ivy.Core.Entities;

public class AdminRolesLink : BaseEntity<int>
{
    public int AdminId { get; set; }
    public Admin Admin { get; set; } = null!;
    public int RoleId { get; set; }
    public AdminRole Role { get; set; } = null!;
}
