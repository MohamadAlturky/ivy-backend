using IvyBackend;
using Ivy.Core.DataContext;
using Ivy.Core.Entities;
using Microsoft.EntityFrameworkCore;

namespace Ivy.Core.Services;

public class GovernorateService : IGovernorateService
{
    private readonly IvyContext _context;

    public GovernorateService(IvyContext context)
    {
        _context = context;
    }

    public async Task<Result<PaginatedResult<Governorate>>> GetAllAsync(
        int page = 1,
        int pageSize = 10,
        string? nameAr = null,
        string? nameEn = null,
        bool? isActive = null,
        bool includeCities = false)
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

            var query = _context.Governorates.AsQueryable();

            if (includeCities)
            {
                query = query.Include(g => g.Cities);
            }

            // Apply filters
            if (!string.IsNullOrWhiteSpace(nameAr))
            {
                query = query.Where(g => g.NameAr.Contains(nameAr));
            }

            if (!string.IsNullOrWhiteSpace(nameEn))
            {
                query = query.Where(g => g.NameEn.Contains(nameEn));
            }

            if (isActive.HasValue)
            {
                query = query.Where(g => g.IsActive == isActive.Value);
            }

            // Get total count before pagination
            var totalCount = await query.CountAsync();

            // Apply pagination and ordering
            var governorates = await query
                .OrderBy(g => g.NameEn)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            var paginatedResult = PaginatedResult<Governorate>.Create(governorates, totalCount, page, pageSize);
            return Result<PaginatedResult<Governorate>>.Ok("GOVERNORATES_RETRIEVED_SUCCESS", paginatedResult);
        }
        catch (Exception ex)
        {
            var emptyResult = new PaginatedResult<Governorate>();
            return Result<PaginatedResult<Governorate>>.Error("GOVERNORATES_RETRIEVAL_FAILED", emptyResult);
        }
    }

    public async Task<Result<Governorate>> GetByIdAsync(int id, bool includeCities = false)
    {
        try
        {
            var query = _context.Governorates.AsQueryable();

            if (includeCities)
            {
                query = query.Include(g => g.Cities);
            }

            var governorate = await query.FirstOrDefaultAsync(g => g.Id == id);

            if (governorate == null)
            {
                return Result<Governorate>.Error("GOVERNORATE_NOT_FOUND", null!);
            }

            return Result<Governorate>.Ok("GOVERNORATE_RETRIEVED_SUCCESS", governorate);
        }
        catch (Exception ex)
        {
            return Result<Governorate>.Error("GOVERNORATE_RETRIEVAL_FAILED", null!);
        }
    }

    public async Task<Result<Governorate>> CreateAsync(Governorate governorate)
    {
        try
        {
            // Check for duplicate name
            var duplicateExists = await _context.Governorates
                .AnyAsync(g => g.NameAr == governorate.NameAr || g.NameEn == governorate.NameEn);

            if (duplicateExists)
            {
                return Result<Governorate>.Error("GOVERNORATE_NAME_ALREADY_EXISTS", null!);
            }

            governorate.CreatedAt = DateTime.UtcNow;
            governorate.UpdatedAt = DateTime.UtcNow;

            _context.Governorates.Add(governorate);
            await _context.SaveChangesAsync();

            return Result<Governorate>.Ok("GOVERNORATE_CREATED_SUCCESS", governorate);
        }
        catch (Exception ex)
        {
            return Result<Governorate>.Error("GOVERNORATE_CREATION_FAILED", null!);
        }
    }

    public async Task<Result<Governorate>> UpdateAsync(int id, Governorate governorate)
    {
        try
        {
            var existingGovernorate = await _context.Governorates.FirstOrDefaultAsync(g => g.Id == id);
            if (existingGovernorate == null)
            {
                return Result<Governorate>.Error("GOVERNORATE_NOT_FOUND", null!);
            }

            // Check for duplicate name (excluding current governorate)
            var duplicateExists = await _context.Governorates
                .AnyAsync(g => g.Id != id && 
                          (g.NameAr == governorate.NameAr || g.NameEn == governorate.NameEn));

            if (duplicateExists)
            {
                return Result<Governorate>.Error("GOVERNORATE_NAME_ALREADY_EXISTS", null!);
            }

            existingGovernorate.NameAr = governorate.NameAr;
            existingGovernorate.NameEn = governorate.NameEn;
            existingGovernorate.IsActive = governorate.IsActive;
            existingGovernorate.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            return Result<Governorate>.Ok("GOVERNORATE_UPDATED_SUCCESS", existingGovernorate);
        }
        catch (Exception ex)
        {
            return Result<Governorate>.Error("GOVERNORATE_UPDATE_FAILED", null!);
        }
    }

    public async Task<Result> DeleteAsync(int id)
    {
        try
        {
            var governorate = await _context.Governorates
                .Include(g => g.Cities)
                .FirstOrDefaultAsync(g => g.Id == id);

            if (governorate == null)
            {
                return Result.Error("GOVERNORATE_NOT_FOUND");
            }

            // Check if governorate has active cities
            var hasActiveCities = governorate.Cities.Any(c => !c.IsDeleted);
            if (hasActiveCities)
            {
                return Result.Error("GOVERNORATE_HAS_ACTIVE_CITIES");
            }

            // Soft delete
            governorate.IsDeleted = true;
            governorate.DeletedAt = DateTime.UtcNow;
            governorate.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            return Result.Ok("GOVERNORATE_DELETED_SUCCESS");
        }
        catch (Exception ex)
        {
            return Result.Error("GOVERNORATE_DELETION_FAILED");
        }
    }

    public async Task<Result<bool>> ExistsAsync(int id)
    {
        try
        {
            var exists = await _context.Governorates.AnyAsync(g => g.Id == id);
            return Result<bool>.Ok("GOVERNORATE_EXISTS_CHECK_SUCCESS", exists);
        }
        catch (Exception ex)
        {
            return Result<bool>.Error("GOVERNORATE_EXISTS_CHECK_FAILED", false);
        }
    }

    public async Task<Result<bool>> ExistsAsync(string nameAr, string nameEn, int? excludeId = null)
    {
        try
        {
            var query = _context.Governorates.AsQueryable();

            if (excludeId.HasValue)
            {
                query = query.Where(g => g.Id != excludeId.Value);
            }

            var exists = await query.AnyAsync(g => g.NameAr == nameAr || g.NameEn == nameEn);

            return Result<bool>.Ok("GOVERNORATE_EXISTS_CHECK_SUCCESS", exists);
        }
        catch (Exception ex)
        {
            return Result<bool>.Error("GOVERNORATE_EXISTS_CHECK_FAILED", false);
        }
    }

    public async Task<Result<int>> GetCitiesCountAsync(int governorateId)
    {
        try
        {
            var count = await _context.Cities
                .Where(c => c.GovernorateId == governorateId && !c.IsDeleted)
                .CountAsync();

            return Result<int>.Ok("CITIES_COUNT_RETRIEVED_SUCCESS", count);
        }
        catch (Exception ex)
        {
            return Result<int>.Error("CITIES_COUNT_RETRIEVAL_FAILED", 0);
        }
    }
}
