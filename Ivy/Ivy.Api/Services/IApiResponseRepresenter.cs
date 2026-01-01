using IvyBackend;
using Ivy.Api.DTOs;
using Microsoft.AspNetCore.Mvc;

namespace Ivy.Api.Services;

public interface IApiResponseRepresenter
{
    /// <summary>
    /// Creates an ActionResult from a Result object
    /// </summary>
    /// <param name="result">The result object</param>
    /// <param name="language">Optional language override</param>
    /// <returns>ActionResult with appropriate status code and ApiResponse</returns>
    IActionResult CreateResponse(Result result, string? language = null);

    /// <summary>
    /// Creates an ActionResult from a Result&lt;T&gt; object
    /// </summary>
    /// <typeparam name="T">The data type</typeparam>
    /// <param name="result">The result object</param>
    /// <param name="language">Optional language override</param>
    /// <returns>ActionResult with appropriate status code and ApiResponse&lt;T&gt;</returns>
    IActionResult CreateResponse<T>(Result<T> result, string? language = null);

    /// <summary>
    /// Creates a validation error response
    /// </summary>
    /// <typeparam name="T">The data type</typeparam>
    /// <param name="errors">The validation errors</param>
    /// <param name="language">Optional language override</param>
    /// <returns>ActionResult with validation error response</returns>
    IActionResult CreateValidationErrorResponse<T>(object errors, string? language = null);

    /// <summary>
    /// Creates an internal error response
    /// </summary>
    /// <typeparam name="T">The data type</typeparam>
    /// <param name="language">Optional language override</param>
    /// <returns>ActionResult with internal error response</returns>
    IActionResult CreateInternalErrorResponse<T>(string? language = null);

    /// <summary>
    /// Creates an internal error response without data
    /// </summary>
    /// <param name="language">Optional language override</param>
    /// <returns>ActionResult with internal error response</returns>
    IActionResult CreateInternalErrorResponse(string? language = null);
}
