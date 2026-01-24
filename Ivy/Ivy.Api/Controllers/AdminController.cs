using Ivy.Contracts.Models;
using Ivy.Contracts.Services;
using Microsoft.AspNetCore.Authorization;

namespace Ivy.Api.Controllers;

[Route("api/admin")]
public class AdminController : BaseController
{
    private readonly IAdminService _adminService;

    public AdminController(
        IAdminService adminService,
        IApiResponseRepresenter responseRepresenter,
        ILogger<AdminController> logger
    )
        : base(responseRepresenter, logger)
    {
        _adminService = adminService;
    }

    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] AdminLoginDto loginDto)
    {
        var result = await _adminService.LoginAsync(loginDto, GetLanguage());
        return HandleResult(result);
    }

    [HttpGet("profile")]
    [Authorize(Roles = "admin")]
    public async Task<IActionResult> GetProfile()
    {
        var result = await _adminService.GetProfileAsync(GetUserId(), GetLanguage());
        return HandleResult(result);
    }

    [HttpPut("change-password")]
    [Authorize(Roles = "admin")]
    public async Task<IActionResult> ChangePassword([FromBody] ChangePasswordDto changePasswordDto)
    {
        var result = await _adminService.ChangePasswordAsync(
            GetUserId(),
            changePasswordDto,
            GetLanguage()
        );
        return HandleResult(result);
    }
}
