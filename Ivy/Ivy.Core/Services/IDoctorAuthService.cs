using Ivy.Core.Entities;
using IvyBackend;

namespace Ivy.Core.Services;

public interface IDoctorAuthService
{
    /// <summary>
    /// Register a new doctor account
    /// </summary>
    /// <param name="firstName">Doctor's first name</param>
    /// <param name="middleName">Doctor's middle name</param>
    /// <param name="lastName">Doctor's last name</param>
    /// <param name="userName">Unique username</param>
    /// <param name="password">Password</param>
    /// <param name="phoneNumber">Phone number for OTP verification</param>
    /// <param name="gender">Doctor's gender</param>
    /// <param name="dateOfBirth">Date of birth</param>
    /// <returns>Result with OTP for verification</returns>
    Task<Result<string>> RegisterAsync(
        string firstName,
        string middleName,
        string lastName,
        string userName,
        string password,
        string phoneNumber,
        Gender gender,
        DateTime dateOfBirth
    );

    /// <summary>
    /// Verify OTP and activate doctor account
    /// </summary>
    /// <param name="phoneNumber">Doctor's phone number</param>
    /// <param name="otp">OTP code</param>
    /// <returns>Result with doctor data</returns>
    Task<Result<Doctor>> VerifyOtpAsync(string phoneNumber, string otp);

    /// <summary>
    /// Login doctor with phone number and password
    /// </summary>
    /// <param name="phoneNumber">Doctor's phone number</param>
    /// <param name="password">Password</param>
    /// <returns>Result with doctor data</returns>
    Task<Result<Doctor>> LoginAsync(string phoneNumber, string password);

    /// <summary>
    /// Check if phone number is already registered
    /// </summary>
    /// <param name="phoneNumber">Phone number to check</param>
    /// <returns>Boolean indicating if phone exists</returns>
    Task<Result<bool>> PhoneExistsAsync(string phoneNumber);

    /// <summary>
    /// Check if username is already taken
    /// </summary>
    /// <param name="userName">Username to check</param>
    /// <returns>Boolean indicating if username exists</returns>
    Task<Result<bool>> UserNameExistsAsync(string userName);

    /// <summary>
    /// Initiate forgot password process by sending OTP
    /// </summary>
    /// <param name="phoneNumber">Doctor's registered phone number</param>
    /// <returns>Result with OTP for password reset</returns>
    Task<Result<string>> ForgotPasswordAsync(string phoneNumber);

    /// <summary>
    /// Reset password using OTP verification
    /// </summary>
    /// <param name="phoneNumber">Doctor's phone number</param>
    /// <param name="otp">OTP code</param>
    /// <param name="newPassword">New password</param>
    /// <returns>Result indicating password reset success</returns>
    Task<Result<bool>> ResetPasswordAsync(string phoneNumber, string otp, string newPassword);

    /// <summary>
    /// Get doctor by user ID
    /// </summary>
    /// <param name="userId">User ID</param>
    /// <returns>Result with doctor data</returns>
    Task<Result<Doctor>> GetDoctorByUserIdAsync(int userId);
}