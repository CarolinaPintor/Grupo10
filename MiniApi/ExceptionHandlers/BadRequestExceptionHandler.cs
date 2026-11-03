using System.Text.Json;
using Microsoft.AspNetCore.Diagnostics;
using MiniApi.Exceptions;

namespace MiniApi.ExceptionHandlers;

// JSON malformado, tipos incorrectos o cuerpo vacío: son errores del cliente, no del servidor (400).
public class BadRequestExceptionHandler(ILogger<BadRequestExceptionHandler> logger) : IExceptionHandler
{
    public async ValueTask<bool> TryHandleAsync(
        HttpContext context, Exception exception, CancellationToken cancellationToken)
    {
        if (exception is not (BadHttpRequestException or JsonException)) return false;

        var errorCode = ObtenerErrorCode(context.Request.Path);
        logger.LogWarning("{ErrorCode} Solicitud mal formada: {Message}", errorCode, exception.Message);
        await ErrorResponseWriter.WriteAsync(
            context, StatusCodes.Status400BadRequest, errorCode,
            "El cuerpo de la solicitud es inválido o tiene un formato incorrecto.", cancellationToken);
        return true;
    }

    private static string ObtenerErrorCode(PathString path)
    {
        if (path.StartsWithSegments("/api/products")) return ErrorCodes.PRD_002;
        if (path.StartsWithSegments("/api/users")) return ErrorCodes.USR_002;
        if (path.StartsWithSegments("/api/orders")) return ErrorCodes.ORD_002;
        if (path.StartsWithSegments("/api/cart")) return ErrorCodes.CRT_004;
        if (path.StartsWithSegments("/api/notifications")) return ErrorCodes.NTF_002;
        return "ERR-400";
    }
}
