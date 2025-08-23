namespace Ivy.Core.Services;

public interface IOtpService
{
    /// <summary>
    /// Generates and stores an OTP for the specified phone number
    /// </summary>
    /// <param name="phoneNumber">The phone number to generate OTP for</param>
    /// <returns>The generated OTP</returns>
    string GenerateOtp(string phoneNumber);

    /// <summary>
    /// Verifies if the provided OTP is valid for the phone number
    /// </summary>
    /// <param name="phoneNumber">The phone number</param>
    /// <param name="otp">The OTP to verify</param>
    /// <returns>True if valid, false otherwise</returns>
    bool VerifyOtp(string phoneNumber, string otp);

    /// <summary>
    /// Removes OTP for the specified phone number
    /// </summary>
    /// <param name="phoneNumber">The phone number</param>
    void ClearOtp(string phoneNumber);

    /// <summary>
    /// Checks if an OTP exists for the specified phone number
    /// </summary>
    /// <param name="phoneNumber">The phone number</param>
    /// <returns>True if OTP exists, false otherwise</returns>
    bool HasOtp(string phoneNumber);
}
