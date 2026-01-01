using Ivy.Contracts.Models;
using Ivy.Contracts.Services;
using Microsoft.AspNetCore.Mvc;

namespace Ivy.Api.Controllers;

[Route("api/admin-roles")]
public class AdminRoleController : BaseController
{
    private readonly IAdminRoleService _adminRoleService;

    public AdminRoleController(
        IAdminRoleService adminRoleService,
        IApiResponseRepresenter responseRepresenter,
        ILogger<AdminRoleController> logger
    )
        : base(responseRepresenter, logger)
    {
        _adminRoleService = adminRoleService;
    }

    [HttpGet]
    public async Task<IActionResult> GetAllAdminRoles(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 10,
        [FromQuery] string? name = null
    )
    {
        var result = await _adminRoleService.GetAllAsync(
            page: page,
            pageSize: pageSize,
            name: name
        );
        return HandleResult(result);
    }

    [HttpGet("localized")]
    public async Task<IActionResult> GetAllLocalizedAdminRoles(
        [FromQuery] string language = "en",
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 10,
        [FromQuery] string? name = null
    )
    {
        var result = await _adminRoleService.GetAllLocalizedAsync(
            language: language,
            page: page,
            pageSize: pageSize,
            name: name
        );
        return HandleResult(result);
    }

    [HttpPost]
    public async Task<IActionResult> CreateAdminRole(
        [FromBody] CreateAdminRoleDto createAdminRoleDto
    )
    {
        var result = await _adminRoleService.CreateAsync(createAdminRoleDto);
        return HandleResult(result);
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateAdminRole(
        int id,
        [FromBody] UpdateAdminRoleDto updateAdminRoleDto
    )
    {
        var result = await _adminRoleService.UpdateAsync(id, updateAdminRoleDto);
        return HandleResult(result);
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteAdminRole(int id)
    {
        var result = await _adminRoleService.DeleteAsync(id);
        return HandleResult(result);
    }

    [HttpGet("dropdown")]
    public async Task<IActionResult> GetDropdownAdminRoles(
        [FromQuery] string? name = null
    )
    {
        var result = await _adminRoleService.DropDownAsync(GetLanguage(), name: name);
        return HandleResult(result);
    }
}