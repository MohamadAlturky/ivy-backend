namespace Ivy.Core.Entities;

public class Admin : BaseEntity<int>
{
    public string Email { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
    public List<AdminRolesLink> AdminRolesLinks { get; set; } = [];
}

public class AdminRolesLink : BaseEntity<int>
{
    public int AdminId { get; set; }
    public Admin Admin { get; set; } = null!;
    public int RoleId { get; set; }
    public AdminRole Role { get; set; } = null!;
}

public class AdminRole : BaseEntity<int>
{
    public string NameAr { get; set; } = string.Empty;
    public string NameEn { get; set; } = string.Empty;
    public string DescriptionAr { get; set; } = string.Empty;
    public string DescriptionEn { get; set; } = string.Empty;
    public List<RolePermissionsLink> RolePermissionsLinks { get; set; } = [];
}

public class RolePermissionsLink : BaseEntity<int>
{
    public int RoleId { get; set; }
    public AdminRole Role { get; set; } = null!;
    public int PermissionId { get; set; }
    public AdminPermission Permission { get; set; } = null!;
}

public class AdminPermission : BaseEntity<int>
{
    public string NameAr { get; set; } = string.Empty;
    public string NameEn { get; set; } = string.Empty;
    public string DescriptionAr { get; set; } = string.Empty;
    public string DescriptionEn { get; set; } = string.Empty;
}
