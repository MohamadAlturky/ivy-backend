using Ivy.Contracts.Models;
using Ivy.Core.DataContext;
using Ivy.Core.Entities;
using IvyBackend;
using Microsoft.EntityFrameworkCore;

namespace Ivy.Contracts.Services;

public interface IAdminRolePermissionService
{
    Task<Result<List<AdminPermissionLocalizedDto>>> GetPermissionsByRoleIdAsync(
        int roleId,
        string language = "en"
    );

    Task<Result> UpdatePermissionsForRoleAsync(int roleId, UpdateRolePermissionsDto dto);
}

public class AdminRolePermissionService : IAdminRolePermissionService
{
    private readonly IvyContext _context;

    public AdminRolePermissionService(IvyContext context)
    {
        _context = context;
    }

    public async Task<Result<List<AdminPermissionLocalizedDto>>> GetPermissionsByRoleIdAsync(
        int roleId,
        string language = "en"
    )
    {
        // Validate Role exists (Optional, depending on UI flow)
        var roleExists = await _context.Set<AdminRole>().AnyAsync(r => r.Id == roleId);
        if (!roleExists)
        {
            return Result<List<AdminPermissionLocalizedDto>>.Error(
                AdminRolePermissionServiceMessageCodes.ROLE_NOT_FOUND,
                null!
            );
        }

        var permissions = await _context
            .Set<RolePermissionsLink>()
            .Where(link => link.RoleId == roleId)
            .Include(link => link.Permission)
            .Select(link => new AdminPermissionLocalizedDto
            {
                Id = link.Permission.Id,
                Code = link.Permission.Code,
                Name = language == "ar" ? link.Permission.NameAr : link.Permission.NameEn,
                Description =
                    language == "ar"
                        ? link.Permission.DescriptionAr
                        : link.Permission.DescriptionEn,
            })
            .ToListAsync();

        return Result<List<AdminPermissionLocalizedDto>>.Ok(
            AdminRolePermissionServiceMessageCodes.SUCCESS,
            permissions
        );
    }

    public async Task<Result> UpdatePermissionsForRoleAsync(
        int roleId,
        UpdateRolePermissionsDto dto
    )
    {
        // 1. Validate Role exists
        var roleExists = await _context.Set<AdminRole>().AnyAsync(x => x.Id == roleId);
        if (!roleExists)
        {
            return Result.Error(AdminRolePermissionServiceMessageCodes.ROLE_NOT_FOUND);
        }

        // 2. Validate all PermissionIds exist
        var distinctPermIds = dto.PermissionIds.Distinct().ToList();

        // If the list is not empty, check if they are valid
        if (distinctPermIds.Any())
        {
            var validPermsCount = await _context
                .Set<AdminPermission>()
                .CountAsync(p => distinctPermIds.Contains(p.Id));

            if (validPermsCount != distinctPermIds.Count)
            {
                return Result.Error(AdminRolePermissionServiceMessageCodes.INVALID_PERMISSION_IDS);
            }
        }

        // 3. Fetch existing links for this role
        var existingLinks = await _context
            .Set<RolePermissionsLink>()
            .Where(x => x.RoleId == roleId)
            .ToListAsync();

        var existingPermIds = existingLinks.Select(x => x.PermissionId).ToList();

        // 4. Determine links to remove (Present in DB but not in DTO)
        var linksToRemove = existingLinks
            .Where(x => !distinctPermIds.Contains(x.PermissionId))
            .ToList();

        // 5. Determine links to add (Present in DTO but not in DB)
        var linksToAdd = distinctPermIds
            .Where(newId => !existingPermIds.Contains(newId))
            .Select(permId => new RolePermissionsLink { RoleId = roleId, PermissionId = permId })
            .ToList();

        // 6. Execute Updates
        if (linksToRemove.Any())
        {
            _context.Set<RolePermissionsLink>().RemoveRange(linksToRemove);
        }

        if (linksToAdd.Any())
        {
            await _context.Set<RolePermissionsLink>().AddRangeAsync(linksToAdd);
        }

        await _context.SaveChangesAsync();

        return Result.Ok(AdminRolePermissionServiceMessageCodes.UPDATED);
    }
}

public static class AdminRolePermissionServiceMessageCodes
{
    public const string ROLE_NOT_FOUND = "ROLE_NOT_FOUND";
    public const string INVALID_PERMISSION_IDS = "INVALID_PERMISSION_IDS";
    public const string UPDATED = "UPDATED";
    public const string SUCCESS = "SUCCESS";
}
