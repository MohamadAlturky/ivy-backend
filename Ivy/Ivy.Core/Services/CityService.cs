using Ivy.Core.DataContext;
using Ivy.Core.Entities;
using IvyBackend;
using Microsoft.EntityFrameworkCore;

namespace Ivy.Core.Services;

public class CityService : ICityService
{
    private readonly IvyContext _context;

    public CityService(IvyContext context)
    {
        _context = context;
    }

    public async Task<Result<PaginatedResult<City>>> GetAllAsync(
        int page = 1,
        int pageSize = 10,
        string? nameAr = null,
        string? nameEn = null,
        int? governorateId = null,
        bool? isActive = null
    )
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

            var query = _context.Cities.Include(c => c.Governorate).AsQueryable();

            // Apply filters
            if (!string.IsNullOrWhiteSpace(nameAr))
            {
                query = query.Where(c => c.NameAr.Contains(nameAr));
            }

            if (!string.IsNullOrWhiteSpace(nameEn))
            {
                query = query.Where(c => c.NameEn.Contains(nameEn));
            }

            if (governorateId.HasValue)
            {
                query = query.Where(c => c.GovernorateId == governorateId.Value);
            }

            if (isActive.HasValue)
            {
                query = query.Where(c => c.IsActive == isActive.Value);
            }

            // Get total count before pagination
            var totalCount = await query.CountAsync();

            // Apply pagination and ordering
            var cities = await query
                .OrderBy(c => c.NameEn)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            var paginatedResult = PaginatedResult<City>.Create(cities, totalCount, page, pageSize);
            return Result<PaginatedResult<City>>.Ok("CITIES_RETRIEVED_SUCCESS", paginatedResult);
        }
        catch (Exception _)
        {
            var emptyResult = new PaginatedResult<City>();
            return Result<PaginatedResult<City>>.Error("CITIES_RETRIEVAL_FAILED", emptyResult);
        }
    }

    public async Task<Result<City>> GetByIdAsync(int id)
    {
        try
        {
            var city = await _context
                .Cities.Include(c => c.Governorate)
                .FirstOrDefaultAsync(c => c.Id == id);

            if (city == null)
            {
                return Result<City>.Error("CITY_NOT_FOUND", null!);
            }

            return Result<City>.Ok("CITY_RETRIEVED_SUCCESS", city);
        }
        catch (Exception _)
        {
            return Result<City>.Error("CITY_RETRIEVAL_FAILED", null!);
        }
    }

    public async Task<Result<City>> CreateAsync(City city)
    {
        try
        {
            // Check if governorate exists
            var governorateExists = await _context.Governorates.AnyAsync(g =>
                g.Id == city.GovernorateId
            );
            if (!governorateExists)
            {
                return Result<City>.Error("GOVERNORATE_NOT_FOUND", null!);
            }

            // Check for duplicate name within the same governorate
            var duplicateExists = await _context.Cities.AnyAsync(c =>
                (c.NameAr == city.NameAr || c.NameEn == city.NameEn)
                && c.GovernorateId == city.GovernorateId
            );

            if (duplicateExists)
            {
                return Result<City>.Error("CITY_NAME_ALREADY_EXISTS", null!);
            }

            city.CreatedAt = DateTime.UtcNow;
            city.UpdatedAt = DateTime.UtcNow;

            _context.Cities.Add(city);
            await _context.SaveChangesAsync();

            // Load the created city with its governorate
            var createdCity = await _context
                .Cities.Include(c => c.Governorate)
                .FirstAsync(c => c.Id == city.Id);

            return Result<City>.Ok("CITY_CREATED_SUCCESS", createdCity);
        }
        catch (Exception _)
        {
            return Result<City>.Error("CITY_CREATION_FAILED", null!);
        }
    }

    public async Task<Result<City>> UpdateAsync(int id, City city)
    {
        try
        {
            var existingCity = await _context.Cities.FirstOrDefaultAsync(c => c.Id == id);
            if (existingCity == null)
            {
                return Result<City>.Error("CITY_NOT_FOUND", null!);
            }

            // Check if governorate exists
            var governorateExists = await _context.Governorates.AnyAsync(g =>
                g.Id == city.GovernorateId
            );
            if (!governorateExists)
            {
                return Result<City>.Error("GOVERNORATE_NOT_FOUND", null!);
            }

            // Check for duplicate name within the same governorate (excluding current city)
            var duplicateExists = await _context.Cities.AnyAsync(c =>
                c.Id != id
                && (c.NameAr == city.NameAr || c.NameEn == city.NameEn)
                && c.GovernorateId == city.GovernorateId
            );

            if (duplicateExists)
            {
                return Result<City>.Error("CITY_NAME_ALREADY_EXISTS", null!);
            }

            existingCity.NameAr = city.NameAr;
            existingCity.NameEn = city.NameEn;
            existingCity.GovernorateId = city.GovernorateId;
            existingCity.IsActive = city.IsActive;
            existingCity.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            // Load the updated city with its governorate
            var updatedCity = await _context
                .Cities.Include(c => c.Governorate)
                .FirstAsync(c => c.Id == id);

            return Result<City>.Ok("CITY_UPDATED_SUCCESS", updatedCity);
        }
        catch (Exception _)
        {
            return Result<City>.Error("CITY_UPDATE_FAILED", null!);
        }
    }

    public async Task<Result> DeleteAsync(int id)
    {
        try
        {
            var city = await _context.Cities.FirstOrDefaultAsync(c => c.Id == id);
            if (city == null)
            {
                return Result.Error("CITY_NOT_FOUND");
            }

            // Soft delete
            city.IsDeleted = true;
            city.DeletedAt = DateTime.UtcNow;
            city.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            return Result.Ok("CITY_DELETED_SUCCESS");
        }
        catch (Exception _)
        {
            return Result.Error("CITY_DELETION_FAILED");
        }
    }

    public async Task<Result<IEnumerable<City>>> GetByGovernorateIdAsync(int governorateId)
    {
        try
        {
            var cities = await _context
                .Cities.Include(c => c.Governorate)
                .Where(c => c.GovernorateId == governorateId)
                .OrderBy(c => c.NameEn)
                .ToListAsync();

            return Result<IEnumerable<City>>.Ok("CITIES_RETRIEVED_SUCCESS", cities);
        }
        catch (Exception _)
        {
            return Result<IEnumerable<City>>.Error(
                "CITIES_RETRIEVAL_FAILED",
                Enumerable.Empty<City>()
            );
        }
    }

    public async Task<Result<bool>> ExistsAsync(int id)
    {
        try
        {
            var exists = await _context.Cities.AnyAsync(c => c.Id == id);
            return Result<bool>.Ok("CITY_EXISTS_CHECK_SUCCESS", exists);
        }
        catch (Exception _)
        {
            return Result<bool>.Error("CITY_EXISTS_CHECK_FAILED", false);
        }
    }

    public async Task<Result<bool>> ExistsAsync(
        string nameAr,
        string nameEn,
        int governorateId,
        int? excludeId = null
    )
    {
        try
        {
            var query = _context.Cities.AsQueryable();

            if (excludeId.HasValue)
            {
                query = query.Where(c => c.Id != excludeId.Value);
            }

            var exists = await query.AnyAsync(c =>
                (c.NameAr == nameAr || c.NameEn == nameEn) && c.GovernorateId == governorateId
            );

            return Result<bool>.Ok("CITY_EXISTS_CHECK_SUCCESS", exists);
        }
        catch (Exception _)
        {
            return Result<bool>.Error("CITY_EXISTS_CHECK_FAILED", false);
        }
    }
}
