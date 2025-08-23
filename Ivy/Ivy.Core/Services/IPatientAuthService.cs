using Ivy.Core.Entities;
using IvyBackend;

namespace Ivy.Core.Services;

public interface IPatientAuthService
{
    /// <summary>
    /// Registers a new patient and sends OTP for phone verification
    /// </summary>
    /// <param name="firstName">First name</param>
    /// <param name="middleName">Middle name</param>
    /// <param name="lastName">Last name</param>
    /// <param name="userName">Username</param>
    /// <param name="password">Password</param>
    /// <param name="phoneNumber">Phone number</param>
    /// <param name="gender">Gender</param>
    /// <param name="dateOfBirth">Date of birth</param>
    /// <returns>Result with registration status and OTP</returns>
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
    /// Verifies OTP and activates the patient account
    /// </summary>
    /// <param name="phoneNumber">Phone number</param>
    /// <param name="otp">OTP to verify</param>
    /// <returns>Result with verification status and patient data</returns>
    Task<Result<Patient>> VerifyOtpAsync(string phoneNumber, string otp);

    /// <summary>
    /// Authenticates patient with phone number and password
    /// </summary>
    /// <param name="phoneNumber">Phone number</param>
    /// <param name="password">Password</param>
    /// <returns>Result with login status and patient data</returns>
    Task<Result<Patient>> LoginAsync(string phoneNumber, string password);

    /// <summary>
    /// Checks if a phone number is already registered
    /// </summary>
    /// <param name="phoneNumber">Phone number to check</param>
    /// <returns>True if phone number exists</returns>
    Task<Result<bool>> PhoneExistsAsync(string phoneNumber);

    /// <summary>
    /// Checks if a username is already taken
    /// </summary>
    /// <param name="userName">Username to check</param>
    /// <returns>True if username exists</returns>
    Task<Result<bool>> UserNameExistsAsync(string userName);

    /// <summary>
    /// Initiates forgot password process by sending OTP to registered phone number
    /// </summary>
    /// <param name="phoneNumber">Phone number to send reset OTP</param>
    /// <returns>Result with OTP generation status</returns>
    Task<Result<string>> ForgotPasswordAsync(string phoneNumber);

    /// <summary>
    /// Resets password using OTP verification
    /// </summary>
    /// <param name="phoneNumber">Phone number</param>
    /// <param name="otp">OTP for verification</param>
    /// <param name="newPassword">New password to set</param>
    /// <returns>Result with password reset status</returns>
    Task<Result<bool>> ResetPasswordAsync(string phoneNumber, string otp, string newPassword);

    /// <summary>
    /// Gets patient profile by user ID
    /// </summary>
    /// <param name="userId">User ID to get patient profile for</param>
    /// <returns>Result with patient data</returns>
    Task<Result<Patient>> GetPatientByUserIdAsync(int userId);
}
