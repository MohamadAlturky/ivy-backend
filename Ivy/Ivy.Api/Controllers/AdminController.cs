using Ivy.Api.DTOs;
using Ivy.Api.Services;
using Ivy.Core.Jwt;
using Ivy.Core.Services;
using IvyBackend;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace Ivy.Api.Controllers;

[Route("api/admin")]
public class AdminController : BaseController
{
    private readonly IAdminService _adminService;
    private readonly JwtService _jwtService;

    public AdminController(
        IAdminService adminService,
        JwtService jwtService,
        IApiResponseRepresenter responseRepresenter,
        ILogger<AdminController> logger
    )
        : base(responseRepresenter, logger)
    {
        _adminService = adminService;
        _jwtService = jwtService;
    }

    /// <summary>
    /// Admin login with email and password
    /// </summary>
    /// <param name="loginDto">Admin login credentials</param>
    /// <returns>Login result with admin data and JWT token</returns>
    [HttpPost("login")]
    public async Task<ActionResult<ApiResponse<AdminAuthResponseDto>>> Login(
        [FromBody] AdminLoginDto loginDto
    )
    {
        try
        {
            if (!IsModelValid())
            {
                return HandleValidationError<AdminAuthResponseDto>();
            }

            var result = await _adminService.LoginAsync(loginDto.Email, loginDto.Password);

            if (result.Success)
            {
                var adminDto = MapToAdminDto(result.Data);
                var token = _jwtService.GenerateToken(
                    result.Data.Id,
                    result.Data.Email,
                    "admin"
                );
                var responseDto = new AdminAuthResponseDto
                {
                    Admin = adminDto,
                    Message = "Admin login successful.",
                    Token = token,
                };

                var mappedResult = Result<AdminAuthResponseDto>.Ok(result.MessageCode, responseDto);
                return HandleResult(mappedResult);
            }

            var failedResult = Result<AdminAuthResponseDto>.Error(result.MessageCode, default!);
            return HandleResult(failedResult);
        }
        catch (Exception ex)
        {
            return HandleInternalError<AdminAuthResponseDto>(ex, "logging in admin");
        }
    }

    /// <summary>
    /// Get current authenticated admin's profile
    /// </summary>
    /// <returns>Current admin profile data</returns>
    [HttpGet("profile")]
    [Authorize(Roles = "admin")]
    public async Task<ActionResult<ApiResponse<AdminDto>>> GetProfile()
    {
        try
        {
            // Get admin ID from JWT token
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrWhiteSpace(userIdClaim) || !int.TryParse(userIdClaim, out int adminId))
            {
                var invalidTokenResult = Result<AdminDto>.Error("INVALID_TOKEN", default!);
                return HandleResult(invalidTokenResult);
            }

            var result = await _adminService.GetProfileAsync(adminId);

            if (result.Success)
            {
                var adminDto = MapToAdminDto(result.Data);
                var mappedResult = Result<AdminDto>.Ok(result.MessageCode, adminDto);
                return HandleResult(mappedResult);
            }

            var failedResult = Result<AdminDto>.Error(result.MessageCode, default!);
            return HandleResult(failedResult);
        }
        catch (Exception ex)
        {
            return HandleInternalError<AdminDto>(ex, "getting admin profile");
        }
    }

    /// <summary>
    /// Update current authenticated admin's profile
    /// </summary>
    /// <param name="updateProfileDto">Profile update data</param>
    /// <returns>Updated admin profile data</returns>
    // [HttpPut("profile")]
    // [Authorize(Roles = "admin")]
    // public async Task<ActionResult<ApiResponse<AdminDto>>> UpdateProfile(
    //     [FromBody] UpdateAdminProfileWithPasswordDto updateProfileDto
    // )
    // {
    //     try
    //     {
    //         if (!IsModelValid())
    //         {
    //             return HandleValidationError<AdminDto>();
    //         }

    //         // Get admin ID from JWT token
    //         var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
    //         if (string.IsNullOrWhiteSpace(userIdClaim) || !int.TryParse(userIdClaim, out int adminId))
    //         {
    //             var invalidTokenResult = Result<AdminDto>.Error("INVALID_TOKEN", default!);
    //             return HandleResult(invalidTokenResult);
    //         }

    //         // Validate password fields if password change is requested
    //         if (!string.IsNullOrWhiteSpace(updateProfileDto.NewPassword))
    //         {
    //             if (string.IsNullOrWhiteSpace(updateProfileDto.CurrentPassword))
    //             {
    //                 var passwordRequiredResult = Result<AdminDto>.Error("CURRENT_PASSWORD_REQUIRED", default!);
    //                 return HandleResult(passwordRequiredResult);
    //             }

    //             if (updateProfileDto.NewPassword != updateProfileDto.ConfirmPassword)
    //             {
    //                 var passwordMismatchResult = Result<AdminDto>.Error("PASSWORD_CONFIRMATION_MISMATCH", default!);
    //                 return HandleResult(passwordMismatchResult);
    //             }
    //         }

    //         var result = await _adminService.UpdateProfileAsync(
    //             adminId,
    //             updateProfileDto.Email,
    //             updateProfileDto.CurrentPassword,
    //             updateProfileDto.NewPassword
    //         );

    //         if (result.Success)
    //         {
    //             var adminDto = MapToAdminDto(result.Data);
    //             var mappedResult = Result<AdminDto>.Ok(result.MessageCode, adminDto);
    //             return HandleResult(mappedResult);
    //         }

    //         var failedResult = Result<AdminDto>.Error(result.MessageCode, default!);
    //         return HandleResult(failedResult);
    //     }
    //     catch (Exception ex)
    //     {
    //         return HandleInternalError<AdminDto>(ex, "updating admin profile");
    //     }
    // }

    /// <summary>
    /// Change admin password
    /// </summary>
    /// <param name="changePasswordDto">Password change data</param>
    /// <returns>Result indicating password change success</returns>
    [HttpPut("change-password")]
    [Authorize(Roles = "admin")]
    public async Task<ActionResult<ApiResponse<string>>> ChangePassword(
        [FromBody] ChangeAdminPasswordDto changePasswordDto
    )
    {
        try
        {
            if (!IsModelValid())
            {
                return HandleValidationError<string>();
            }

            // Get admin ID from JWT token
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrWhiteSpace(userIdClaim) || !int.TryParse(userIdClaim, out int adminId))
            {
                var invalidTokenResult = Result<string>.Error("INVALID_TOKEN", default!);
                return HandleResult(invalidTokenResult);
            }

            if (changePasswordDto.NewPassword != changePasswordDto.ConfirmPassword)
            {
                var passwordMismatchResult = Result<string>.Error("PASSWORD_CONFIRMATION_MISMATCH", default!);
                return HandleResult(passwordMismatchResult);
            }

            var result = await _adminService.ChangePasswordAsync(
                adminId,
                changePasswordDto.CurrentPassword,
                changePasswordDto.NewPassword
            );

            if (result.Success)
            {
                var message = "Password changed successfully.";
                var mappedResult = Result<string>.Ok(result.MessageCode, message);
                return HandleResult(mappedResult);
            }

            var failedResult = Result<string>.Error(result.MessageCode, default!);
            return HandleResult(failedResult);
        }
        catch (Exception ex)
        {
            return HandleInternalError<string>(ex, "changing admin password");
        }
    }

    // /// <summary>
    // /// Check if an email is already registered
    // /// </summary>
    // /// <param name="email">Email to check</param>
    // /// <returns>Boolean indicating if email exists</returns>
    // [HttpGet("email-exists")]
    // public async Task<ActionResult<ApiResponse<bool>>> CheckEmailExists(
    //     [FromQuery] string email
    // )
    // {
    //     try
    //     {
    //         if (string.IsNullOrWhiteSpace(email))
    //         {
    //             var invalidResult = Result<bool>.Error("INVALID_EMAIL", false);
    //             return HandleResult(invalidResult);
    //         }

    //         var result = await _adminService.EmailExistsAsync(email);
    //         return HandleResult(result);
    //     }
    //     catch (Exception ex)
    //     {
    //         return HandleInternalError<bool>(ex, "checking email existence");
    //     }
    // }

    private static AdminDto MapToAdminDto(Core.Entities.Admin admin)
    {
        return new AdminDto
        {
            Id = admin.Id,
            Email = admin.Email,
            IsActive = admin.IsActive,
            CreatedAt = admin.CreatedAt,
            UpdatedAt = admin.UpdatedAt,
        };
    }
}
