using System.Security.Claims;
using Ivy.Api.DTOs;
using Ivy.Api.Services;
using Ivy.Core.DataContext;
using IvyBackend;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace Ivy.Api.Controllers;

[ApiController]
public abstract class BaseController : ControllerBase
{
    protected readonly IApiResponseRepresenter _responseRepresenter;
    protected readonly ILogger _logger;

    protected BaseController(IApiResponseRepresenter responseRepresenter, ILogger logger)
    {
        _responseRepresenter = responseRepresenter;
        _logger = logger;
    }

    /// <summary>
    /// Handles a Result object and returns an appropriate ActionResult
    /// </summary>
    /// <param name="result">The result object</param>
    /// <param name="language">Optional language override</param>
    /// <returns>ActionResult with proper response</returns>
    protected IActionResult HandleResult(Result result, string? language = null)
    {
        return _responseRepresenter.CreateResponse(result, language);
    }

    /// <summary>
    /// Handles a Result&lt;T&gt; object and returns an appropriate ActionResult
    /// </summary>
    /// <typeparam name="T">The data type</typeparam>
    /// <param name="result">The result object</param>
    /// <param name="language">Optional language override</param>
    /// <returns>ActionResult with proper response</returns>
    protected IActionResult HandleResult<T>(
        Result<T> result,
        string? language = null
    )
    {
        return _responseRepresenter.CreateResponse(result, language);
    }

    /// <summary>
    /// Handles validation errors
    /// </summary>
    /// <typeparam name="T">The data type</typeparam>
    /// <param name="language">Optional language override</param>
    /// <returns>ActionResult with validation error response</returns>
    protected IActionResult HandleValidationError<T>(string? language = null)
    {
        return _responseRepresenter.CreateValidationErrorResponse<T>(ModelState, language);
    }

    /// <summary>
    /// Handles validation errors without data type
    /// </summary>
    /// <param name="language">Optional language override</param>
    /// <returns>ActionResult with validation error response</returns>
    protected IActionResult HandleValidationError(string? language = null)
    {
        var response = _responseRepresenter.CreateValidationErrorResponse<object>(
            ModelState,
            language
        );
        return new BadRequestObjectResult(response);
    }

    /// <summary>
    /// Handles internal server errors
    /// </summary>
    /// <typeparam name="T">The data type</typeparam>
    /// <param name="exception">The exception that occurred</param>
    /// <param name="operationName">Name of the operation for logging</param>
    /// <param name="language">Optional language override</param>
    /// <returns>ActionResult with internal error response</returns>
    protected IActionResult HandleInternalError<T>(
        Exception exception,
        string operationName,
        string? language = null
    )
    {
        _logger.LogError(exception, "Error occurred while {OperationName}", operationName);
        return _responseRepresenter.CreateInternalErrorResponse<T>(language);
    }

    /// <summary>
    /// Handles internal server errors without data type
    /// </summary>
    /// <param name="exception">The exception that occurred</param>
    /// <param name="operationName">Name of the operation for logging</param>
    /// <param name="language">Optional language override</param>
    /// <returns>ActionResult with internal error response</returns>
    protected IActionResult HandleInternalError(
        Exception exception,
        string operationName,
        string? language = null
    )
    {
        _logger.LogError(exception, "Error occurred while {OperationName}", operationName);
        return _responseRepresenter.CreateInternalErrorResponse(language);
    }

    /// <summary>
    /// Validates model state and returns true if valid, false if invalid
    /// </summary>
    /// <returns>True if model state is valid</returns>
    protected bool IsModelValid()
    {
        return ModelState.IsValid;
    }

    /// <summary>
    /// Gets the language from the x-lang header or returns default
    /// </summary>
    /// <returns>Language code</returns>
    protected string GetLanguage()
    {
        if (Request.Headers.TryGetValue("x-lang", out var languageHeader))
        {
            var language = languageHeader.FirstOrDefault();
            if (!string.IsNullOrEmpty(language))
            {
                return language.ToLowerInvariant();
            }
        }
        return "en";
    }

    protected int GetUserId()
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrWhiteSpace(userIdClaim) || !int.TryParse(userIdClaim, out int userId))
        {
            return 0;
        }
        return userId;
    }

    [NonAction]
    public async Task<int?> GetClinicId(IvyContext context)
    {
        var adminId = GetUserId();
        var admin = await context.Set<Admin>().FindAsync(adminId);
        if (admin == null)
        {
            return null;
        }
        return admin.ClinicId;
    }
}
