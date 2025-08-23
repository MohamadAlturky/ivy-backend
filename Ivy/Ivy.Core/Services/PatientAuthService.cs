using Ivy.Core.DataContext;
using Ivy.Core.Entities;
using IvyBackend;
using Microsoft.EntityFrameworkCore;

namespace Ivy.Core.Services;

public class PatientAuthService : IPatientAuthService
{
    private readonly IvyContext _context;
    private readonly IOtpService _otpService;

    public PatientAuthService(IvyContext context, IOtpService otpService)
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
        DateTime dateOfBirth)
    {
        try
        {
            // Validate required fields
            if (string.IsNullOrWhiteSpace(firstName) || 
                string.IsNullOrWhiteSpace(lastName) || 
                string.IsNullOrWhiteSpace(userName) ||
                string.IsNullOrWhiteSpace(password) ||
                string.IsNullOrWhiteSpace(phoneNumber))
            {
                return Result<string>.Error("INVALID_REGISTRATION_DATA", string.Empty);
            }

            // Check if phone number already exists
            var phoneExists = await _context.Users
                .AnyAsync(u => u.PhoneNumber == phoneNumber && !u.IsDeleted);
            
            if (phoneExists)
            {
                return Result<string>.Error("PHONE_NUMBER_ALREADY_EXISTS", string.Empty);
            }

            // Check if username already exists
            var userNameExists = await _context.Users
                .AnyAsync(u => u.UserName == userName && !u.IsDeleted);
            
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
                IsActive = false // Will be activated after OTP verification
            };

            _context.Users.Add(user);
            await _context.SaveChangesAsync();

            // Create patient entity
            var patient = new Patient
            {
                UserId = user.Id,
                User = user
            };

            _context.Patients.Add(patient);
            await _context.SaveChangesAsync();

            // Generate and send OTP
            var otp = _otpService.GenerateOtp(phoneNumber);

            return Result<string>.Ok("PATIENT_REGISTERED_SUCCESS", otp);
        }
        catch (Exception ex)
        {
            return Result<string>.Error("PATIENT_REGISTRATION_FAILED", string.Empty);
        }
    }

    public async Task<Result<Patient>> VerifyOtpAsync(string phoneNumber, string otp)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(phoneNumber) || string.IsNullOrWhiteSpace(otp))
            {
                return Result<Patient>.Error("INVALID_OTP_DATA", null!);
            }

            // Verify OTP
            if (!_otpService.VerifyOtp(phoneNumber, otp))
            {
                return Result<Patient>.Error("INVALID_OTP", null!);
            }

            // Find user by phone number
            var user = await _context.Users
                .FirstOrDefaultAsync(u => u.PhoneNumber == phoneNumber && !u.IsDeleted);

            if (user == null)
            {
                return Result<Patient>.Error("USER_NOT_FOUND", null!);
            }

            // Update user verification status
            user.IsPhoneVerified = true;
            user.IsActive = true;
            user.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            // Clear OTP
            _otpService.ClearOtp(phoneNumber);

            // Get patient with user data
            var patient = await _context.Patients
                .Include(p => p.User)
                .FirstOrDefaultAsync(p => p.UserId == user.Id);

            if (patient == null)
            {
                return Result<Patient>.Error("PATIENT_NOT_FOUND", null!);
            }

            return Result<Patient>.Ok("OTP_VERIFIED_SUCCESS", patient);
        }
        catch (Exception ex)
        {
            return Result<Patient>.Error("OTP_VERIFICATION_FAILED", null!);
        }
    }

    public async Task<Result<Patient>> LoginAsync(string phoneNumber, string password)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(phoneNumber) || string.IsNullOrWhiteSpace(password))
            {
                return Result<Patient>.Error("INVALID_LOGIN_DATA", null!);
            }

            // Find user by phone number
            var user = await _context.Users
                .FirstOrDefaultAsync(u => u.PhoneNumber == phoneNumber && !u.IsDeleted);

            if (user == null)
            {
                return Result<Patient>.Error("INVALID_CREDENTIALS", null!);
            }

            // Verify password
            if (!VerifyPassword(password, user.Password))
            {
                return Result<Patient>.Error("INVALID_CREDENTIALS", null!);
            }

            // Check if phone is verified
            if (!user.IsPhoneVerified)
            {
                return Result<Patient>.Error("PHONE_NOT_VERIFIED", null!);
            }

            // Check if account is active
            if (!user.IsActive)
            {
                return Result<Patient>.Error("ACCOUNT_INACTIVE", null!);
            }

            // Get patient with user data
            var patient = await _context.Patients
                .Include(p => p.User)
                .FirstOrDefaultAsync(p => p.UserId == user.Id);

            if (patient == null)
            {
                return Result<Patient>.Error("PATIENT_NOT_FOUND", null!);
            }

            return Result<Patient>.Ok("LOGIN_SUCCESS", patient);
        }
        catch (Exception ex)
        {
            return Result<Patient>.Error("LOGIN_FAILED", null!);
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

            var exists = await _context.Users
                .AnyAsync(u => u.PhoneNumber == phoneNumber && !u.IsDeleted);

            return Result<bool>.Ok("PHONE_CHECK_SUCCESS", exists);
        }
        catch (Exception ex)
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

            var exists = await _context.Users
                .AnyAsync(u => u.UserName == userName && !u.IsDeleted);

            return Result<bool>.Ok("USERNAME_CHECK_SUCCESS", exists);
        }
        catch (Exception ex)
        {
            return Result<bool>.Error("USERNAME_CHECK_FAILED", false);
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
