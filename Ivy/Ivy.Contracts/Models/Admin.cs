namespace Ivy.Contracts.Models;

public class UpdateAdminProfileDto
{
    public string Email { get; set; } = string.Empty;

    public string? CurrentPassword { get; set; }
    public string? NewPassword { get; set; }
}

public class ChangePasswordDto
{
    public string CurrentPassword { get; set; } = string.Empty;
    public string NewPassword { get; set; } = string.Empty;
}

public class AdminDto
{
    public int Id { get; set; }
    public string Email { get; set; } = string.Empty;
    public bool IsActive { get; set; }
    public DateTime CreatedAt { get; set; }
    public required bool IsClinicAdmin { get; set; }
    public required ClinicLocalizedDto? Clinic { get; set; }
}

public class AdminLoginResponseDto
{
    public AdminDto Profile { get; set; } = null!;
    public string Token { get; set; } = string.Empty;
    public bool IsClinicAdmin { get; set; }
}

public class AdminLoginDto
{
    public string Email { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
}
