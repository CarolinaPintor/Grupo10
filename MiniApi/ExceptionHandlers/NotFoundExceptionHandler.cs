using Microsoft.AspNetCore.Diagnostics;
using MiniApi.Exceptions;

namespace MiniApi.ExceptionHandlers;

public class NotFoundExceptionHandler(ILogger<NotFoundExceptionHandler> logger) : IExceptionHandler
{
    public async ValueTask<bool> TryHandleAsync(
        HttpContext context, Exception exception, CancellationToken cancellationToken)
    {
        if (exception is not NotFoundException ex) return false;

        logger.LogWarning("{ErrorCode} {Message}", ex.ErrorCode, ex.Message);
        await ErrorResponseWriter.WriteAsync(
            context, StatusCodes.Status404NotFound, ex.ErrorCode, ex.Message, cancellationToken);
        return true;
    }
}
