using Ivy.Core.DataContext;
using Ivy.Core.Entities;
using IvyBackend;
using Microsoft.EntityFrameworkCore;

namespace Ivy.Core.Services;

public class DoctorAuthService : IDoctorAuthService
{
    private readonly IvyContext _context;
    private readonly IOtpService _otpService;

    public DoctorAuthService(IvyContext context, IOtpService otpService)
    {
        _context = context;
        _otpService = otpService;
    }

    public async Task<Result<string>> RegisterAsync(
        string firstName,
        string middleName,
        string lastName,
        string userName,
        string password,
        string phoneNumber,
        Gender gender,
        DateTime dateOfBirth
    )
    {
        try
        {
            // Validate required fields
            if (
                string.IsNullOrWhiteSpace(firstName)
                || string.IsNullOrWhiteSpace(lastName)
                || string.IsNullOrWhiteSpace(userName)
                || string.IsNullOrWhiteSpace(password)
                || string.IsNullOrWhiteSpace(phoneNumber)
            )
            {
                return Result<string>.Error("INVALID_REGISTRATION_DATA", string.Empty);
            }

            if (!PhoneNumberValidator.ValidatePhoneNumber(phoneNumber))
            {
                return Result<string>.Error("INVALID_PHONE_NUMBER", string.Empty);
            }

            // Check if phone number already exists
            var phoneExists = await _context.Users.AnyAsync(u =>
                u.PhoneNumber == phoneNumber && !u.IsDeleted
            );

            if (phoneExists)
            {
                return Result<string>.Error("PHONE_NUMBER_ALREADY_EXISTS", string.Empty);
            }

            // Check if username already exists
            var userNameExists = await _context.Users.AnyAsync(u =>
                u.UserName == userName && !u.IsDeleted
            );

            if (userNameExists)
            {
                return Result<string>.Error("USERNAME_ALREADY_EXISTS", string.Empty);
            }

            // Hash password (in production, use proper password hashing like BCrypt)
            var hashedPassword = HashPassword(password);

            // Create user entity
            var user = new User
            {
                FirstName = firstName,
                MiddleName = middleName,
                LastName = lastName,
                UserName = userName,
                Password = hashedPassword,
                PhoneNumber = phoneNumber,
                Gender = gender,
                DateOfBirth = dateOfBirth,
                IsPhoneVerified = false,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow,
                IsActive = false, // Will be activated after OTP verification
            };

            _context.Users.Add(user);
            await _context.SaveChangesAsync();

            // Create doctor entity
            var doctor = new Doctor
            {
                UserId = user.Id,
                User = user,
                ProfileImageUrl = string.Empty,
            };

            _context.Doctors.Add(doctor);
            await _context.SaveChangesAsync();

            // Generate and send OTP
            var otp = _otpService.GenerateOtp(phoneNumber);

            return Result<string>.Ok("DOCTOR_REGISTERED_SUCCESS", otp);
        }
        catch (Exception _)
        {
            return Result<string>.Error("DOCTOR_REGISTRATION_FAILED", string.Empty);
        }
    }

    public async Task<Result<Doctor>> VerifyOtpAsync(string phoneNumber, string otp)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(phoneNumber) || string.IsNullOrWhiteSpace(otp))
            {
                return Result<Doctor>.Error("INVALID_OTP_DATA", null!);
            }

            // Verify OTP
            if (!_otpService.VerifyOtp(phoneNumber, otp))
            {
                return Result<Doctor>.Error("INVALID_OTP", null!);
            }

            // Find user by phone number
            var user = await _context.Users.FirstOrDefaultAsync(u =>
                u.PhoneNumber == phoneNumber && !u.IsDeleted
            );

            if (user == null)
            {
                return Result<Doctor>.Error("USER_NOT_FOUND", null!);
            }

            // Update user verification status
            user.IsPhoneVerified = true;
            user.IsActive = true;
            user.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            // Clear OTP
            _otpService.ClearOtp(phoneNumber);

            // Get doctor with user data
            var doctor = await _context
                .Doctors.Include(d => d.User)
                .FirstOrDefaultAsync(d => d.UserId == user.Id);

            if (doctor == null)
            {
                return Result<Doctor>.Error("DOCTOR_NOT_FOUND", null!);
            }

            return Result<Doctor>.Ok("OTP_VERIFIED_SUCCESS", doctor);
        }
        catch (Exception _)
        {
            return Result<Doctor>.Error("OTP_VERIFICATION_FAILED", null!);
        }
    }

    public async Task<Result<Doctor>> LoginAsync(string phoneNumber, string password)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(phoneNumber) || string.IsNullOrWhiteSpace(password))
            {
                return Result<Doctor>.Error("INVALID_LOGIN_DATA", null!);
            }

            // Find user by phone number
            var user = await _context.Users.FirstOrDefaultAsync(u =>
                u.PhoneNumber == phoneNumber && !u.IsDeleted
            );

            if (user == null)
            {
                return Result<Doctor>.Error("INVALID_CREDENTIALS", null!);
            }

            // Verify password
            if (!VerifyPassword(password, user.Password))
            {
                return Result<Doctor>.Error("INVALID_CREDENTIALS", null!);
            }

            // Check if phone is verified
            if (!user.IsPhoneVerified)
            {
                return Result<Doctor>.Error("PHONE_NOT_VERIFIED", null!);
            }

            // Check if account is active
            if (!user.IsActive)
            {
                return Result<Doctor>.Error("ACCOUNT_INACTIVE", null!);
            }

            // Get doctor with user data
            var doctor = await _context
                .Doctors.Include(d => d.User)
                .FirstOrDefaultAsync(d => d.UserId == user.Id);

            if (doctor == null)
            {
                return Result<Doctor>.Error("DOCTOR_NOT_FOUND", null!);
            }

            return Result<Doctor>.Ok("LOGIN_SUCCESS", doctor);
        }
        catch (Exception _)
        {
            return Result<Doctor>.Error("LOGIN_FAILED", null!);
        }
    }

    public async Task<Result<bool>> PhoneExistsAsync(string phoneNumber)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(phoneNumber))
            {
                return Result<bool>.Error("INVALID_PHONE_NUMBER", false);
            }

            var exists = await _context.Users.AnyAsync(u =>
                u.PhoneNumber == phoneNumber && !u.IsDeleted
            );

            return Result<bool>.Ok("PHONE_CHECK_SUCCESS", exists);
        }
        catch (Exception _)
        {
            return Result<bool>.Error("PHONE_CHECK_FAILED", false);
        }
    }

    public async Task<Result<bool>> UserNameExistsAsync(string userName)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(userName))
            {
                return Result<bool>.Error("INVALID_USERNAME", false);
            }

            var exists = await _context.Users.AnyAsync(u => u.UserName == userName && !u.IsDeleted);

            return Result<bool>.Ok("USERNAME_CHECK_SUCCESS", exists);
        }
        catch (Exception _)
        {
            return Result<bool>.Error("USERNAME_CHECK_FAILED", false);
        }
    }

    public async Task<Result<string>> ForgotPasswordAsync(string phoneNumber)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(phoneNumber))
            {
                return Result<string>.Error("INVALID_PHONE_NUMBER", string.Empty);
            }

            // Check if phone number exists and is verified
            var user = await _context.Users.FirstOrDefaultAsync(u =>
                u.PhoneNumber == phoneNumber && !u.IsDeleted
            );

            if (user == null)
            {
                return Result<string>.Error("PHONE_NUMBER_NOT_FOUND", string.Empty);
            }

            if (!user.IsPhoneVerified)
            {
                return Result<string>.Error("PHONE_NOT_VERIFIED", string.Empty);
            }

            if (!user.IsActive)
            {
                return Result<string>.Error("ACCOUNT_INACTIVE", string.Empty);
            }

            // Verify this user is actually a doctor
            var doctor = await _context.Doctors.FirstOrDefaultAsync(d => d.UserId == user.Id);
            if (doctor == null)
            {
                return Result<string>.Error("DOCTOR_NOT_FOUND", string.Empty);
            }

            // Generate and send OTP for password reset
            var otp = _otpService.GenerateOtp(phoneNumber);

            return Result<string>.Ok("FORGOT_PASSWORD_OTP_SENT", otp);
        }
        catch (Exception _)
        {
            return Result<string>.Error("FORGOT_PASSWORD_FAILED", string.Empty);
        }
    }

    public async Task<Result<bool>> ResetPasswordAsync(
        string phoneNumber,
        string otp,
        string newPassword
    )
    {
        try
        {
            if (
                string.IsNullOrWhiteSpace(phoneNumber)
                || string.IsNullOrWhiteSpace(otp)
                || string.IsNullOrWhiteSpace(newPassword)
            )
            {
                return Result<bool>.Error("INVALID_RESET_PASSWORD_DATA", false);
            }

            // Verify OTP
            if (!_otpService.VerifyOtp(phoneNumber, otp))
            {
                return Result<bool>.Error("INVALID_OTP", false);
            }

            // Find user by phone number
            var user = await _context.Users.FirstOrDefaultAsync(u =>
                u.PhoneNumber == phoneNumber && !u.IsDeleted
            );

            if (user == null)
            {
                return Result<bool>.Error("USER_NOT_FOUND", false);
            }

            // Check if phone is verified and account is active
            if (!user.IsPhoneVerified)
            {
                return Result<bool>.Error("PHONE_NOT_VERIFIED", false);
            }

            if (!user.IsActive)
            {
                return Result<bool>.Error("ACCOUNT_INACTIVE", false);
            }

            // Verify this user is actually a doctor
            var doctor = await _context.Doctors.FirstOrDefaultAsync(d => d.UserId == user.Id);
            if (doctor == null)
            {
                return Result<bool>.Error("DOCTOR_NOT_FOUND", false);
            }

            // Hash new password and update user
            var hashedPassword = HashPassword(newPassword);
            user.Password = hashedPassword;
            user.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            // Clear OTP after successful password reset
            _otpService.ClearOtp(phoneNumber);

            return Result<bool>.Ok("PASSWORD_RESET_SUCCESS", true);
        }
        catch (Exception _)
        {
            return Result<bool>.Error("PASSWORD_RESET_FAILED", false);
        }
    }

    public async Task<Result<Doctor>> GetDoctorByUserIdAsync(int userId)
    {
        try
        {
            if (userId <= 0)
            {
                return Result<Doctor>.Error("INVALID_USER_ID", null!);
            }

            // Get doctor with user data
            var doctor = await _context
                .Doctors.Include(d => d.User)
                .FirstOrDefaultAsync(d => d.UserId == userId && !d.User.IsDeleted);

            if (doctor == null)
            {
                return Result<Doctor>.Error("DOCTOR_NOT_FOUND", null!);
            }

            // Check if account is active
            if (!doctor.User.IsActive)
            {
                return Result<Doctor>.Error("ACCOUNT_INACTIVE", null!);
            }

            return Result<Doctor>.Ok("DOCTOR_PROFILE_SUCCESS", doctor);
        }
        catch (Exception _)
        {
            return Result<Doctor>.Error("GET_DOCTOR_PROFILE_FAILED", null!);
        }
    }

    private static string HashPassword(string password)
    {
        // In production, use a proper password hashing library like BCrypt, Argon2, etc.
        // This is a simple implementation for demonstration purposes
        return Convert.ToBase64String(System.Text.Encoding.UTF8.GetBytes(password));
    }

    private static bool VerifyPassword(string password, string hashedPassword)
    {
        // In production, use the corresponding verification method for your hashing algorithm
        var inputHash = Convert.ToBase64String(System.Text.Encoding.UTF8.GetBytes(password));
        return inputHash == hashedPassword;
    }
}
