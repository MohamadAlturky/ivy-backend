using Ivy.Contracts.Models;
using Ivy.Core.DataContext;
using Ivy.Core.Entities;
using IvyBackend; // Assuming this namespace holds Result/PaginatedResult
using Microsoft.EntityFrameworkCore;

namespace Ivy.Contracts.Services;

public interface IMedicalSpecialityService
{
    Task<Result<PaginatedResult<MedicalSpecialityDto>>> GetAllAsync(
        int page = 1,
        int pageSize = 10,
        string? name = null,
        bool? isActive = null
    );
    Task<Result<PaginatedResult<MedicalSpecialityLocalizedDto>>> GetAllLocalizedAsync(
        string language = "en",
        int page = 1,
        int pageSize = 10,
        string? name = null,
        bool? isActive = null
    );
    Task<Result<MedicalSpecialityDto>> CreateAsync(CreateMedicalSpecialityDto dto);
    Task<Result<MedicalSpecialityDto>> UpdateAsync(int id, UpdateMedicalSpecialityDto dto);
    Task<Result> DeleteAsync(int id);
    Task<Result> ToggleStatusAsync(int id);
}

public class MedicalSpecialityService : IMedicalSpecialityService
{
    private readonly IvyContext _context;

    public MedicalSpecialityService(IvyContext context)
    {
        _context = context;
    }

    public async Task<Result<PaginatedResult<MedicalSpecialityDto>>> GetAllAsync(
        int page = 1,
        int pageSize = 10,
        string? name = null,
        bool? isActive = null
    )
    {
        var query = _context.Set<MedicalSpeciality>().AsQueryable();

        // Filtering
        if (isActive.HasValue)
        {
            // Assuming IsActive is inherited from BaseEntity or exists on the entity
            query = query.Where(x => x.IsActive == isActive.Value);
        }

        if (!string.IsNullOrWhiteSpace(name))
        {
            query = query.Where(x => x.NameAr.Contains(name) || x.NameEn.Contains(name));
        }

        // Pagination
        var totalCount = await query.CountAsync();

        var items = await query
            .OrderBy(x => x.Id)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(x => new MedicalSpecialityDto
            {
                Id = x.Id,
                NameAr = x.NameAr,
                NameEn = x.NameEn,
                DescriptionAr = x.DescriptionAr,
                DescriptionEn = x.DescriptionEn,
                IsActive = x.IsActive,
            })
            .ToListAsync();

        var result = PaginatedResult<MedicalSpecialityDto>.Create(items, totalCount, page, pageSize);

        return Result<PaginatedResult<MedicalSpecialityDto>>.Ok(
            MedicalSpecialityServiceMessageCodes.SUCCESS,
            result
        );
    }

    public async Task<Result<MedicalSpecialityDto>> CreateAsync(CreateMedicalSpecialityDto dto)
    {
        // Validation
        if (string.IsNullOrWhiteSpace(dto.NameAr) || string.IsNullOrWhiteSpace(dto.NameEn))
        {
            return Result<MedicalSpecialityDto>.Error(MedicalSpecialityServiceMessageCodes.INVALID_DATA, null!);
        }

        // Check for duplicate names
        bool exists = await _context
            .Set<MedicalSpeciality>()
            .AnyAsync(x => x.NameAr == dto.NameAr || x.NameEn == dto.NameEn);

        if (exists)
        {
            return Result<MedicalSpecialityDto>.Error(
                MedicalSpecialityServiceMessageCodes.DUPLICATE_NAME,
                null!
            );
        }

        var entity = new MedicalSpeciality
        {
            NameAr = dto.NameAr,
            NameEn = dto.NameEn,
            DescriptionAr = dto.DescriptionAr,
            DescriptionEn = dto.DescriptionEn,
            IsActive = dto.IsActive,
        };

        await _context.Set<MedicalSpeciality>().AddAsync(entity);
        await _context.SaveChangesAsync();

        // Map back to DTO
        var resultDto = new MedicalSpecialityDto
        {
            Id = entity.Id,
            NameAr = entity.NameAr,
            NameEn = entity.NameEn,
            DescriptionAr = entity.DescriptionAr,
            DescriptionEn = entity.DescriptionEn,
            IsActive = entity.IsActive,
        };

        return Result<MedicalSpecialityDto>.Ok(MedicalSpecialityServiceMessageCodes.CREATED, resultDto);
    }

    public async Task<Result<MedicalSpecialityDto>> UpdateAsync(int id, UpdateMedicalSpecialityDto dto)
    {
        if (string.IsNullOrWhiteSpace(dto.NameAr) || string.IsNullOrWhiteSpace(dto.NameEn))
        {
            return Result<MedicalSpecialityDto>.Error(MedicalSpecialityServiceMessageCodes.INVALID_DATA, null!);
        }

        var entity = await _context.Set<MedicalSpeciality>().FindAsync(id);

        if (entity == null)
        {
            return Result<MedicalSpecialityDto>.Error(MedicalSpecialityServiceMessageCodes.NOT_FOUND, null!);
        }

        // Update fields
        entity.NameAr = dto.NameAr;
        entity.NameEn = dto.NameEn;
        entity.DescriptionAr = dto.DescriptionAr;
        entity.DescriptionEn = dto.DescriptionEn;
        entity.IsActive = dto.IsActive;

        _context.Set<MedicalSpeciality>().Update(entity);
        await _context.SaveChangesAsync();

        var resultDto = new MedicalSpecialityDto
        {
            Id = entity.Id,
            NameAr = entity.NameAr,
            NameEn = entity.NameEn,
            DescriptionAr = entity.DescriptionAr,
            DescriptionEn = entity.DescriptionEn,
            IsActive = entity.IsActive,
        };

        return Result<MedicalSpecialityDto>.Ok(MedicalSpecialityServiceMessageCodes.UPDATED, resultDto);
    }

    public async Task<Result> DeleteAsync(int id)
    {
        var entity = await _context.Set<MedicalSpeciality>().FindAsync(id);

        if (entity == null)
        {
            return Result.Error(MedicalSpecialityServiceMessageCodes.NOT_FOUND);
        }

        // Hard delete
        _context.Set<MedicalSpeciality>().Remove(entity);
        await _context.SaveChangesAsync();

        return Result.Ok(MedicalSpecialityServiceMessageCodes.DELETED);
    }

    public async Task<Result<PaginatedResult<MedicalSpecialityLocalizedDto>>> GetAllLocalizedAsync(
        string language = "en",
        int page = 1,
        int pageSize = 10,
        string? name = null,
        bool? isActive = null
    )
    {
        var query = _context.Set<MedicalSpeciality>().AsQueryable();

        // Filtering
        if (isActive.HasValue)
        {
            query = query.Where(x => x.IsActive == isActive.Value);
        }

        if (!string.IsNullOrWhiteSpace(name))
        {
            query = query.Where(x => x.NameAr.Contains(name) || x.NameEn.Contains(name));
        }

        // Pagination
        var totalCount = await query.CountAsync();

        var items = await query
            .OrderBy(x => x.Id)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(x => new MedicalSpecialityLocalizedDto
            {
                Id = x.Id,
                Name = language == "ar" ? x.NameAr : x.NameEn,
                Description = language == "ar" ? x.DescriptionAr : x.DescriptionEn,
                IsActive = x.IsActive,
            })
            .ToListAsync();

        var result = PaginatedResult<MedicalSpecialityLocalizedDto>.Create(
            items,
            totalCount,
            page,
            pageSize
        );

        return Result<PaginatedResult<MedicalSpecialityLocalizedDto>>.Ok(
            MedicalSpecialityServiceMessageCodes.SUCCESS,
            result
        );
    }

    public async Task<Result> ToggleStatusAsync(int id)
    {
        var entity = await _context.Set<MedicalSpeciality>().FindAsync(id);

        if (entity == null)
        {
            return Result.Error(MedicalSpecialityServiceMessageCodes.NOT_FOUND);
        }

        entity.IsActive = !entity.IsActive;
        await _context.SaveChangesAsync();

        return Result.Ok(MedicalSpecialityServiceMessageCodes.STATUS_UPDATED);
    }
}

public static class MedicalSpecialityServiceMessageCodes
{
    public const string INVALID_DATA = "MEDICAL_SPECIALITY_INVALID_DATA";
    public const string DUPLICATE_NAME = "MEDICAL_SPECIALITY_DUPLICATE_NAME";
    public const string NOT_FOUND = "MEDICAL_SPECIALITY_NOT_FOUND";
    public const string CREATED = "MEDICAL_SPECIALITY_CREATED";
    public const string UPDATED = "MEDICAL_SPECIALITY_UPDATED";
    public const string DELETED = "MEDICAL_SPECIALITY_DELETED";
    public const string SUCCESS = "MEDICAL_SPECIALITY_SUCCESS";
    public const string STATUS_UPDATED = "MEDICAL_SPECIALITY_STATUS_UPDATED";
}