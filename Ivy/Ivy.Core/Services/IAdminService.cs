using Ivy.Core.Entities;
using IvyBackend;

namespace Ivy.Core.Services;

public interface IAdminService
{
    /// <summary>
    /// Authenticates admin with email and password
    /// </summary>
    /// <param name="email">Admin email</param>
    /// <param name="password">Admin password</param>
    /// <returns>Result with login status and admin data</returns>
    Task<Result<Admin>> LoginAsync(string email, string password);

    /// <summary>
    /// Gets admin profile by admin ID
    /// </summary>
    /// <param name="adminId">Admin ID</param>
    /// <returns>Result with admin profile data</returns>
    Task<Result<Admin>> GetProfileAsync(int adminId);

    /// <summary>
    /// Updates admin profile information
    /// </summary>
    /// <param name="adminId">Admin ID</param>
    /// <param name="email">New email</param>
    /// <param name="currentPassword">Current password for verification</param>
    /// <param name="newPassword">New password (optional)</param>
    /// <returns>Result with updated admin data</returns>
    Task<Result<Admin>> UpdateProfileAsync(int adminId, string email, string? currentPassword = null,
        string? newPassword = null);

    /// <summary>
    /// Checks if an admin exists by email
    /// </summary>
    /// <param name="email">Email to check</param>
    /// <returns>True if admin exists</returns>
    Task<Result<bool>> EmailExistsAsync(string email);

    /// <summary>
    /// Changes admin password
    /// </summary>
    /// <param name="adminId">Admin ID</param>
    /// <param name="currentPassword">Current password</param>
    /// <param name="newPassword">New password</param>
    /// <returns>Result indicating password change success</returns>
    Task<Result<bool>> ChangePasswordAsync(int adminId, string currentPassword, string newPassword);
}