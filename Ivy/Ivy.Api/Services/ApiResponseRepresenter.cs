using Ivy.Api.DTOs;
using IvyBackend;
using IvyBackend.Services;
using Microsoft.AspNetCore.Mvc;

namespace Ivy.Api.Services;

public class ApiResponseRepresenter : IApiResponseRepresenter
{
    private readonly IMessageStore _messageStore;
    private readonly IHttpContextAccessor _httpContextAccessor;

    public ApiResponseRepresenter(
        IMessageStore messageStore,
        IHttpContextAccessor httpContextAccessor
    )
    {
        _messageStore = messageStore;
        _httpContextAccessor = httpContextAccessor;
    }

    public IActionResult CreateResponse(Result result, string? language = null)
    {
        var lang = language ?? GetLanguageFromHeader();
        var message = _messageStore.GetMessage(result.MessageCode, lang);

        var apiResponse = new ApiResponse
        {
            Success = result.Success,
            MessageCode = result.MessageCode,
            Message = message,
        };

        // Always return 200 OK as per requirements
        return new OkObjectResult(apiResponse);
    }

    public IActionResult CreateResponse<T>(Result<T> result, string? language = null)
    {
        var lang = language ?? GetLanguageFromHeader();
        var message = _messageStore.GetMessage(result.MessageCode, lang);

        var apiResponse = new ApiResponse<T>
        {
            Success = result.Success,
            MessageCode = result.MessageCode,
            Message = message,
            Data = result.Data,
        };

        // Always return 200 OK as per requirements
        return new OkObjectResult(apiResponse);
    }

    public IActionResult CreateValidationErrorResponse<T>(
        object errors,
        string? language = null
    )
    {
        var lang = language ?? GetLanguageFromHeader();
        var message = _messageStore.GetMessage("VALIDATION_ERROR", lang);

        var apiResponse = new ApiResponse<T>
        {
            Success = false,
            MessageCode = "VALIDATION_ERROR",
            Message = message
        };

        // Always return 200 OK as per requirements
        return new OkObjectResult(apiResponse);
    }

    public IActionResult CreateInternalErrorResponse<T>(string? language = null)
    {
        var lang = language ?? GetLanguageFromHeader();
        var message = _messageStore.GetMessage("INTERNAL_ERROR", lang);

        var apiResponse = new ApiResponse<T>
        {
            Success = false,
            MessageCode = "INTERNAL_ERROR",
            Message = message,
        };

        // Always return 200 OK as per requirements
        return new OkObjectResult(apiResponse);
    }

    public IActionResult CreateInternalErrorResponse(string? language = null)
    {
        var lang = language ?? GetLanguageFromHeader();
        var message = _messageStore.GetMessage("INTERNAL_ERROR", lang);

        var apiResponse = new ApiResponse
        {
            Success = false,
            MessageCode = "INTERNAL_ERROR",
            Message = message,
        };

        // Always return 200 OK as per requirements
        return new OkObjectResult(apiResponse);
    }

    private string GetLanguageFromHeader()
    {
        var httpContext = _httpContextAccessor.HttpContext;
        if (httpContext?.Request.Headers.TryGetValue("x-lang", out var languageHeader) == true)
        {
            var language = languageHeader.FirstOrDefault();
            if (!string.IsNullOrEmpty(language))
            {
                return language.ToLowerInvariant();
            }
        }

        // Default to English if no header found
        return "en";
    }
}
