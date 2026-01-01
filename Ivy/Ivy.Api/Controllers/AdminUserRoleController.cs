using Ivy.Contracts.Models;
using Ivy.Contracts.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Ivy.Api.Controllers;

[Route("api/admins/roles")]
public class AdminUserRoleController : BaseController
{
    private readonly IAdminUserRoleService _adminUserRoleService;

    public AdminUserRoleController(
        IAdminUserRoleService adminUserRoleService,
        IApiResponseRepresenter responseRepresenter,
        ILogger<AdminUserRoleController> logger
    )
        : base(responseRepresenter, logger)
    {
        _adminUserRoleService = adminUserRoleService;
    }

    [HttpGet("{adminId}")]
    public async Task<IActionResult> GetAdminRoles(int adminId)
    {
        var result = await _adminUserRoleService.GetRolesByAdminIdAsync(adminId);
        return HandleResult(result);
    }
    [HttpGet("my-roles")]
    [Authorize(Roles = "admin")]
    public async Task<IActionResult> GetMyRoles()
    {
        var result = await _adminUserRoleService.GetRolesByAdminIdAsync(GetUserId());
        return HandleResult(result);
    }

    [HttpPut("{adminId}")]
    [Authorize(Roles = "admin")]
    public async Task<IActionResult> UpdateAdminRoles(
        int adminId,
        [FromBody] UpdateAdminUserRolesDto dto
    )
    {
        var result = await _adminUserRoleService.UpdateRolesForAdminAsync(adminId, dto);
        return HandleResult(result);
    }
}