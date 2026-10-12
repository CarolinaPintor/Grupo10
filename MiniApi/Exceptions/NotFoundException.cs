namespace MiniApi.Exceptions;

// Recurso inexistente (HTTP 404).
public class NotFoundException(string errorCode, string message) : Exception(message)
{
    public string ErrorCode { get; } = errorCode;
}
