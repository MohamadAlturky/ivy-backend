using Ivy.Core.Entities;
using IvyBackend;

namespace Ivy.Core.Services;

public interface IMedicalSpecialityService
{
    Task<Result<PaginatedResult<MedicalSpeciality>>> GetAllAsync(
        int page = 1,
        int pageSize = 10,
        string? nameAr = null,
        string? nameEn = null,
        string? searchTerm = null,
        bool? isActive = null
    );

    Task<Result<MedicalSpeciality>> GetByIdAsync(int id);

    Task<Result<MedicalSpeciality>> CreateAsync(MedicalSpeciality medicalSpeciality);

    Task<Result<MedicalSpeciality>> UpdateAsync(int id, MedicalSpeciality medicalSpeciality);

    Task<Result> DeleteAsync(int id);

    Task<Result<bool>> ExistsAsync(int id);

    Task<Result<bool>> ExistsAsync(string nameAr, string nameEn, int? excludeId = null);
}
