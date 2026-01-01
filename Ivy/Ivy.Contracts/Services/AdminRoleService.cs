using Ivy.Contracts.Models;
using Ivy.Core.DataContext;
using Ivy.Core.Entities;
using IvyBackend;
using Microsoft.EntityFrameworkCore;

namespace Ivy.Contracts.Services;

public interface IAdminRoleService
{
    Task<Result<PaginatedResult<AdminRoleDto>>> GetAllAsync(
        int page = 1,
        int pageSize = 10,
        string? name = null
    );
    Task<Result<PaginatedResult<AdminRoleLocalizedDto>>> GetAllLocalizedAsync(
        string language = "en",
        int page = 1,
        int pageSize = 10,
        string? name = null
    );
    Task<Result<List<AdminRoleDropDownDto>>> DropDownAsync(
        string language = "en",
        string? name = null
    );
    Task<Result<AdminRoleDto>> CreateAsync(CreateAdminRoleDto dto);
    Task<Result<AdminRoleDto>> UpdateAsync(int id, UpdateAdminRoleDto dto);
    Task<Result> DeleteAsync(int id);
}

public class AdminRoleService : IAdminRoleService
{
    private readonly IvyContext _context;

    public AdminRoleService(IvyContext context)
    {
        _context = context;
    }

    public async Task<Result<PaginatedResult<AdminRoleDto>>> GetAllAsync(
        int page = 1,
        int pageSize = 10,
        string? name = null
    )
    {
        var query = _context.Set<AdminRole>().AsQueryable();

        // Filtering
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
            .Select(x => new AdminRoleDto
            {
                Id = x.Id,
                NameAr = x.NameAr,
                NameEn = x.NameEn,
                DescriptionAr = x.DescriptionAr,
                DescriptionEn = x.DescriptionEn,
            })
            .ToListAsync();

        var result = PaginatedResult<AdminRoleDto>.Create(items, totalCount, page, pageSize);

        return Result<PaginatedResult<AdminRoleDto>>.Ok(
            AdminRoleServiceMessageCodes.SUCCESS,
            result
        );
    }

    public async Task<Result<AdminRoleDto>> CreateAsync(CreateAdminRoleDto dto)
    {
        // Validation
        if (string.IsNullOrWhiteSpace(dto.NameAr) || string.IsNullOrWhiteSpace(dto.NameEn))
        {
            return Result<AdminRoleDto>.Error(AdminRoleServiceMessageCodes.INVALID_DATA, null!);
        }

        // Check for duplicate names
        bool exists = await _context
            .Set<AdminRole>()
            .AnyAsync(x => x.NameAr == dto.NameAr || x.NameEn == dto.NameEn);

        if (exists)
        {
            return Result<AdminRoleDto>.Error(AdminRoleServiceMessageCodes.DUPLICATE_NAME, null!);
        }

        var entity = new AdminRole
        {
            NameAr = dto.NameAr,
            NameEn = dto.NameEn,
            DescriptionAr = dto.DescriptionAr,
            DescriptionEn = dto.DescriptionEn,
        };

        await _context.Set<AdminRole>().AddAsync(entity);
        await _context.SaveChangesAsync();

        // Map back to DTO
        var resultDto = new AdminRoleDto
        {
            Id = entity.Id,
            NameAr = entity.NameAr,
            NameEn = entity.NameEn,
            DescriptionAr = entity.DescriptionAr,
            DescriptionEn = entity.DescriptionEn,
        };

        return Result<AdminRoleDto>.Ok(AdminRoleServiceMessageCodes.CREATED, resultDto);
    }

    public async Task<Result<AdminRoleDto>> UpdateAsync(int id, UpdateAdminRoleDto dto)
    {
        if (string.IsNullOrWhiteSpace(dto.NameAr) || string.IsNullOrWhiteSpace(dto.NameEn))
        {
            return Result<AdminRoleDto>.Error(AdminRoleServiceMessageCodes.INVALID_DATA, null!);
        }

        var entity = await _context.Set<AdminRole>().FindAsync(id);

        if (entity == null)
        {
            return Result<AdminRoleDto>.Error(AdminRoleServiceMessageCodes.NOT_FOUND, null!);
        }

        // Update fields
        entity.NameAr = dto.NameAr;
        entity.NameEn = dto.NameEn;
        entity.DescriptionAr = dto.DescriptionAr;
        entity.DescriptionEn = dto.DescriptionEn;

        _context.Set<AdminRole>().Update(entity);
        await _context.SaveChangesAsync();

        var resultDto = new AdminRoleDto
        {
            Id = entity.Id,
            NameAr = entity.NameAr,
            NameEn = entity.NameEn,
            DescriptionAr = entity.DescriptionAr,
            DescriptionEn = entity.DescriptionEn,
        };

        return Result<AdminRoleDto>.Ok(AdminRoleServiceMessageCodes.UPDATED, resultDto);
    }

    public async Task<Result> DeleteAsync(int id)
    {
        var entity = await _context.Set<AdminRole>().FindAsync(id);

        if (entity == null)
        {
            return Result.Error(AdminRoleServiceMessageCodes.NOT_FOUND);
        }

        // Hard delete
        _context.Set<AdminRole>().Remove(entity);
        await _context.SaveChangesAsync();

        return Result.Ok(AdminRoleServiceMessageCodes.DELETED);
    }

    public async Task<Result<PaginatedResult<AdminRoleLocalizedDto>>> GetAllLocalizedAsync(
        string language = "en",
        int page = 1,
        int pageSize = 10,
        string? name = null
    )
    {
        var query = _context.Set<AdminRole>().AsQueryable();

        // Filtering
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
            .Select(x => new AdminRoleLocalizedDto
            {
                Id = x.Id,
                Name = language == "ar" ? x.NameAr : x.NameEn,
                Description = language == "ar" ? x.DescriptionAr : x.DescriptionEn,
            })
            .ToListAsync();

        var result = PaginatedResult<AdminRoleLocalizedDto>.Create(
            items,
            totalCount,
            page,
            pageSize
        );

        return Result<PaginatedResult<AdminRoleLocalizedDto>>.Ok(
            AdminRoleServiceMessageCodes.SUCCESS,
            result
        );
    }

    public async Task<Result<List<AdminRoleDropDownDto>>> DropDownAsync(
        string language = "en",
        string? name = null
    )
    {
        var query = _context.Set<AdminRole>().AsQueryable();

        // Note: Removed IsActive filter as it is not explicitly in the AdminRole entity provided

        if (!string.IsNullOrWhiteSpace(name))
        {
            query = query.Where(x => x.NameAr.Contains(name) || x.NameEn.Contains(name));
        }

        var items = await query
            .OrderBy(x => x.Id)
            .Select(x => new AdminRoleDropDownDto
            {
                Id = x.Id,
                Name = language == "ar" ? x.NameAr : x.NameEn,
            })
            .ToListAsync();

        return Result<List<AdminRoleDropDownDto>>.Ok(AdminRoleServiceMessageCodes.SUCCESS, items);
    }
}

public static class AdminRoleServiceMessageCodes
{
    public const string INVALID_DATA = "ADMINROLE_INVALID_DATA";
    public const string DUPLICATE_NAME = "ADMINROLE_DUPLICATE_NAME";
    public const string NOT_FOUND = "ADMINROLE_NOT_FOUND";
    public const string CREATED = "ADMINROLE_CREATED";
    public const string UPDATED = "ADMINROLE_UPDATED";
    public const string DELETED = "ADMINROLE_DELETED";
    public const string SUCCESS = "ADMINROLE_SUCCESS";
}
