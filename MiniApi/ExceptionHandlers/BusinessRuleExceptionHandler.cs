using Microsoft.AspNetCore.Diagnostics;
using MiniApi.Exceptions;

namespace MiniApi.ExceptionHandlers;

public class BusinessRuleExceptionHandler(ILogger<BusinessRuleExceptionHandler> logger) : IExceptionHandler
{
    public async ValueTask<bool> TryHandleAsync(
        HttpContext context, Exception exception, CancellationToken cancellationToken)
    {
        if (exception is not BusinessRuleException ex) return false;

        logger.LogWarning("{ErrorCode} {Message}", ex.ErrorCode, ex.Message);
        await ErrorResponseWriter.WriteAsync(
            context, ex.StatusCode, ex.ErrorCode, ex.Message, cancellationToken);
        return true;
    }
}
