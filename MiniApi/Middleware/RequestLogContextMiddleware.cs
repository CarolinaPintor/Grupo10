using Serilog.Context;

namespace MiniApi.Middleware
{
    // Agrega el Endpoint (método + ruta) a todos los logs del request.
    public class RequestLogContextMiddleware
    {
        private readonly RequestDelegate _next;

        public RequestLogContextMiddleware(RequestDelegate next) => _next = next;

        public async Task InvokeAsync(HttpContext context)
        {
            var endpoint = $"{context.Request.Method} {context.Request.Path}";

            using (LogContext.PushProperty("Endpoint", endpoint))
            {
                await _next(context);
            }
        }
    }
}
