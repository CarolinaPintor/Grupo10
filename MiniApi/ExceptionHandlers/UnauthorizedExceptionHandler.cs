using Microsoft.AspNetCore.Diagnostics;
using MiniApi.Exceptions;

namespace MiniApi.ExceptionHandlers;

public class UnauthorizedExceptionHandler : IExceptionHandler
{
    public async ValueTask<bool> TryHandleAsync(HttpContext context, Exception exception, CancellationToken cancellationToken)
    {
        // Si no es nuestra excepción 401, la dejamos pasar al siguiente manejador
        if (exception is not UnauthorizedException unauthorizedEx)
        {
            return false;
        }

        // Si es nuestra, la atrapamos y usamos el formateador 
        await ErrorResponseWriter.WriteAsync(
            context,
            StatusCodes.Status401Unauthorized,
            unauthorizedEx.ErrorCode,
            unauthorizedEx.Message,
            cancellationToken);

        return true; // Le avisamos a .NET que ya manejamos el error
    }
}