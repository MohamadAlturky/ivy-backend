using Ivy.Contracts.Models;
using Ivy.Core.DataContext;
using Ivy.Core.Entities;
using IvyBackend;
using Microsoft.EntityFrameworkCore;

namespace Ivy.Contracts.Services;

public interface IAdminPermissionService
{
    Task<Result<PaginatedResult<AdminPermissionDto>>> GetAllAsync(
        int page = 1,
        int pageSize = 10,
        string? search = null
    );
    Task<Result<PaginatedResult<AdminPermissionLocalizedDto>>> GetAllLocalizedAsync(
        string language = "en",
        int page = 1,
        int pageSize = 10,
        string? search = null
    );
    Task<Result<List<AdminPermissionDropDownDto>>> DropDownAsync(
        string language = "en",
        string? search = null
    );
    Task<Result<AdminPermissionDto>> CreateAsync(CreateAdminPermissionDto dto);
    Task<Result<AdminPermissionDto>> UpdateAsync(int id, UpdateAdminPermissionDto dto);
    Task<Result> DeleteAsync(int id);
}

public class AdminPermissionService : IAdminPermissionService
{
    private readonly IvyContext _context;

    public AdminPermissionService(IvyContext context)
    {
        _context = context;
    }

    public async Task<Result<PaginatedResult<AdminPermissionDto>>> GetAllAsync(
        int page = 1,
        int pageSize = 10,
        string? search = null
    )
    {
        var query = _context.Set<AdminPermission>().AsQueryable();

        // Filtering
        if (!string.IsNullOrWhiteSpace(search))
        {
            query = query.Where(x =>
                x.NameAr.Contains(search) || x.NameEn.Contains(search) || x.Code.Contains(search)
            );
        }

        // Pagination
        var totalCount = await query.CountAsync();

        var items = await query
            .OrderBy(x => x.Id)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(x => new AdminPermissionDto
            {
                Id = x.Id,
                Code = x.Code,
                NameAr = x.NameAr,
                NameEn = x.NameEn,
                DescriptionAr = x.DescriptionAr,
                DescriptionEn = x.DescriptionEn,
            })
            .ToListAsync();

        var result = PaginatedResult<AdminPermissionDto>.Create(items, totalCount, page, pageSize);

        return Result<PaginatedResult<AdminPermissionDto>>.Ok(
            AdminPermissionServiceMessageCodes.SUCCESS,
            result
        );
    }

    public async Task<Result<AdminPermissionDto>> CreateAsync(CreateAdminPermissionDto dto)
    {
        // Validation
        if (
            string.IsNullOrWhiteSpace(dto.Code)
            || string.IsNullOrWhiteSpace(dto.NameAr)
            || string.IsNullOrWhiteSpace(dto.NameEn)
        )
        {
            return Result<AdminPermissionDto>.Error(
                AdminPermissionServiceMessageCodes.INVALID_DATA,
                null!
            );
        }

        // Check for duplicate names or code
        bool exists = await _context
            .Set<AdminPermission>()
            .AnyAsync(x => x.Code == dto.Code || x.NameAr == dto.NameAr || x.NameEn == dto.NameEn);

        if (exists)
        {
            return Result<AdminPermissionDto>.Error(
                AdminPermissionServiceMessageCodes.DUPLICATE_DATA,
                null!
            );
        }

        var entity = new AdminPermission
        {
            Code = dto.Code,
            NameAr = dto.NameAr,
            NameEn = dto.NameEn,
            DescriptionAr = dto.DescriptionAr,
            DescriptionEn = dto.DescriptionEn,
        };

        await _context.Set<AdminPermission>().AddAsync(entity);
        await _context.SaveChangesAsync();

        // Map back to DTO
        var resultDto = new AdminPermissionDto
        {
            Id = entity.Id,
            Code = entity.Code,
            NameAr = entity.NameAr,
            NameEn = entity.NameEn,
            DescriptionAr = entity.DescriptionAr,
            DescriptionEn = entity.DescriptionEn,
        };

        return Result<AdminPermissionDto>.Ok(AdminPermissionServiceMessageCodes.CREATED, resultDto);
    }

    public async Task<Result<AdminPermissionDto>> UpdateAsync(int id, UpdateAdminPermissionDto dto)
    {
        if (
            string.IsNullOrWhiteSpace(dto.Code)
            || string.IsNullOrWhiteSpace(dto.NameAr)
            || string.IsNullOrWhiteSpace(dto.NameEn)
        )
        {
            return Result<AdminPermissionDto>.Error(
                AdminPermissionServiceMessageCodes.INVALID_DATA,
                null!
            );
        }

        var entity = await _context.Set<AdminPermission>().FindAsync(id);

        if (entity == null)
        {
            return Result<AdminPermissionDto>.Error(
                AdminPermissionServiceMessageCodes.NOT_FOUND,
                null!
            );
        }

        // Check for duplicates (excluding current entity)
        bool exists = await _context
            .Set<AdminPermission>()
            .AnyAsync(x =>
                x.Id != id
                && (x.Code == dto.Code || x.NameAr == dto.NameAr || x.NameEn == dto.NameEn)
            );

        if (exists)
        {
            return Result<AdminPermissionDto>.Error(
                AdminPermissionServiceMessageCodes.DUPLICATE_DATA,
                null!
            );
        }

        // Update fields
        entity.Code = dto.Code;
        entity.NameAr = dto.NameAr;
        entity.NameEn = dto.NameEn;
        entity.DescriptionAr = dto.DescriptionAr;
        entity.DescriptionEn = dto.DescriptionEn;

        _context.Set<AdminPermission>().Update(entity);
        await _context.SaveChangesAsync();

        var resultDto = new AdminPermissionDto
        {
            Id = entity.Id,
            Code = entity.Code,
            NameAr = entity.NameAr,
            NameEn = entity.NameEn,
            DescriptionAr = entity.DescriptionAr,
            DescriptionEn = entity.DescriptionEn,
        };

        return Result<AdminPermissionDto>.Ok(AdminPermissionServiceMessageCodes.UPDATED, resultDto);
    }

    public async Task<Result> DeleteAsync(int id)
    {
        var entity = await _context.Set<AdminPermission>().FindAsync(id);

        if (entity == null)
        {
            return Result.Error(AdminPermissionServiceMessageCodes.NOT_FOUND);
        }

        _context.Set<AdminPermission>().Remove(entity);
        await _context.SaveChangesAsync();

        return Result.Ok(AdminPermissionServiceMessageCodes.DELETED);
    }

    public async Task<Result<PaginatedResult<AdminPermissionLocalizedDto>>> GetAllLocalizedAsync(
        string language = "en",
        int page = 1,
        int pageSize = 10,
        string? search = null
    )
    {
        var query = _context.Set<AdminPermission>().AsQueryable();

        if (!string.IsNullOrWhiteSpace(search))
        {
            query = query.Where(x =>
                x.NameAr.Contains(search) || x.NameEn.Contains(search) || x.Code.Contains(search)
            );
        }

        var totalCount = await query.CountAsync();

        var items = await query
            .OrderBy(x => x.Id)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(x => new AdminPermissionLocalizedDto
            {
                Id = x.Id,
                Code = x.Code,
                Name = language == "ar" ? x.NameAr : x.NameEn,
                Description = language == "ar" ? x.DescriptionAr : x.DescriptionEn,
            })
            .ToListAsync();

        var result = PaginatedResult<AdminPermissionLocalizedDto>.Create(
            items,
            totalCount,
            page,
            pageSize
        );

        return Result<PaginatedResult<AdminPermissionLocalizedDto>>.Ok(
            AdminPermissionServiceMessageCodes.SUCCESS,
            result
        );
    }

    public async Task<Result<List<AdminPermissionDropDownDto>>> DropDownAsync(
        string language = "en",
        string? search = null
    )
    {
        var query = _context.Set<AdminPermission>().AsQueryable();

        if (!string.IsNullOrWhiteSpace(search))
        {
            query = query.Where(x =>
                x.NameAr.Contains(search) || x.NameEn.Contains(search) || x.Code.Contains(search)
            );
        }

        var items = await query
            .OrderBy(x => x.Id)
            .Select(x => new AdminPermissionDropDownDto
            {
                Id = x.Id,
                Code = x.Code,
                Name = language == "ar" ? x.NameAr : x.NameEn,
            })
            .ToListAsync();

        return Result<List<AdminPermissionDropDownDto>>.Ok(
            AdminPermissionServiceMessageCodes.SUCCESS,
            items
        );
    }
}

public static class AdminPermissionServiceMessageCodes
{
    public const string INVALID_DATA = "ADMINPERMISSION_INVALID_DATA";
    public const string DUPLICATE_DATA = "ADMINPERMISSION_DUPLICATE_DATA";
    public const string NOT_FOUND = "ADMINPERMISSION_NOT_FOUND";
    public const string CREATED = "ADMINPERMISSION_CREATED";
    public const string UPDATED = "ADMINPERMISSION_UPDATED";
    public const string DELETED = "ADMINPERMISSION_DELETED";
    public const string SUCCESS = "ADMINPERMISSION_SUCCESS";
}
