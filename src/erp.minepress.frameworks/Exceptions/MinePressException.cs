namespace erp.minepress.frameworks.Exceptions;

public class MinePressException : Exception
{
    public string ErrorCode { get; }
    public int StatusCode { get; }

    public MinePressException(string message, string errorCode = "GENERAL_ERROR", int statusCode = 400)
        : base(message)
    {
        ErrorCode = errorCode;
        StatusCode = statusCode;
    }
}

public class NotFoundException : MinePressException
{
    public NotFoundException(string entityName, object key)
        : base($"{entityName} with key '{key}' was not found.", "NOT_FOUND", 404)
    {
    }
}

public class ValidationException : MinePressException
{
    public IReadOnlyList<string> ValidationErrors { get; }

    public ValidationException(IReadOnlyList<string> errors)
        : base(errors.FirstOrDefault() ?? "Validation failed.", "VALIDATION_ERROR", 422)
    {
        ValidationErrors = errors;
    }

    public ValidationException(string error)
        : this([error])
    {
    }
}

public class UnauthorizedException : MinePressException
{
    public UnauthorizedException(string message = "Unauthorized access.")
        : base(message, "UNAUTHORIZED", 401)
    {
    }
}
