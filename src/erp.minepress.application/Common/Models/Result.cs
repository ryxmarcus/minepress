namespace erp.minepress.application.Common.Models;

public class Result<T>
{
    public bool IsSuccess { get; }
    public T? Data { get; }
    public string? ErrorMessage { get; }
    public IReadOnlyList<string> Errors { get; }

    private Result(bool isSuccess, T? data, string? errorMessage, IReadOnlyList<string>? errors)
    {
        IsSuccess = isSuccess;
        Data = data;
        ErrorMessage = errorMessage;
        Errors = errors ?? [];
    }

    public static Result<T> Success(T data) => new(true, data, null, null);
    public static Result<T> Failure(string errorMessage) => new(false, default, errorMessage, [errorMessage]);
    public static Result<T> Failure(IReadOnlyList<string> errors) => new(false, default, errors.FirstOrDefault(), errors);
}

public class Result
{
    public bool IsSuccess { get; }
    public string? ErrorMessage { get; }
    public IReadOnlyList<string> Errors { get; }

    private Result(bool isSuccess, string? errorMessage, IReadOnlyList<string>? errors)
    {
        IsSuccess = isSuccess;
        ErrorMessage = errorMessage;
        Errors = errors ?? [];
    }

    public static Result Success() => new(true, null, null);
    public static Result Failure(string errorMessage) => new(false, errorMessage, [errorMessage]);
    public static Result Failure(IReadOnlyList<string> errors) => new(false, errors.FirstOrDefault(), errors);
}
