using IvyBackend;

namespace Ivy.Api.DTOs;

public class ApiResponse<T>
{
    public bool Success { get; set; }
    public string MessageCode { get; set; } = string.Empty;
    public string? Message { get; set; }
    public T? Data { get; set; }
    public object? Errors { get; set; }

    public static ApiResponse<T> FromResult(Result<T> result, string? message = null)
    {
        return new ApiResponse<T>
        {
            Success = result.Success,
            MessageCode = result.MessageCode,
            Message = message,
            Data = result.Data,
        };
    }

    public static ApiResponse<T> Ok(T data, string messageCode, string? message = null)
    {
        return new ApiResponse<T>
        {
            Success = true,
            MessageCode = messageCode,
            Message = message,
            Data = data,
        };
    }

    public static ApiResponse<T> Error(
        string messageCode,
        string? message = null,
        object? errors = null
    )
    {
        return new ApiResponse<T>
        {
            Success = false,
            MessageCode = messageCode,
            Message = message,
            Errors = errors,
        };
    }
}

public class ApiResponse
{
    public bool Success { get; set; }
    public string MessageCode { get; set; } = string.Empty;
    public string? Message { get; set; }
    public object? Errors { get; set; }

    public static ApiResponse FromResult(Result result, string? message = null)
    {
        return new ApiResponse
        {
            Success = result.Success,
            MessageCode = result.MessageCode,
            Message = message,
        };
    }

    public static ApiResponse Ok(string messageCode, string? message = null)
    {
        return new ApiResponse
        {
            Success = true,
            MessageCode = messageCode,
            Message = message,
        };
    }

    public static ApiResponse Error(
        string messageCode,
        string? message = null,
        object? errors = null
    )
    {
        return new ApiResponse
        {
            Success = false,
            MessageCode = messageCode,
            Message = message,
            Errors = errors,
        };
    }
}
