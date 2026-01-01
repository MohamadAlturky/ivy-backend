using Ivy.Core.Entities;
using IvyBackend;

namespace Ivy.Core.Services;

public interface IGovernorateService
{
    Task<Result<PaginatedResult<Governorate>>> GetAllAsync(
        int page = 1,
        int pageSize = 10,
        string? nameAr = null,
        string? nameEn = null,
        bool? isActive = null,
        bool includeCities = false
    );

    Task<Result<Governorate>> GetByIdAsync(int id, bool includeCities = false);

    Task<Result<Governorate>> CreateAsync(Governorate governorate);

    Task<Result<Governorate>> UpdateAsync(int id, Governorate governorate);

    Task<Result> DeleteAsync(int id);

    Task<Result<bool>> ExistsAsync(int id);

    Task<Result<bool>> ExistsAsync(string nameAr, string nameEn, int? excludeId = null);

    Task<Result<int>> GetCitiesCountAsync(int governorateId);
}