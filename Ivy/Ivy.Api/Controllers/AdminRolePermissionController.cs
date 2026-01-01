using Ivy.Contracts.Models;
using Ivy.Contracts.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Ivy.Api.Controllers;

[Route("api/admin-roles/permissions")]
public class AdminRolePermissionController : BaseController
{
    private readonly IAdminRolePermissionService _rolePermissionService;

    public AdminRolePermissionController(
        IAdminRolePermissionService rolePermissionService,
        IApiResponseRepresenter responseRepresenter,
        ILogger<AdminRolePermissionController> logger
    )
        : base(responseRepresenter, logger)
    {
        _rolePermissionService = rolePermissionService;
    }

    [HttpGet("{roleId}")]
    [Authorize(Roles = "admin")]
    public async Task<IActionResult> GetRolePermissions(int roleId)
    {
        var result = await _rolePermissionService.GetPermissionsByRoleIdAsync(roleId);
        return HandleResult(result);
    }

    [HttpPut("{roleId}")]
    [Authorize(Roles = "admin")]
    public async Task<IActionResult> UpdateRolePermissions(
        int roleId,
        [FromBody] UpdateRolePermissionsDto dto
    )
    {
        var result = await _rolePermissionService.UpdatePermissionsForRoleAsync(roleId, dto);
        return HandleResult(result);
    }
}