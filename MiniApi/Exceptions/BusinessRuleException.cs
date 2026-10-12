namespace MiniApi.Exceptions;

// Violación de una regla de negocio. El status depende del caso (409, 422, 401, 403).
public class BusinessRuleException(string errorCode, string message, int statusCode = StatusCodes.Status409Conflict)
    : Exception(message)
{
    public string ErrorCode { get; } = errorCode;
    public int StatusCode { get; } = statusCode;
}
