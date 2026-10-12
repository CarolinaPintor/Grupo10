using Microsoft.AspNetCore.Diagnostics;
using MiniApi.Exceptions;

namespace MiniApi.ExceptionHandlers;

// Red de seguridad: cualquier error inesperado termina acá como 500, sin stack trace.
public class GlobalExceptionHandler(ILogger<GlobalExceptionHandler> logger, IHostEnvironment env) : IExceptionHandler
{
    public async ValueTask<bool> TryHandleAsync(
        HttpContext context, Exception exception, CancellationToken cancellationToken)
    {
        var errorCode = ObtenerErrorCode(context.Request.Path);
        logger.LogError(exception, "{ErrorCode} Error no controlado", errorCode);

        // En Development se muestra el mensaje real; en Production uno genérico.
        var message = env.IsDevelopment() ? exception.Message : "Error interno al procesar la solicitud.";

        await ErrorResponseWriter.WriteAsync(
            context, StatusCodes.Status500InternalServerError, errorCode, message, cancellationToken);
        return true;
    }

    private static string ObtenerErrorCode(PathString path)
    {
        if (path.StartsWithSegments("/api/products")) return ErrorCodes.PRD_005;
        if (path.StartsWithSegments("/api/users")) return ErrorCodes.USR_006;
        if (path.StartsWithSegments("/api/orders")) return ErrorCodes.ORD_007;
        if (path.StartsWithSegments("/api/cart")) return ErrorCodes.CRT_005;
        if (path.StartsWithSegments("/api/notifications")) return ErrorCodes.NTF_004;
        return "ERR-500";
    }
}
