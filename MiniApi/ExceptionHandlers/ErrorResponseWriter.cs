namespace MiniApi.ExceptionHandlers;

// Arma la respuesta de error con el formato del contrato (ProblemDetails + errorCode y errorMessage).
public static class ErrorResponseWriter
{
    public static async ValueTask WriteAsync(
        HttpContext context, int statusCode, string errorCode, string errorMessage,
        CancellationToken cancellationToken)
    {
        var (type, title, detail) = statusCode switch
        {
            StatusCodes.Status400BadRequest => ("https://tools.ietf.org/html/rfc7231#section-6.5.1", "Bad Request", "La solicitud no es válida."),
            StatusCodes.Status401Unauthorized => ("https://tools.ietf.org/html/rfc7235#section-3.1", "Unauthorized", "Las credenciales no son válidas."),
            StatusCodes.Status403Forbidden => ("https://tools.ietf.org/html/rfc7231#section-6.5.3", "Forbidden", "El acceso está prohibido."),
            StatusCodes.Status404NotFound => ("https://tools.ietf.org/html/rfc7231#section-6.5.4", "Not Found", "El recurso solicitado no fue encontrado."),
            StatusCodes.Status409Conflict => ("https://tools.ietf.org/html/rfc7231#section-6.5.8", "Conflict", "No se puede completar la operación."),
            StatusCodes.Status422UnprocessableEntity => ("https://tools.ietf.org/html/rfc4918#section-11.2", "Unprocessable Entity", "No se puede procesar la solicitud."),
            _ => ("https://tools.ietf.org/html/rfc7231#section-6.6.1", "Internal Server Error", "Ocurrió un error interno.")
        };

        context.Response.StatusCode = statusCode;
        await context.Response.WriteAsJsonAsync(new
        {
            type,
            title,
            status = statusCode,
            detail,
            instance = context.Request.Path.Value,
            errorCode,
            errorMessage
        }, options: null, contentType: "application/problem+json", cancellationToken: cancellationToken);
    }
}
