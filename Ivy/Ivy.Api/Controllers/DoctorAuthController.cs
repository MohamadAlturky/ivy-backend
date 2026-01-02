using Ivy.Api.DTOs;
using Ivy.Api.Services;
using Ivy.Core.Jwt;
using Ivy.Core.Services;
using IvyBackend;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace Ivy.Api.Controllers;

[Route("api/doctor-auth")]
public class DoctorAuthController : BaseController
{
    private readonly IDoctorAuthService _doctorAuthService;
    private readonly JwtService _jwtService;

    public DoctorAuthController(
        IDoctorAuthService doctorAuthService,
        JwtService jwtService,
        IApiResponseRepresenter responseRepresenter,
        ILogger<DoctorAuthController> logger
    )
        : base(responseRepresenter, logger)
    {
        _doctorAuthService = doctorAuthService;
        _jwtService = jwtService;
    }

    /// <summary>
    /// Register a new doctor account
    /// </summary>
    /// <param name="registerDto">Doctor registration data</param>
    /// <returns>Registration result with OTP</returns>
    [HttpPost("register")]
    public async Task<IActionResult> Register(
        [FromBody] RegisterDoctorDto registerDto
    )
    {
        try
        {
            if (!IsModelValid())
            {
                return HandleValidationError<DoctorRegistrationResponseDto>();
            }

            var result = await _doctorAuthService.RegisterAsync(
                registerDto.FirstName,
                registerDto.MiddleName,
                registerDto.LastName,
                registerDto.FirstName + ' ' + registerDto.MiddleName + ' ' + registerDto.LastName,
                registerDto.Password,
                registerDto.PhoneNumber,
                registerDto.Gender,
                registerDto.DateOfBirth
            );

            if (result.Success)
            {
                var responseDto = new DoctorRegistrationResponseDto
                {
                    Message =
                        "Doctor registered successfully. Please verify your phone number with the OTP sent.",
                    Otp = result.Data, // In production, don't return OTP in response, send via SMS instead
                };

                var mappedResult = Result<DoctorRegistrationResponseDto>.Ok(
                    result.MessageCode,
                    responseDto
                );
                return HandleResult(mappedResult);
            }

            var failedResult = Result<DoctorRegistrationResponseDto>.Error(result.MessageCode, default!);
            return HandleResult(failedResult);
        }
        catch (Exception ex)
        {
            return HandleInternalError<DoctorRegistrationResponseDto>(ex, "registering doctor");
        }
    }

    /// <summary>
    /// Verify OTP and activate doctor account
    /// </summary>
    /// <param name="verifyOtpDto">OTP verification data</param>
    /// <returns>Verification result with doctor data</returns>
    [HttpPost("verify-otp")]
    public async Task<IActionResult> VerifyOtp(
        [FromBody] VerifyDoctorOtpDto verifyOtpDto
    )
    {
        try
        {
            if (!IsModelValid())
            {
                return HandleValidationError<DoctorAuthResponseDto>();
            }

            var result = await _doctorAuthService.VerifyOtpAsync(
                verifyOtpDto.PhoneNumber,
                verifyOtpDto.Otp
            );

            if (result.Success)
            {
                var doctorDto = MapToDoctorDto(result.Data);
                var token = _jwtService.GenerateToken(
                    result.Data.User.Id,
                    result.Data.User.UserName,
                    "doctor"
                );
                var responseDto = new DoctorAuthResponseDto
                {
                    Doctor = doctorDto,
                    Message = "Phone number verified successfully. Account is now active.",
                    Token = token,
                };

                var mappedResult = Result<DoctorAuthResponseDto>.Ok(result.MessageCode, responseDto);
                return HandleResult(mappedResult);
            }

            var failedResult = Result<DoctorAuthResponseDto>.Error(result.MessageCode, default!);
            return HandleResult(failedResult);
        }
        catch (Exception ex)
        {
            return HandleInternalError<DoctorAuthResponseDto>(ex, "verifying OTP");
        }
    }

    /// <summary>
    /// Login doctor with phone number and password
    /// </summary>
    /// <param name="loginDto">Login credentials</param>
    /// <returns>Login result with doctor data</returns>
    [HttpPost("login")]
    public async Task<IActionResult> Login(
        [FromBody] LoginDoctorDto loginDto
    )
    {
        try
        {
            if (!IsModelValid())
            {
                return HandleValidationError<DoctorAuthResponseDto>();
            }

            var result = await _doctorAuthService.LoginAsync(
                loginDto.PhoneNumber,
                loginDto.Password
            );

            if (result.Success)
            {
                var doctorDto = MapToDoctorDto(result.Data);
                var token = _jwtService.GenerateToken(
                    result.Data.User.Id,
                    result.Data.User.UserName,
                    "doctor"
                );
                var responseDto = new DoctorAuthResponseDto
                {
                    Doctor = doctorDto,
                    Message = "Login successful.",
                    Token = token,
                };

                var mappedResult = Result<DoctorAuthResponseDto>.Ok(result.MessageCode, responseDto);
                return HandleResult(mappedResult);
            }

            var failedResult = Result<DoctorAuthResponseDto>.Error(result.MessageCode, default!);
            return HandleResult(failedResult);
        }
        catch (Exception ex)
        {
            return HandleInternalError<DoctorAuthResponseDto>(ex, "logging in doctor");
        }
    }

    /// <summary>
    /// Initiate forgot password process by sending OTP to registered phone number
    /// </summary>
    /// <param name="forgotPasswordDto">Forgot password data with phone number</param>
    /// <returns>Result with OTP for password reset</returns>
    [HttpPost("forgot-password")]
    public async Task<IActionResult> ForgotPassword(
        [FromBody] ForgotDoctorPasswordDto forgotPasswordDto
    )
    {
        try
        {
            if (!IsModelValid())
            {
                return HandleValidationError<ForgotDoctorPasswordResponseDto>();
            }

            var result = await _doctorAuthService.ForgotPasswordAsync(forgotPasswordDto.PhoneNumber);

            if (result.Success)
            {
                var responseDto = new ForgotDoctorPasswordResponseDto
                {
                    Message = "Password reset OTP has been sent to your phone number.",
                    Otp = result.Data, // In production, don't return OTP in response, send via SMS instead
                };

                var mappedResult = Result<ForgotDoctorPasswordResponseDto>.Ok(
                    result.MessageCode,
                    responseDto
                );
                return HandleResult(mappedResult);
            }

            var failedResult = Result<ForgotDoctorPasswordResponseDto>.Error(result.MessageCode, default!);
            return HandleResult(failedResult);
        }
        catch (Exception ex)
        {
            return HandleInternalError<ForgotDoctorPasswordResponseDto>(ex, "processing forgot password request");
        }
    }

    /// <summary>
    /// Reset password using OTP verification
    /// </summary>
    /// <param name="resetPasswordDto">Password reset data with OTP and new password</param>
    /// <returns>Result indicating password reset success</returns>
    [HttpPost("reset-password")]
    public async Task<IActionResult> ResetPassword(
        [FromBody] ResetDoctorPasswordDto resetPasswordDto
    )
    {
        try
        {
            if (!IsModelValid())
            {
                return HandleValidationError<string>();
            }

            var result = await _doctorAuthService.ResetPasswordAsync(
                resetPasswordDto.PhoneNumber,
                resetPasswordDto.Otp,
                resetPasswordDto.NewPassword
            );

            if (result.Success)
            {
                var message = "Password has been reset successfully. You can now login with your new password.";
                var mappedResult = Result<string>.Ok(result.MessageCode, message);
                return HandleResult(mappedResult);
            }

            var failedResult = Result<string>.Error(result.MessageCode, default!);
            return HandleResult(failedResult);
        }
        catch (Exception ex)
        {
            return HandleInternalError<string>(ex, "resetting password");
        }
    }

    /// <summary>
    /// Get current authenticated doctor's profile
    /// </summary>
    /// <returns>Current doctor profile data</returns>
    [HttpGet("my-profile")]
    [Authorize]
    public async Task<IActionResult> GetMyProfile()
    {
        try
        {
            // Get user ID from JWT token
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrWhiteSpace(userIdClaim) || !int.TryParse(userIdClaim, out int userId))
            {
                var invalidTokenResult = Result<DoctorDto>.Error("INVALID_TOKEN", default!);
                return HandleResult(invalidTokenResult);
            }

            var result = await _doctorAuthService.GetDoctorByUserIdAsync(userId);

            if (result.Success)
            {
                var doctorDto = MapToDoctorDto(result.Data);
                var mappedResult = Result<DoctorDto>.Ok(result.MessageCode, doctorDto);
                return HandleResult(mappedResult);
            }

            var failedResult = Result<DoctorDto>.Error(result.MessageCode, default!);
            return HandleResult(failedResult);
        }
        catch (Exception ex)
        {
            return HandleInternalError<DoctorDto>(ex, "getting doctor profile");
        }
    }

    private static DoctorDto MapToDoctorDto(Core.Entities.Doctor doctor)
    {
        return new DoctorDto
        {
            UserId = doctor.UserId,
            User = new DoctorUserDto
            {
                Id = doctor.User.Id,
                FirstName = doctor.User.FirstName,
                MiddleName = doctor.User.MiddleName,
                LastName = doctor.User.LastName,
                UserName = doctor.User.UserName,
                PhoneNumber = doctor.User.PhoneNumber,
                Gender = doctor.User.Gender,
                IsPhoneVerified = doctor.User.IsPhoneVerified,
                DateOfBirth = doctor.User.DateOfBirth,
                IsActive = doctor.User.IsActive,
                CreatedAt = doctor.User.CreatedAt,
                UpdatedAt = doctor.User.UpdatedAt,
            },
            ProfileImageUrl = doctor.ProfileImageUrl,
        };
    }
}
