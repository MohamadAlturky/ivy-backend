using Ivy.Contracts.Models;
using Ivy.Core.DataContext;
using Ivy.Core.Entities;
using IvyBackend;
using Microsoft.EntityFrameworkCore;

namespace Ivy.Contracts.Services;

public interface IGovernorateService
{
    Task<Result<PaginatedResult<GovernorateDto>>> GetAllAsync(
        int page = 1,
        int pageSize = 10,
        string? name = null,
        bool? isActive = null
    );
    Task<Result<PaginatedResult<GovernorateLocalizedDto>>> GetAllLocalizedAsync(
        string language = "en",
        int page = 1,
        int pageSize = 10,
        string? name = null,
        bool? isActive = null
    );
    Task<Result<List<GovernorateDropDownDto>>> DropDownAsync(
        string language = "en",
        string? name = null
    );
    Task<Result<GovernorateDto>> CreateAsync(CreateGovernorateDto dto);
    Task<Result<GovernorateDto>> UpdateAsync(int id, UpdateGovernorateDto dto);
    Task<Result> DeleteAsync(int id);
    Task<Result> ToggleStatusAsync(int id);
}

public class GovernorateService : IGovernorateService
{
    private readonly IvyContext _context;

    public GovernorateService(IvyContext context)
    {
        _context = context;
    }

    public async Task<Result<PaginatedResult<GovernorateDto>>> GetAllAsync(
        int page = 1,
        int pageSize = 10,
        string? name = null,
        bool? isActive = null
    )
    {
        var query = _context.Set<Governorate>().AsQueryable();

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
            .Select(x => new GovernorateDto
            {
                Id = x.Id,
                NameAr = x.NameAr,
                NameEn = x.NameEn,
                IsActive = x.IsActive,
            })
            .ToListAsync();

        var result = PaginatedResult<GovernorateDto>.Create(items, totalCount, page, pageSize);

        return Result<PaginatedResult<GovernorateDto>>.Ok(
            GovernorateServiceMessageCodes.SUCCESS,
            result
        );
    }

    public async Task<Result<GovernorateDto>> CreateAsync(CreateGovernorateDto dto)
    {
        // Validation
        if (string.IsNullOrWhiteSpace(dto.NameAr) || string.IsNullOrWhiteSpace(dto.NameEn))
        {
            return Result<GovernorateDto>.Error(GovernorateServiceMessageCodes.INVALID_DATA, null!);
        }

        // Check for duplicate names
        bool exists = await _context
            .Set<Governorate>()
            .AnyAsync(x => x.NameAr == dto.NameAr || x.NameEn == dto.NameEn);

        if (exists)
        {
            return Result<GovernorateDto>.Error(
                GovernorateServiceMessageCodes.DUPLICATE_NAME,
                null!
            );
        }

        var entity = new Governorate
        {
            NameAr = dto.NameAr,
            NameEn = dto.NameEn,
            IsActive = dto.IsActive,
        };

        await _context.Set<Governorate>().AddAsync(entity);
        await _context.SaveChangesAsync();

        // Map back to DTO
        var resultDto = new GovernorateDto
        {
            Id = entity.Id,
            NameAr = entity.NameAr,
            NameEn = entity.NameEn,
            IsActive = entity.IsActive,
        };

        return Result<GovernorateDto>.Ok(GovernorateServiceMessageCodes.CREATED, resultDto);
    }

    public async Task<Result<GovernorateDto>> UpdateAsync(int id, UpdateGovernorateDto dto)
    {
        if (string.IsNullOrWhiteSpace(dto.NameAr) || string.IsNullOrWhiteSpace(dto.NameEn))
        {
            return Result<GovernorateDto>.Error(GovernorateServiceMessageCodes.INVALID_DATA, null!);
        }

        var entity = await _context.Set<Governorate>().FindAsync(id);

        if (entity == null)
        {
            return Result<GovernorateDto>.Error(GovernorateServiceMessageCodes.NOT_FOUND, null!);
        }

        // Update fields
        entity.NameAr = dto.NameAr;
        entity.NameEn = dto.NameEn;
        entity.IsActive = dto.IsActive;

        _context.Set<Governorate>().Update(entity);
        await _context.SaveChangesAsync();

        var resultDto = new GovernorateDto
        {
            Id = entity.Id,
            NameAr = entity.NameAr,
            NameEn = entity.NameEn,
            IsActive = entity.IsActive,
        };

        return Result<GovernorateDto>.Ok(GovernorateServiceMessageCodes.UPDATED, resultDto);
    }

    public async Task<Result> DeleteAsync(int id)
    {
        var entity = await _context.Set<Governorate>().FindAsync(id);

        if (entity == null)
        {
            return Result.Error(GovernorateServiceMessageCodes.NOT_FOUND);
        }

        // Hard delete
        _context.Set<Governorate>().Remove(entity);
        await _context.SaveChangesAsync();

        return Result.Ok(GovernorateServiceMessageCodes.DELETED);
    }

    public async Task<Result<PaginatedResult<GovernorateLocalizedDto>>> GetAllLocalizedAsync(
        string language = "en",
        int page = 1,
        int pageSize = 10,
        string? name = null,
        bool? isActive = null
    )
    {
        var query = _context.Set<Governorate>().AsQueryable();

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
            .Select(x => new GovernorateLocalizedDto
            {
                Id = x.Id,
                Name = language == "ar" ? x.NameAr : x.NameEn,
                IsActive = x.IsActive,
            })
            .ToListAsync();

        var result = PaginatedResult<GovernorateLocalizedDto>.Create(
            items,
            totalCount,
            page,
            pageSize
        );

        return Result<PaginatedResult<GovernorateLocalizedDto>>.Ok(
            GovernorateServiceMessageCodes.SUCCESS,
            result
        );
    }

    public async Task<Result<List<GovernorateDropDownDto>>> DropDownAsync(
        string language = "en",
        string? name = null
    )
    {
        var query = _context.Set<Governorate>().AsQueryable();

        // Filtering
        query = query.Where(x => x.IsActive == true);

        if (!string.IsNullOrWhiteSpace(name))
        {
            query = query.Where(x => x.NameAr.Contains(name) || x.NameEn.Contains(name));
        }

        // Pagination
        var totalCount = await query.CountAsync();

        var items = await query
            .OrderBy(x => x.Id)
            .Select(x => new GovernorateDropDownDto
            {
                Id = x.Id,
                Name = language == "ar" ? x.NameAr : x.NameEn,
            })
            .ToListAsync();

        return Result<List<GovernorateDropDownDto>>.Ok(
            GovernorateServiceMessageCodes.SUCCESS,
            items
        );
    }

    public async Task<Result> ToggleStatusAsync(int id)
    {
        var entity = await _context.Set<Governorate>().FindAsync(id);

        if (entity == null)
        {
            return Result.Error(GovernorateServiceMessageCodes.NOT_FOUND);
        }

        entity.IsActive = !entity.IsActive;
        await _context.SaveChangesAsync();

        return Result.Ok(GovernorateServiceMessageCodes.STATUS_UPDATED);
    }
}

public static class GovernorateServiceMessageCodes
{
    public const string INVALID_DATA = "GOVERNORATE_INVALID_DATA";
    public const string DUPLICATE_NAME = "GOVERNORATE_DUPLICATE_NAME";
    public const string NOT_FOUND = "GOVERNORATE_NOT_FOUND";
    public const string CREATED = "GOVERNORATE_CREATED";
    public const string UPDATED = "GOVERNORATE_UPDATED";
    public const string DELETED = "GOVERNORATE_DELETED";
    public const string SUCCESS = "GOVERNORATE_SUCCESS";
    public const string STATUS_UPDATED = "GOVERNORATE_STATUS_UPDATED";
}
