namespace erp.minepress.frameworks.Common;

public class ApiResponse<T>
{
    public bool Success { get; set; }
    public string? Message { get; set; }
    public T? Data { get; set; }
    public IReadOnlyList<string>? Errors { get; set; }
    public int StatusCode { get; set; }

    public static ApiResponse<T> Ok(T data, string? message = null) => new()
    {
        Success = true,
        Data = data,
        Message = message ?? "Success",
        StatusCode = 200
    };

    public static ApiResponse<T> Created(T data, string? message = null) => new()
    {
        Success = true,
        Data = data,
        Message = message ?? "Created",
        StatusCode = 201
    };

    public static ApiResponse<T> Error(string message, int statusCode = 400) => new()
    {
        Success = false,
        Message = message,
        StatusCode = statusCode
    };

    public static ApiResponse<T> Error(IReadOnlyList<string> errors, int statusCode = 400) => new()
    {
        Success = false,
        Message = errors.FirstOrDefault(),
        Errors = errors,
        StatusCode = statusCode
    };

    public static ApiResponse<T> NotFound(string? message = null) => new()
    {
        Success = false,
        Message = message ?? "Not found",
        StatusCode = 404
    };
}
