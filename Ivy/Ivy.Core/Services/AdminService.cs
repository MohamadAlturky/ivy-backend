using IvyBackend;
using Ivy.Core.DataContext;
using Ivy.Core.Entities;
using Microsoft.EntityFrameworkCore;

namespace Ivy.Core.Services;

public class AdminService : IAdminService
{
    private readonly IvyContext _context;

    public AdminService(IvyContext context)
    {
        _context = context;
    }

    public async Task<Result<Admin>> LoginAsync(string email, string password)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(password))
            {
                return Result<Admin>.Error("INVALID_LOGIN_DATA", null!);
            }

            // Find admin by email
            var admin = await _context.Admins.FirstOrDefaultAsync(a =>
                a.Email == email && !a.IsDeleted
            );

            if (admin == null)
            {
                return Result<Admin>.Error("INVALID_CREDENTIALS", null!);
            }

            // Verify password
            if (!VerifyPassword(password, admin.Password))
            {
                return Result<Admin>.Error("INVALID_CREDENTIALS", null!);
            }

            // Check if account is active
            if (!admin.IsActive)
            {
                return Result<Admin>.Error("ACCOUNT_INACTIVE", null!);
            }

            return Result<Admin>.Ok("ADMIN_LOGIN_SUCCESS", admin);
        }
        catch (Exception ex)
        {
            return Result<Admin>.Error("ADMIN_LOGIN_FAILED", null!);
        }
    }

    public async Task<Result<Admin>> GetProfileAsync(int adminId)
    {
        try
        {
            if (adminId <= 0)
            {
                return Result<Admin>.Error("INVALID_ADMIN_ID", null!);
            }

            var admin = await _context.Admins.FirstOrDefaultAsync(a =>
                a.Id == adminId && !a.IsDeleted
            );

            if (admin == null)
            {
                return Result<Admin>.Error("ADMIN_NOT_FOUND", null!);
            }

            // Check if account is active
            if (!admin.IsActive)
            {
                return Result<Admin>.Error("ACCOUNT_INACTIVE", null!);
            }

            return Result<Admin>.Ok("ADMIN_PROFILE_RETRIEVED_SUCCESS", admin);
        }
        catch (Exception ex)
        {
            return Result<Admin>.Error("ADMIN_PROFILE_RETRIEVAL_FAILED", null!);
        }
    }

    public async Task<Result<Admin>> UpdateProfileAsync(int adminId, string email, string? currentPassword = null, string? newPassword = null)
    {
        try
        {
            if (adminId <= 0 || string.IsNullOrWhiteSpace(email))
            {
                return Result<Admin>.Error("INVALID_UPDATE_DATA", null!);
            }

            var admin = await _context.Admins.FirstOrDefaultAsync(a =>
                a.Id == adminId && !a.IsDeleted
            );

            if (admin == null)
            {
                return Result<Admin>.Error("ADMIN_NOT_FOUND", null!);
            }

            // Check if email is already taken by another admin
            var emailExists = await _context.Admins.AnyAsync(a =>
                a.Id != adminId && a.Email == email && !a.IsDeleted
            );

            if (emailExists)
            {
                return Result<Admin>.Error("ADMIN_EMAIL_ALREADY_EXISTS", null!);
            }

            // If password change is requested, verify current password
            if (!string.IsNullOrWhiteSpace(newPassword))
            {
                if (string.IsNullOrWhiteSpace(currentPassword))
                {
                    return Result<Admin>.Error("CURRENT_PASSWORD_REQUIRED", null!);
                }

                if (!VerifyPassword(currentPassword, admin.Password))
                {
                    return Result<Admin>.Error("INVALID_CURRENT_PASSWORD", null!);
                }

                admin.Password = HashPassword(newPassword);
            }

            // Update email
            admin.Email = email;
            admin.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            return Result<Admin>.Ok("ADMIN_PROFILE_UPDATED_SUCCESS", admin);
        }
        catch (Exception ex)
        {
            return Result<Admin>.Error("ADMIN_PROFILE_UPDATE_FAILED", null!);
        }
    }

    public async Task<Result<bool>> EmailExistsAsync(string email)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(email))
            {
                return Result<bool>.Error("INVALID_EMAIL", false);
            }

            var exists = await _context.Admins.AnyAsync(a =>
                a.Email == email && !a.IsDeleted
            );

            return Result<bool>.Ok("ADMIN_EMAIL_CHECK_SUCCESS", exists);
        }
        catch (Exception ex)
        {
            return Result<bool>.Error("ADMIN_EMAIL_CHECK_FAILED", false);
        }
    }

    public async Task<Result<bool>> ChangePasswordAsync(int adminId, string currentPassword, string newPassword)
    {
        try
        {
            if (adminId <= 0 || string.IsNullOrWhiteSpace(currentPassword) || string.IsNullOrWhiteSpace(newPassword))
            {
                return Result<bool>.Error("INVALID_PASSWORD_CHANGE_DATA", false);
            }

            var admin = await _context.Admins.FirstOrDefaultAsync(a =>
                a.Id == adminId && !a.IsDeleted
            );

            if (admin == null)
            {
                return Result<bool>.Error("ADMIN_NOT_FOUND", false);
            }

            // Verify current password
            if (!VerifyPassword(currentPassword, admin.Password))
            {
                return Result<bool>.Error("INVALID_CURRENT_PASSWORD", false);
            }

            // Update password
            admin.Password = HashPassword(newPassword);
            admin.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            return Result<bool>.Ok("ADMIN_PASSWORD_CHANGED_SUCCESS", true);
        }
        catch (Exception ex)
        {
            return Result<bool>.Error("ADMIN_PASSWORD_CHANGE_FAILED", false);
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
