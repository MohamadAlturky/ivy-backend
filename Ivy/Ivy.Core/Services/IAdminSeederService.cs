using IvyBackend;

namespace Ivy.Core.Services;

public interface IAdminSeederService
{
    /// <summary>
    /// Seeds default admin if no admin exists in the database
    /// </summary>
    /// <returns>Result indicating seeding operation success</returns>
    Task<Result<bool>> SeedDefaultAdminAsync();
}