using Ivy.Core.Entities;
using IvyBackend;

namespace Ivy.Core.Services;

public interface IClinicService
{
    Task<Result<PaginatedResult<Clinic>>> GetAllAsync(
        int page = 1,
        int pageSize = 10,
        string? nameAr = null,
        string? nameEn = null,
        string? descriptionAr = null,
        string? descriptionEn = null,
        int? locationId = null,
        bool? isActive = null
    );

    Task<Result<Clinic>> GetByIdAsync(int id);

    Task<Result<Clinic>> CreateAsync(Clinic clinic);

    Task<Result<Clinic>> UpdateAsync(int id, Clinic clinic);

    Task<Result> DeleteAsync(int id);

    Task<Result<IEnumerable<Clinic>>> GetByLocationIdAsync(int locationId);

    Task<Result<bool>> ExistsAsync(int id);

    Task<Result<bool>> ExistsAsync(
        string nameAr,
        string nameEn,
        int locationId,
        int? excludeId = null
    );

    Task<Result<bool>> EmailExistsAsync(string email, int? excludeId = null);

    Task<Result> DeactivateAsync(int id);

    Task<Result> ActivateAsync(int id);

    Task<Result<DoctorClinic>> AddDoctorToClinicAsync(int clinicId, int doctorId);

    Task<Result> RemoveDoctorFromClinicAsync(int clinicId, int doctorId);

    Task<Result<IEnumerable<DoctorClinic>>> GetClinicDoctorsAsync(int clinicId);
}