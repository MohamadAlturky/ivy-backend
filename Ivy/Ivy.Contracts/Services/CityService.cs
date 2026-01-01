using Ivy.Contracts.Models;
using Ivy.Contracts.Services;
using Ivy.Core.DataContext;
using Ivy.Core.Entities;
using IvyBackend;
using Microsoft.EntityFrameworkCore;

namespace Ivy.Contracts.Services;

public interface ICityService
{
    Task<Result<PaginatedResult<CityDto>>> GetAllAsync(
        int page = 1,
        int pageSize = 10,
        string? name = null,
        int? governorateId = null,
        bool? isActive = null
    );

    Task<Result<PaginatedResult<CityLocalizedDto>>> GetAllLocalizedAsync(
        string language = "en",
        int page = 1,
        int pageSize = 10,
        string? name = null,
        int? governorateId = null,
        bool? isActive = null
    );
    Task<Result<List<CityDropDownDto>>> DropDownAsync(
        string language = "en",
        string? name = null,
        int? governorateId = null
    );

    Task<Result> ToggleStatusAsync(int id);
    Task<Result<CityDto>> CreateAsync(CreateCityDto dto);
    Task<Result<CityDto>> UpdateAsync(int id, UpdateCityDto dto);
    Task<Result> DeleteAsync(int id);
}

public class CityService : ICityService
{
    private readonly IvyContext _context;

    public CityService(IvyContext context)
    {
        _context = context;
    }

    public async Task<Result<PaginatedResult<CityDto>>> GetAllAsync(
        int page = 1,
        int pageSize = 10,
        string? name = null,
        int? governorateId = null,
        bool? isActive = null
    )
    {
        var query = _context.Set<City>().AsQueryable();

        // Filtering
        if (isActive.HasValue)
        {
            query = query.Where(x => x.IsActive == isActive.Value);
        }

        if (governorateId.HasValue)
        {
            query = query.Where(x => x.GovernorateId == governorateId.Value);
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
            .Select(x => new CityDto
            {
                Id = x.Id,
                NameAr = x.NameAr,
                NameEn = x.NameEn,
                Governorate = new GovernorateDto
                {
                    Id = x.GovernorateId,
                    NameAr = x.Governorate.NameAr,
                    NameEn = x.Governorate.NameEn,
                    IsActive = x.Governorate.IsActive,
                },
                IsActive = x.IsActive,
            })
            .ToListAsync();

        var result = PaginatedResult<CityDto>.Create(items, totalCount, page, pageSize);

        return Result<PaginatedResult<CityDto>>.Ok(CityServiceMessageCodes.SUCCESS, result);
    }

    public async Task<Result<CityDto>> CreateAsync(CreateCityDto dto)
    {
        // Validation
        if (string.IsNullOrWhiteSpace(dto.NameAr) || string.IsNullOrWhiteSpace(dto.NameEn))
        {
            return Result<CityDto>.Error(CityServiceMessageCodes.INVALID_DATA, null!);
        }

        // Check if Governorate exists
        var governorate = await _context
            .Set<Governorate>()
            .FirstOrDefaultAsync(x => x.Id == dto.GovernorateId);
        if (governorate == null)
        {
            return Result<CityDto>.Error(CityServiceMessageCodes.GOVERNORATE_NOT_FOUND, null!);
        }

        // Check for duplicate names (optional: consider checking duplication only within the same Governorate)
        bool exists = await _context
            .Set<City>()
            .AnyAsync(x =>
                (x.NameAr == dto.NameAr || x.NameEn == dto.NameEn)
                && x.GovernorateId == dto.GovernorateId
            );

        if (exists)
        {
            return Result<CityDto>.Error(CityServiceMessageCodes.DUPLICATE_NAME, null!);
        }

        var entity = new City
        {
            NameAr = dto.NameAr,
            NameEn = dto.NameEn,
            GovernorateId = dto.GovernorateId,
        };

        await _context.Set<City>().AddAsync(entity);
        await _context.SaveChangesAsync();

        // Map back to DTO
        var resultDto = new CityDto
        {
            Id = entity.Id,
            NameAr = entity.NameAr,
            NameEn = entity.NameEn,
            Governorate = new GovernorateDto
            {
                Id = entity.GovernorateId,
                NameAr = governorate.NameAr,
                NameEn = governorate.NameEn,
                IsActive = governorate.IsActive,
            },
            IsActive = entity.IsActive,
        };

        return Result<CityDto>.Ok(CityServiceMessageCodes.CREATED, resultDto);
    }

    public async Task<Result<CityDto>> UpdateAsync(int id, UpdateCityDto dto)
    {
        if (string.IsNullOrWhiteSpace(dto.NameAr) || string.IsNullOrWhiteSpace(dto.NameEn))
        {
            return Result<CityDto>.Error(CityServiceMessageCodes.INVALID_DATA, null!);
        }

        var entity = await _context
            .Set<City>()
            .Include(x => x.Governorate)
            .FirstOrDefaultAsync(x => x.Id == id);

        if (entity == null)
        {
            return Result<CityDto>.Error(CityServiceMessageCodes.NOT_FOUND, null!);
        }

        // Validate Governorate if it changed
        if (entity.GovernorateId != dto.GovernorateId)
        {
            var governorate = await _context
                .Set<Governorate>()
                .FirstOrDefaultAsync(x => x.Id == dto.GovernorateId);
            if (governorate == null)
            {
                return Result<CityDto>.Error(CityServiceMessageCodes.GOVERNORATE_NOT_FOUND, null!);
            }
        }

        // Update fields
        entity.NameAr = dto.NameAr;
        entity.NameEn = dto.NameEn;
        entity.GovernorateId = dto.GovernorateId;

        _context.Set<City>().Update(entity);
        await _context.SaveChangesAsync();

        var resultDto = new CityDto
        {
            Id = entity.Id,
            NameAr = entity.NameAr,
            NameEn = entity.NameEn,
            Governorate = new GovernorateDto
            {
                Id = entity.GovernorateId,
                NameAr = entity.Governorate.NameAr,
                NameEn = entity.Governorate.NameEn,
                IsActive = entity.Governorate.IsActive,
            },
            IsActive = entity.IsActive,
        };

        return Result<CityDto>.Ok(CityServiceMessageCodes.UPDATED, resultDto);
    }

    public async Task<Result> DeleteAsync(int id)
    {
        var entity = await _context.Set<City>().FindAsync(id);

        if (entity == null)
        {
            return Result.Error(CityServiceMessageCodes.NOT_FOUND);
        }

        // Hard delete
        _context.Set<City>().Remove(entity);
        await _context.SaveChangesAsync();

        return Result.Ok(CityServiceMessageCodes.DELETED);
    }

    public async Task<Result<PaginatedResult<CityLocalizedDto>>> GetAllLocalizedAsync(
        string language = "en",
        int page = 1,
        int pageSize = 10,
        string? name = null,
        int? governorateId = null,
        bool? isActive = null
    )
    {
        var query = _context.Set<City>().AsQueryable();

        // Filtering
        if (isActive.HasValue)
        {
            query = query.Where(x => x.IsActive == isActive.Value);
        }

        if (governorateId.HasValue)
        {
            query = query.Where(x => x.GovernorateId == governorateId.Value);
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
            .Select(x => new CityLocalizedDto
            {
                Id = x.Id,
                Name = language == "ar" ? x.NameAr : x.NameEn,
                Governorate = new GovernorateLocalizedDto
                {
                    Id = x.GovernorateId,
                    Name = language == "ar" ? x.Governorate.NameAr : x.Governorate.NameEn,
                    IsActive = x.Governorate.IsActive,
                },
                IsActive = x.IsActive,
            })
            .ToListAsync();

        var result = PaginatedResult<CityLocalizedDto>.Create(items, totalCount, page, pageSize);

        return Result<PaginatedResult<CityLocalizedDto>>.Ok(
            CityServiceMessageCodes.SUCCESS,
            result
        );
    }

    public async Task<Result<List<CityDropDownDto>>> DropDownAsync(
        string language = "en",
        string? name = null,
        int? governorateId = null
    )
    {
        var query = _context.Set<City>().AsQueryable();

        // Filtering
        query = query.Where(x => x.IsActive == true);

        if (governorateId.HasValue)
        {
            query = query.Where(x => x.GovernorateId == governorateId.Value);
        }

        if (!string.IsNullOrWhiteSpace(name))
        {
            query = query.Where(x => x.NameAr.Contains(name) || x.NameEn.Contains(name));
        }

        // Pagination
        var totalCount = await query.CountAsync();

        var items = await query
            .OrderBy(x => x.Id)
            .Select(x => new CityDropDownDto
            {
                Id = x.Id,
                Name = language == "ar" ? x.NameAr : x.NameEn,
                Governorate = new GovernorateDropDownDto
                {
                    Id = x.GovernorateId,
                    Name = language == "ar" ? x.Governorate.NameAr : x.Governorate.NameEn,
                },
            })
            .ToListAsync();

        return Result<List<CityDropDownDto>>.Ok(CityServiceMessageCodes.SUCCESS, items);
    }

    public async Task<Result> ToggleStatusAsync(int id)
    {
        var entity = await _context.Set<City>().FindAsync(id);

        if (entity == null)
        {
            return Result.Error(CityServiceMessageCodes.NOT_FOUND);
        }

        entity.IsActive = !entity.IsActive;
        await _context.SaveChangesAsync();

        return Result.Ok(CityServiceMessageCodes.STATUS_UPDATED);
    }
}

public static class CityServiceMessageCodes
{
    public const string INVALID_DATA = "CITY_INVALID_DATA";
    public const string DUPLICATE_NAME = "CITY_DUPLICATE_NAME";
    public const string NOT_FOUND = "CITY_NOT_FOUND";
    public const string GOVERNORATE_NOT_FOUND = "CITY_GOVERNORATE_NOT_FOUND";
    public const string CREATED = "CITY_CREATED";
    public const string UPDATED = "CITY_UPDATED";
    public const string DELETED = "CITY_DELETED";
    public const string SUCCESS = "CITY_SUCCESS";
    public const string STATUS_UPDATED = "CITY_STATUS_UPDATED";
}
