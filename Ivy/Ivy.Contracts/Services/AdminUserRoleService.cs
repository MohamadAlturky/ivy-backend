using Ivy.Contracts.Models;
using Ivy.Core.DataContext;
using Ivy.Core.Entities;
using IvyBackend; // Assuming Result<T> is here based on previous context
using Microsoft.EntityFrameworkCore;

namespace Ivy.Contracts.Services;

public interface IAdminUserRoleService
{
    Task<Result<List<AdminRoleLocalizedDto>>> GetRolesByAdminIdAsync(
        int adminId,
        string language = "en"
    );
    Task<Result> UpdateRolesForAdminAsync(int adminId, UpdateAdminUserRolesDto dto);
}

public class AdminUserRoleService : IAdminUserRoleService
{
    private readonly IvyContext _context;

    public AdminUserRoleService(IvyContext context)
    {
        _context = context;
    }

    public async Task<Result<List<AdminRoleLocalizedDto>>> GetRolesByAdminIdAsync(
        int adminId,
        string language = "en"
    )
    {
        var roles = await _context
            .Set<AdminRolesLink>()
            .Where(link => link.AdminId == adminId)
            .Include(link => link.Role)
            .Select(link => new AdminRoleLocalizedDto
            {
                Id = link.Role.Id,
                Name = language == "ar" ? link.Role.NameAr : link.Role.NameEn,
                Description = language == "ar" ? link.Role.DescriptionAr : link.Role.DescriptionEn,
            })
            .ToListAsync();

        return Result<List<AdminRoleLocalizedDto>>.Ok(AdminRoleServiceMessageCodes.SUCCESS, roles);
    }

    public async Task<Result> UpdateRolesForAdminAsync(int adminId, UpdateAdminUserRolesDto dto)
    {
        // 1. Validate Admin exists (Optional but recommended)
        var adminExists = await _context.Set<Admin>().AnyAsync(x => x.Id == adminId);
        if (!adminExists)
        {
            return Result.Error("ADMIN_NOT_FOUND");
        }

        // 2. Validate all RoleIds exist
        var distinctRoleIds = dto.RoleIds.Distinct().ToList();
        var validRolesCount = await _context
            .Set<AdminRole>()
            .CountAsync(r => distinctRoleIds.Contains(r.Id));

        if (validRolesCount != distinctRoleIds.Count)
        {
            return Result.Error(AdminRoleServiceMessageCodes.NOT_FOUND); // Or a specific "INVALID_ROLE_IDS"
        }

        // 3. Fetch existing links
        var existingLinks = await _context
            .Set<AdminRolesLink>()
            .Where(x => x.AdminId == adminId)
            .ToListAsync();

        // 4. Determine links to remove and add
        var existingRoleIds = existingLinks.Select(x => x.RoleId).ToList();

        // Roles to Remove: present in DB but not in DTO
        var linksToRemove = existingLinks.Where(x => !distinctRoleIds.Contains(x.RoleId)).ToList();

        // Roles to Add: present in DTO but not in DB
        var rolesToAdd = distinctRoleIds
            .Where(newId => !existingRoleIds.Contains(newId))
            .Select(roleId => new AdminRolesLink { AdminId = adminId, RoleId = roleId })
            .ToList();

        if (linksToRemove.Any())
        {
            _context.Set<AdminRolesLink>().RemoveRange(linksToRemove);
        }

        if (rolesToAdd.Any())
        {
            await _context.Set<AdminRolesLink>().AddRangeAsync(rolesToAdd);
        }

        await _context.SaveChangesAsync();

        return Result.Ok(AdminRoleServiceMessageCodes.UPDATED);
    }
}
