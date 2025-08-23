using Ivy.Api.DTOs;
using Ivy.Api.Services;
using Ivy.Core.Jwt;
using Ivy.Core.Services;
using IvyBackend;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace Ivy.Api.Controllers;

[Route("api/patient-auth")]
public class PatientAuthController : BaseController
{
    private readonly IPatientAuthService _patientAuthService;
    private readonly JwtService _jwtService;

    public PatientAuthController(
        IPatientAuthService patientAuthService,
        JwtService jwtService,
        IApiResponseRepresenter responseRepresenter,
        ILogger<PatientAuthController> logger
    )
        : base(responseRepresenter, logger)
    {
        _patientAuthService = patientAuthService;
        _jwtService = jwtService;
    }

    /// <summary>
    /// Register a new patient account
    /// </summary>
    /// <param name="registerDto">Patient registration data</param>
    /// <returns>Registration result with OTP</returns>
    [HttpPost("register")]
    public async Task<ActionResult<ApiResponse<RegistrationResponseDto>>> Register(
        [FromBody] RegisterPatientDto registerDto
    )
    {
        try
        {
            if (!IsModelValid())
            {
                return HandleValidationError<RegistrationResponseDto>();
            }

            var result = await _patientAuthService.RegisterAsync(
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
                var responseDto = new RegistrationResponseDto
                {
                    Message =
                        "Patient registered successfully. Please verify your phone number with the OTP sent.",
                    Otp = result.Data, // In production, don't return OTP in response, send via SMS instead
                };

                var mappedResult = Result<RegistrationResponseDto>.Ok(
                    result.MessageCode,
                    responseDto
                );
                return HandleResult(mappedResult);
            }

            var failedResult = Result<RegistrationResponseDto>.Error(result.MessageCode, default!);
            return HandleResult(failedResult);
        }
        catch (Exception ex)
        {
            return HandleInternalError<RegistrationResponseDto>(ex, "registering patient");
        }
    }

    /// <summary>
    /// Verify OTP and activate patient account
    /// </summary>
    /// <param name="verifyOtpDto">OTP verification data</param>
    /// <returns>Verification result with patient data</returns>
    [HttpPost("verify-otp")]
    public async Task<ActionResult<ApiResponse<AuthResponseDto>>> VerifyOtp(
        [FromBody] VerifyOtpDto verifyOtpDto
    )
    {
        try
        {
            if (!IsModelValid())
            {
                return HandleValidationError<AuthResponseDto>();
            }

            var result = await _patientAuthService.VerifyOtpAsync(
                verifyOtpDto.PhoneNumber,
                verifyOtpDto.Otp
            );

            if (result.Success)
            {
                var patientDto = MapToPatientDto(result.Data);
                var token = _jwtService.GenerateToken(
                    result.Data.User.Id,
                    result.Data.User.UserName
                );
                var responseDto = new AuthResponseDto
                {
                    Patient = patientDto,
                    Message = "Phone number verified successfully. Account is now active.",
                    Token = token,
                };

                var mappedResult = Result<AuthResponseDto>.Ok(result.MessageCode, responseDto);
                return HandleResult(mappedResult);
            }

            var failedResult = Result<AuthResponseDto>.Error(result.MessageCode, default!);
            return HandleResult(failedResult);
        }
        catch (Exception ex)
        {
            return HandleInternalError<AuthResponseDto>(ex, "verifying OTP");
        }
    }

    /// <summary>
    /// Login patient with phone number and password
    /// </summary>
    /// <param name="loginDto">Login credentials</param>
    /// <returns>Login result with patient data</returns>
    [HttpPost("login")]
    public async Task<ActionResult<ApiResponse<AuthResponseDto>>> Login(
        [FromBody] LoginPatientDto loginDto
    )
    {
        try
        {
            if (!IsModelValid())
            {
                return HandleValidationError<AuthResponseDto>();
            }

            var result = await _patientAuthService.LoginAsync(
                loginDto.PhoneNumber,
                loginDto.Password
            );

            if (result.Success)
            {
                var patientDto = MapToPatientDto(result.Data);
                var token = _jwtService.GenerateToken(
                    result.Data.User.Id,
                    result.Data.User.UserName
                );
                var responseDto = new AuthResponseDto
                {
                    Patient = patientDto,
                    Message = "Login successful.",
                    Token = token,
                };

                var mappedResult = Result<AuthResponseDto>.Ok(result.MessageCode, responseDto);
                return HandleResult(mappedResult);
            }

            var failedResult = Result<AuthResponseDto>.Error(result.MessageCode, default!);
            return HandleResult(failedResult);
        }
        catch (Exception ex)
        {
            return HandleInternalError<AuthResponseDto>(ex, "logging in patient");
        }
    }

    // /// <summary>
    // /// Check if phone number is already registered
    // /// </summary>
    // /// <param name="phoneNumber">Phone number to check</param>
    // /// <returns>Boolean indicating if phone exists</returns>
    // [HttpGet("phone-exists")]
    // public async Task<ActionResult<ApiResponse<bool>>> CheckPhoneExists(
    //     [FromQuery] string phoneNumber
    // )
    // {
    //     try
    //     {
    //         if (string.IsNullOrWhiteSpace(phoneNumber))
    //         {
    //             var invalidResult = Result<bool>.Error("INVALID_PHONE_NUMBER", false);
    //             return HandleResult(invalidResult);
    //         }

    //         var result = await _patientAuthService.PhoneExistsAsync(phoneNumber);
    //         return HandleResult(result);
    //     }
    //     catch (Exception ex)
    //     {
    //         return HandleInternalError<bool>(ex, "checking phone existence");
    //     }
    // }

    // /// <summary>
    // /// Check if username is already taken
    // /// </summary>
    // /// <param name="userName">Username to check</param>
    // /// <returns>Boolean indicating if username exists</returns>
    // [HttpGet("username-exists")]
    // public async Task<ActionResult<ApiResponse<bool>>> CheckUserNameExists(
    //     [FromQuery] string userName
    // )
    // {
    //     try
    //     {
    //         if (string.IsNullOrWhiteSpace(userName))
    //         {
    //             var invalidResult = Result<bool>.Error("INVALID_USERNAME", false);
    //             return HandleResult(invalidResult);
    //         }

    //         var result = await _patientAuthService.UserNameExistsAsync(userName);
    //         return HandleResult(result);
    //     }
    //     catch (Exception ex)
    //     {
    //         return HandleInternalError<bool>(ex, "checking username existence");
    //     }
    // }

    /// <summary>
    /// Initiate forgot password process by sending OTP to registered phone number
    /// </summary>
    /// <param name="forgotPasswordDto">Forgot password data with phone number</param>
    /// <returns>Result with OTP for password reset</returns>
    [HttpPost("forgot-password")]
    public async Task<ActionResult<ApiResponse<ForgotPasswordResponseDto>>> ForgotPassword(
        [FromBody] ForgotPasswordDto forgotPasswordDto
    )
    {
        try
        {
            if (!IsModelValid())
            {
                return HandleValidationError<ForgotPasswordResponseDto>();
            }

            var result = await _patientAuthService.ForgotPasswordAsync(forgotPasswordDto.PhoneNumber);

            if (result.Success)
            {
                var responseDto = new ForgotPasswordResponseDto
                {
                    Message = "Password reset OTP has been sent to your phone number.",
                    Otp = result.Data, // In production, don't return OTP in response, send via SMS instead
                };

                var mappedResult = Result<ForgotPasswordResponseDto>.Ok(
                    result.MessageCode,
                    responseDto
                );
                return HandleResult(mappedResult);
            }

            var failedResult = Result<ForgotPasswordResponseDto>.Error(result.MessageCode, default!);
            return HandleResult(failedResult);
        }
        catch (Exception ex)
        {
            return HandleInternalError<ForgotPasswordResponseDto>(ex, "processing forgot password request");
        }
    }

    /// <summary>
    /// Reset password using OTP verification
    /// </summary>
    /// <param name="resetPasswordDto">Password reset data with OTP and new password</param>
    /// <returns>Result indicating password reset success</returns>
    [HttpPost("reset-password")]
    public async Task<ActionResult<ApiResponse<string>>> ResetPassword(
        [FromBody] ResetPasswordDto resetPasswordDto
    )
    {
        try
        {
            if (!IsModelValid())
            {
                return HandleValidationError<string>();
            }

            var result = await _patientAuthService.ResetPasswordAsync(
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
    /// Get current authenticated patient's profile
    /// </summary>
    /// <returns>Current patient profile data</returns>
    [HttpGet("my-profile")]
    [Authorize]
    public async Task<ActionResult<ApiResponse<PatientDto>>> GetMyProfile()
    {
        try
        {
            // Get user ID from JWT token
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrWhiteSpace(userIdClaim) || !int.TryParse(userIdClaim, out int userId))
            {
                var invalidTokenResult = Result<PatientDto>.Error("INVALID_TOKEN", default!);
                return HandleResult(invalidTokenResult);
            }

            var result = await _patientAuthService.GetPatientByUserIdAsync(userId);

            if (result.Success)
            {
                var patientDto = MapToPatientDto(result.Data);
                var mappedResult = Result<PatientDto>.Ok(result.MessageCode, patientDto);
                return HandleResult(mappedResult);
            }

            var failedResult = Result<PatientDto>.Error(result.MessageCode, default!);
            return HandleResult(failedResult);
        }
        catch (Exception ex)
        {
            return HandleInternalError<PatientDto>(ex, "getting patient profile");
        }
    }

    private static PatientDto MapToPatientDto(Core.Entities.Patient patient)
    {
        return new PatientDto
        {
            UserId = patient.UserId,
            User = new UserDto
            {
                Id = patient.User.Id,
                FirstName = patient.User.FirstName,
                MiddleName = patient.User.MiddleName,
                LastName = patient.User.LastName,
                UserName = patient.User.UserName,
                PhoneNumber = patient.User.PhoneNumber,
                Gender = patient.User.Gender,
                IsPhoneVerified = patient.User.IsPhoneVerified,
                DateOfBirth = patient.User.DateOfBirth,
                IsActive = patient.User.IsActive,
                CreatedAt = patient.User.CreatedAt,
                UpdatedAt = patient.User.UpdatedAt,
            },
        };
    }
}
