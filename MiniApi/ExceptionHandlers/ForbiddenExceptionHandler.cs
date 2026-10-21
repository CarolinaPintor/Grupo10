using Microsoft.AspNetCore.Diagnostics;
using MiniApi.Exceptions;

namespace MiniApi.ExceptionHandlers;

public class ForbiddenExceptionHandler : IExceptionHandler
{
    public async ValueTask<bool> TryHandleAsync(HttpContext context, Exception exception, CancellationToken cancellationToken)
    {
        // Si no es nuestra excepción 403, la dejamos pasar
        if (exception is not ForbiddenException forbiddenEx)
        {
            return false;
        }

        // Si es nuestra, la atrapamos y mandamos 403
        await ErrorResponseWriter.WriteAsync(
            context,
            StatusCodes.Status403Forbidden,
            forbiddenEx.ErrorCode,
            forbiddenEx.Message,
            cancellationToken);

        return true;
    }
}