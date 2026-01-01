using Ivy.Core.DataContext;
using Ivy.Core.Entities;
using IvyBackend;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace Ivy.Core.Services;

public class AdminSeederService : IAdminSeederService
{
    private readonly IvyContext _context;
    private readonly IConfiguration _configuration;
    private readonly ILogger<AdminSeederService> _logger;

    public AdminSeederService(
        IvyContext context,
        IConfiguration configuration,
        ILogger<AdminSeederService> logger)
    {
        _context = context;
        _configuration = configuration;
        _logger = logger;
    }

    public async Task<Result<bool>> SeedDefaultAdminAsync()
    {
        try
        {
            // Check if any admin already exists
            var adminExists = await _context.Admins.AnyAsync(a => !a.IsDeleted);

            if (adminExists)
            {
                _logger.LogInformation("Admin already exists in the database. Skipping seeding.");
                return Result<bool>.Ok("ADMIN_ALREADY_EXISTS", false);
            }

            // Get default admin credentials from configuration
            var defaultEmail = _configuration["DefaultAdmin:Email"] ?? "admin@ivy.com";
            var defaultPassword = _configuration["DefaultAdmin:Password"] ?? "Admin123!";

            // Create default admin
            var admin = new Admin
            {
                Email = defaultEmail,
                Password = HashPassword(defaultPassword),
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow,
                IsActive = true,
                IsDeleted = false
            };

            _context.Admins.Add(admin);
            await _context.SaveChangesAsync();

            _logger.LogInformation(
                "Default admin created successfully with email: {Email}",
                defaultEmail);

            return Result<bool>.Ok("DEFAULT_ADMIN_SEEDED_SUCCESS", true);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to seed default admin");
            return Result<bool>.Error("ADMIN_SEEDING_FAILED", false);
        }
    }

    private static string HashPassword(string password)
    {
        // Using the same simple hashing method as in AdminService
        // In production, use a proper password hashing library like BCrypt, Argon2, etc.
        return Convert.ToBase64String(System.Text.Encoding.UTF8.GetBytes(password));
    }
}