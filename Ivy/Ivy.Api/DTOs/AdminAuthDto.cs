using System.ComponentModel.DataAnnotations;

namespace Ivy.Api.DTOs;

public class AdminLoginDto
{
    [Required]
    [EmailAddress]
    [StringLength(255)]
    public string Email { get; set; } = string.Empty;

    [Required]
    [StringLength(255, MinimumLength = 6)]
    public string Password { get; set; } = string.Empty;
}

public class AdminDto
{
    public int Id { get; set; }
    public string Email { get; set; } = string.Empty;
    public bool IsActive { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}

public class AdminAuthResponseDto
{
    public AdminDto Admin { get; set; } = null!;
    public string Message { get; set; } = string.Empty;
    public string Token { get; set; } = string.Empty;
}

public class UpdateAdminProfileDto
{
    [Required]
    [EmailAddress]
    [StringLength(255)]
    public string Email { get; set; } = string.Empty;
}

public class ChangeAdminPasswordDto
{
    [Required]
    [StringLength(255, MinimumLength = 6)]
    public string CurrentPassword { get; set; } = string.Empty;

    [Required]
    [StringLength(255, MinimumLength = 6)]
    public string NewPassword { get; set; } = string.Empty;

    [Compare("NewPassword")]
    public string ConfirmPassword { get; set; } = string.Empty;
}

public class UpdateAdminProfileWithPasswordDto
{
    [Required]
    [EmailAddress]
    [StringLength(255)]
    public string Email { get; set; } = string.Empty;

    [StringLength(255, MinimumLength = 6)]
    public string? CurrentPassword { get; set; }

    [StringLength(255, MinimumLength = 6)]
    public string? NewPassword { get; set; }

    [Compare("NewPassword")]
    public string? ConfirmPassword { get; set; }
}
