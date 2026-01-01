using Ivy.Contracts.Models;
using Ivy.Contracts.Services;
using Microsoft.AspNetCore.Mvc;

namespace Ivy.Api.Controllers;

[Route("api/admin-permissions")]
public class AdminPermissionController : BaseController
{
    private readonly IAdminPermissionService _adminPermissionService;

    public AdminPermissionController(
        IAdminPermissionService adminPermissionService,
        IApiResponseRepresenter responseRepresenter,
        ILogger<AdminPermissionController> logger
    )
        : base(responseRepresenter, logger)
    {
        _adminPermissionService = adminPermissionService;
    }

    [HttpGet]
    public async Task<IActionResult> GetAllAdminPermissions(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 10,
        [FromQuery] string? search = null
    )
    {
        var result = await _adminPermissionService.GetAllAsync(
            page: page,
            pageSize: pageSize,
            search: search
        );
        return HandleResult(result);
    }

    [HttpGet("localized")]
    public async Task<IActionResult> GetAllLocalizedAdminPermissions(
        [FromQuery] string language = "en",
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 10,
        [FromQuery] string? search = null
    )
    {
        var result = await _adminPermissionService.GetAllLocalizedAsync(
            language: language,
            page: page,
            pageSize: pageSize,
            search: search
        );
        return HandleResult(result);
    }

    [HttpPost]
    public async Task<IActionResult> CreateAdminPermission(
        [FromBody] CreateAdminPermissionDto createDto
    )
    {
        var result = await _adminPermissionService.CreateAsync(createDto);
        return HandleResult(result);
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateAdminPermission(
        int id,
        [FromBody] UpdateAdminPermissionDto updateDto
    )
    {
        var result = await _adminPermissionService.UpdateAsync(id, updateDto);
        return HandleResult(result);
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteAdminPermission(int id)
    {
        var result = await _adminPermissionService.DeleteAsync(id);
        return HandleResult(result);
    }

    [HttpGet("dropdown")]
    public async Task<IActionResult> GetDropdownAdminPermissions(
        [FromQuery] string? search = null
    )
    {
        var result = await _adminPermissionService.DropDownAsync(GetLanguage(), search: search);
        return HandleResult(result);
    }
}