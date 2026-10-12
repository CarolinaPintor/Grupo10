namespace MiniApi.Exceptions;

// Datos de entrada inválidos (HTTP 400). Junta todos los problemas en un solo mensaje.
public class ValidationException(string errorCode, IEnumerable<string> errors)
    : Exception(string.Join("; ", errors))
{
    public string ErrorCode { get; } = errorCode;
}
