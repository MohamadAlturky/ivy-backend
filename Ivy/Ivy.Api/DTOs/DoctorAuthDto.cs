using System.ComponentModel.DataAnnotations;
using Ivy.Core.Entities;

namespace Ivy.Api.DTOs;

/// <summary>
/// DTO for doctor registration request
/// </summary>
public class RegisterDoctorDto
{
    [Required(ErrorMessage = "First name is required")]
    [StringLength(50, MinimumLength = 1, ErrorMessage = "First name must be between 1 and 50 characters")]
    public string FirstName { get; set; } = string.Empty;

    [StringLength(50, ErrorMessage = "Middle name cannot exceed 50 characters")]
    public string MiddleName { get; set; } = string.Empty;

    [Required(ErrorMessage = "Last name is required")]
    [StringLength(50, MinimumLength = 1, ErrorMessage = "Last name must be between 1 and 50 characters")]
    public string LastName { get; set; } = string.Empty;

    [Required(ErrorMessage = "Password is required")]
    [StringLength(100, MinimumLength = 8, ErrorMessage = "Password must be between 8 and 100 characters")]
    public string Password { get; set; } = string.Empty;

    [Required(ErrorMessage = "Phone number is required")]
    [StringLength(10, MinimumLength = 10, ErrorMessage = "Phone number must be exactly 10 digits")]
    [RegularExpression(@"^09\d{8}$", ErrorMessage = "Phone number must start with 09 and be 10 digits")]
    public string PhoneNumber { get; set; } = string.Empty;

    [Required(ErrorMessage = "Gender is required")]
    [EnumDataType(typeof(Gender), ErrorMessage = "Please select a valid gender")]
    public Gender Gender { get; set; }

    [Required(ErrorMessage = "Date of birth is required")]
    [DataType(DataType.Date)]
    public DateTime DateOfBirth { get; set; }
}

/// <summary>
/// DTO for doctor login request
/// </summary>
public class LoginDoctorDto
{
    [Required(ErrorMessage = "Phone number is required")]
    [StringLength(10, MinimumLength = 10, ErrorMessage = "Phone number must be exactly 10 digits")]
    [RegularExpression(@"^09\d{8}$", ErrorMessage = "Phone number must start with 09 and be 10 digits")]
    public string PhoneNumber { get; set; } = string.Empty;

    [Required(ErrorMessage = "Password is required")]
    [StringLength(100, MinimumLength = 1, ErrorMessage = "Password is required")]
    public string Password { get; set; } = string.Empty;
}

/// <summary>
/// DTO for doctor OTP verification request
/// </summary>
public class VerifyDoctorOtpDto
{
    [Required(ErrorMessage = "Phone number is required")]
    [StringLength(10, MinimumLength = 10, ErrorMessage = "Phone number must be exactly 10 digits")]
    [RegularExpression(@"^09\d{8}$", ErrorMessage = "Phone number must start with 09 and be 10 digits")]
    public string PhoneNumber { get; set; } = string.Empty;

    [Required(ErrorMessage = "OTP is required")]
    [StringLength(6, MinimumLength = 6, ErrorMessage = "OTP must be exactly 6 digits")]
    [RegularExpression(@"^\d{6}$", ErrorMessage = "OTP must be 6 digits")]
    public string Otp { get; set; } = string.Empty;
}

/// <summary>
/// DTO for doctor forgot password request
/// </summary>
public class ForgotDoctorPasswordDto
{
    [Required(ErrorMessage = "Phone number is required")]
    [StringLength(10, MinimumLength = 10, ErrorMessage = "Phone number must be exactly 10 digits")]
    [RegularExpression(@"^09\d{8}$", ErrorMessage = "Phone number must start with 09 and be 10 digits")]
    public string PhoneNumber { get; set; } = string.Empty;
}

/// <summary>
/// DTO for doctor reset password request
/// </summary>
public class ResetDoctorPasswordDto
{
    [Required(ErrorMessage = "Phone number is required")]
    [StringLength(10, MinimumLength = 10, ErrorMessage = "Phone number must be exactly 10 digits")]
    [RegularExpression(@"^09\d{8}$", ErrorMessage = "Phone number must start with 09 and be 10 digits")]
    public string PhoneNumber { get; set; } = string.Empty;

    [Required(ErrorMessage = "OTP is required")]
    [StringLength(6, MinimumLength = 6, ErrorMessage = "OTP must be exactly 6 digits")]
    [RegularExpression(@"^\d{6}$", ErrorMessage = "OTP must be 6 digits")]
    public string Otp { get; set; } = string.Empty;

    [Required(ErrorMessage = "New password is required")]
    [StringLength(100, MinimumLength = 8, ErrorMessage = "Password must be between 8 and 100 characters")]
    public string NewPassword { get; set; } = string.Empty;
}

/// <summary>
/// DTO for doctor user data
/// </summary>
public class DoctorUserDto
{
    public int Id { get; set; }
    public string FirstName { get; set; } = string.Empty;
    public string MiddleName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string UserName { get; set; } = string.Empty;
    public string PhoneNumber { get; set; } = string.Empty;
    public Gender Gender { get; set; }
    public bool IsPhoneVerified { get; set; }
    public DateTime DateOfBirth { get; set; }
    public bool IsActive { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}

/// <summary>
/// DTO for doctor data
/// </summary>
public class DoctorDto
{
    public int UserId { get; set; }
    public DoctorUserDto User { get; set; } = null!;
    public string ProfileImageUrl { get; set; } = string.Empty;
    public bool IsProfileCompleted { get; set; }
}

/// <summary>
/// DTO for doctor registration response
/// </summary>
public class DoctorRegistrationResponseDto
{
    public string Message { get; set; } = string.Empty;
    public string Otp { get; set; } = string.Empty; // In production, don't return OTP in response
}

/// <summary>
/// DTO for authentication response
/// </summary>
public class DoctorAuthResponseDto
{
    public DoctorDto Doctor { get; set; } = null!;
    public string Message { get; set; } = string.Empty;
    public string Token { get; set; } = string.Empty;
}

/// <summary>
/// DTO for doctor forgot password response
/// </summary>
public class ForgotDoctorPasswordResponseDto
{
    public string Message { get; set; } = string.Empty;
    public string Otp { get; set; } = string.Empty; // In production, don't return OTP in response
}
