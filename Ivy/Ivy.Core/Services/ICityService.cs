using Ivy.Core.Entities;
using IvyBackend;

namespace Ivy.Core.Services;

public interface ICityService
{
    Task<Result<PaginatedResult<City>>> GetAllAsync(
        int page = 1,
        int pageSize = 10,
        string? nameAr = null,
        string? nameEn = null,
        int? governorateId = null,
        bool? isActive = null
    );

    Task<Result<City>> GetByIdAsync(int id);

    Task<Result<City>> CreateAsync(City city);

    Task<Result<City>> UpdateAsync(int id, City city);

    Task<Result> DeleteAsync(int id);

    Task<Result<IEnumerable<City>>> GetByGovernorateIdAsync(int governorateId);

    Task<Result<bool>> ExistsAsync(int id);

    Task<Result<bool>> ExistsAsync(
        string nameAr,
        string nameEn,
        int governorateId,
        int? excludeId = null
    );
}