using IvyBackend;
using Ivy.Core.DataContext;
using Ivy.Core.Entities;
using Microsoft.EntityFrameworkCore;

namespace Ivy.Core.Services;

public class MedicalSpecialityService : IMedicalSpecialityService
{
    private readonly IvyContext _context;

    public MedicalSpecialityService(IvyContext context)
    {
        _context = context;
    }

    public async Task<Result<PaginatedResult<MedicalSpeciality>>> GetAllAsync(
        int page = 1,
        int pageSize = 10,
        string? nameAr = null,
        string? nameEn = null,
        string? searchTerm = null,
        bool? isActive = null)
    {
        try
        {
            // Validate pagination parameters
            if (page < 1)
                page = 1;
            if (pageSize < 1)
                pageSize = 10;
            if (pageSize > 100)
                pageSize = 100; // Limit maximum page size

            var query = _context.MedicalSpecialities.AsQueryable();

            // Apply filters
            if (!string.IsNullOrWhiteSpace(nameAr))
            {
                query = query.Where(ms => ms.NameAr.Contains(nameAr));
            }

            if (!string.IsNullOrWhiteSpace(nameEn))
            {
                query = query.Where(ms => ms.NameEn.Contains(nameEn));
            }

            // Apply general search term (searches in both name and description fields)
            if (!string.IsNullOrWhiteSpace(searchTerm))
            {
                query = query.Where(ms => ms.NameAr.Contains(searchTerm) || 
                                         ms.NameEn.Contains(searchTerm) ||
                                         ms.DescriptionAr.Contains(searchTerm) ||
                                         ms.DescriptionEn.Contains(searchTerm));
            }

            if (isActive.HasValue)
            {
                query = query.Where(ms => ms.IsActive == isActive.Value);
            }

            // Get total count before pagination
            var totalCount = await query.CountAsync();

            // Apply pagination and ordering
            var medicalSpecialities = await query
                .OrderBy(ms => ms.NameEn)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            var paginatedResult = PaginatedResult<MedicalSpeciality>.Create(medicalSpecialities, totalCount, page, pageSize);
            return Result<PaginatedResult<MedicalSpeciality>>.Ok("MEDICAL_SPECIALITIES_RETRIEVED_SUCCESS", paginatedResult);
        }
        catch (Exception ex)
        {
            var emptyResult = new PaginatedResult<MedicalSpeciality>();
            return Result<PaginatedResult<MedicalSpeciality>>.Error("MEDICAL_SPECIALITIES_RETRIEVAL_FAILED", emptyResult);
        }
    }

    public async Task<Result<MedicalSpeciality>> GetByIdAsync(int id)
    {
        try
        {
            var medicalSpeciality = await _context.MedicalSpecialities.FirstOrDefaultAsync(ms => ms.Id == id);

            if (medicalSpeciality == null)
            {
                return Result<MedicalSpeciality>.Error("MEDICAL_SPECIALITY_NOT_FOUND", null!);
            }

            return Result<MedicalSpeciality>.Ok("MEDICAL_SPECIALITY_RETRIEVED_SUCCESS", medicalSpeciality);
        }
        catch (Exception ex)
        {
            return Result<MedicalSpeciality>.Error("MEDICAL_SPECIALITY_RETRIEVAL_FAILED", null!);
        }
    }

    public async Task<Result<MedicalSpeciality>> CreateAsync(MedicalSpeciality medicalSpeciality)
    {
        try
        {
            // Check for duplicate name
            var duplicateExists = await _context.MedicalSpecialities
                .AnyAsync(ms => ms.NameAr == medicalSpeciality.NameAr || ms.NameEn == medicalSpeciality.NameEn);

            if (duplicateExists)
            {
                return Result<MedicalSpeciality>.Error("MEDICAL_SPECIALITY_NAME_ALREADY_EXISTS", null!);
            }

            medicalSpeciality.CreatedAt = DateTime.UtcNow;
            medicalSpeciality.UpdatedAt = DateTime.UtcNow;

            _context.MedicalSpecialities.Add(medicalSpeciality);
            await _context.SaveChangesAsync();

            return Result<MedicalSpeciality>.Ok("MEDICAL_SPECIALITY_CREATED_SUCCESS", medicalSpeciality);
        }
        catch (Exception ex)
        {
            return Result<MedicalSpeciality>.Error("MEDICAL_SPECIALITY_CREATION_FAILED", null!);
        }
    }

    public async Task<Result<MedicalSpeciality>> UpdateAsync(int id, MedicalSpeciality medicalSpeciality)
    {
        try
        {
            var existingMedicalSpeciality = await _context.MedicalSpecialities.FirstOrDefaultAsync(ms => ms.Id == id);
            if (existingMedicalSpeciality == null)
            {
                return Result<MedicalSpeciality>.Error("MEDICAL_SPECIALITY_NOT_FOUND", null!);
            }

            // Check for duplicate name (excluding current medical speciality)
            var duplicateExists = await _context.MedicalSpecialities
                .AnyAsync(ms => ms.Id != id && 
                          (ms.NameAr == medicalSpeciality.NameAr || ms.NameEn == medicalSpeciality.NameEn));

            if (duplicateExists)
            {
                return Result<MedicalSpeciality>.Error("MEDICAL_SPECIALITY_NAME_ALREADY_EXISTS", null!);
            }

            existingMedicalSpeciality.NameAr = medicalSpeciality.NameAr;
            existingMedicalSpeciality.NameEn = medicalSpeciality.NameEn;
            existingMedicalSpeciality.DescriptionAr = medicalSpeciality.DescriptionAr;
            existingMedicalSpeciality.DescriptionEn = medicalSpeciality.DescriptionEn;
            existingMedicalSpeciality.IsActive = medicalSpeciality.IsActive;
            existingMedicalSpeciality.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            return Result<MedicalSpeciality>.Ok("MEDICAL_SPECIALITY_UPDATED_SUCCESS", existingMedicalSpeciality);
        }
        catch (Exception ex)
        {
            return Result<MedicalSpeciality>.Error("MEDICAL_SPECIALITY_UPDATE_FAILED", null!);
        }
    }

    public async Task<Result> DeleteAsync(int id)
    {
        try
        {
            var medicalSpeciality = await _context.MedicalSpecialities.FirstOrDefaultAsync(ms => ms.Id == id);

            if (medicalSpeciality == null)
            {
                return Result.Error("MEDICAL_SPECIALITY_NOT_FOUND");
            }

            // Soft delete
            medicalSpeciality.IsDeleted = true;
            medicalSpeciality.DeletedAt = DateTime.UtcNow;
            medicalSpeciality.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            return Result.Ok("MEDICAL_SPECIALITY_DELETED_SUCCESS");
        }
        catch (Exception ex)
        {
            return Result.Error("MEDICAL_SPECIALITY_DELETION_FAILED");
        }
    }

    public async Task<Result<bool>> ExistsAsync(int id)
    {
        try
        {
            var exists = await _context.MedicalSpecialities.AnyAsync(ms => ms.Id == id);
            return Result<bool>.Ok("MEDICAL_SPECIALITY_EXISTS_CHECK_SUCCESS", exists);
        }
        catch (Exception ex)
        {
            return Result<bool>.Error("MEDICAL_SPECIALITY_EXISTS_CHECK_FAILED", false);
        }
    }

    public async Task<Result<bool>> ExistsAsync(string nameAr, string nameEn, int? excludeId = null)
    {
        try
        {
            var query = _context.MedicalSpecialities.AsQueryable();

            if (excludeId.HasValue)
            {
                query = query.Where(ms => ms.Id != excludeId.Value);
            }

            var exists = await query.AnyAsync(ms => ms.NameAr == nameAr || ms.NameEn == nameEn);

            return Result<bool>.Ok("MEDICAL_SPECIALITY_EXISTS_CHECK_SUCCESS", exists);
        }
        catch (Exception ex)
        {
            return Result<bool>.Error("MEDICAL_SPECIALITY_EXISTS_CHECK_FAILED", false);
        }
    }
}
